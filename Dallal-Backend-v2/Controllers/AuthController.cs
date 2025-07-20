using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Dallal_Backend_v2.Controllers.Auth.Dtos;
using Dallal_Backend_v2.Controllers.Common.Dtos;
using Dallal_Backend_v2.Entities;
using Dallal_Backend_v2.Entities.Enums;
using Dallal_Backend_v2.Entities.Users;
using Dallal_Backend_v2.Exceptions;
using Dallal_Backend_v2.Repositories.Users;
using Dallal_Backend_v2.Services;
using Dallal_Backend_v2.ThirdParty;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dallal_Backend_v2.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(
    FirebaseTokenVerifier _firebaseTokenVerifier,
    JwtService _jwtService,
    IUserRepository _userRepository,
    IConfiguration _configuration,
    S3 _s3Service
) : DallalController
{
    [HttpPost("oauth")]
    public async Task<AuthenticatedUserDto> OAuth([FromBody] OAuthRequest request)
    {
        try
        {
            var firebaseToken = await _firebaseTokenVerifier.VerifyIdTokenAsync(request.idToken);
            var emailClaim = firebaseToken.Claims.GetValueOrDefault("email");

            if (emailClaim is not string email)
            {
                throw new Exception("Invalid Email Claim From Firebase Token");
            }

            firebaseToken.Claims.TryGetValue("picture", out object? image);
            firebaseToken.Claims.TryGetValue("given_name", out object? givenName);
            firebaseToken.Claims.TryGetValue("family_name", out object? familyName);
            User user = await GetOrCreateUser(
                email,
                image as string,
                givenName as string,
                familyName as string,
                request.UserType,
                request.PreferredLanguage,
                validatePassword: false
            );

            return await CreateToken(user);
        }
        catch (FirebaseAuthException)
        {
            throw new ArgumentException();
        }
    }

    private async Task<User> GetOrCreateUser(
        string email,
        string? image,
        string? givenName,
        string? familyName,
        UserType userType,
        string preferredLanguage,
        string? password = null,
        bool validatePassword = true
    )
    {
        var existingUser = await _userRepository.GetUserByEmailWithRolesAsync(email);

        // Handle default values for first and last name
        string? firstName = givenName?.Trim();
        string? lastName = familyName?.Trim();

        if (existingUser is null)
        {
            existingUser = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                Password =
                    password == null ? "oauth-password" : BCrypt.Net.BCrypt.HashPassword(password),
                FirstName = firstName,
                LastName = lastName,
                ProfileImage = image != null ? new Document(image) : null,
                PreferredLanguage = preferredLanguage,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                DeletedAt = null,
                LoginAttempts = 0,
                LockoutUntil = null,
            };
            await _userRepository.AddAsync(existingUser);
        }
        else if (validatePassword)
        {
            await VerifyPassword(password, existingUser);
        }

        return await CreateSubProfile(userType, existingUser);
    }

    private async Task<User> CreateSubProfile(UserType userType, User existingUser)
    {
        switch (userType)
        {
            case UserType.Buyer:
            {
                if (existingUser.Buyer is not null)
                    return existingUser;
                var buyer = new Buyer(existingUser.Id);
                existingUser.AddBuyer(buyer);
                return existingUser;
            }
            case UserType.Broker:
            {
                if (existingUser.Broker is not null)
                    return existingUser;
                var broker = new Broker(existingUser.Id) { Status = BrokerStatus.MissingData };
                existingUser.AddBroker(broker);
                return existingUser;
            }
            case UserType.Admin:
            {
                if (existingUser.Admin is not null)
                    return existingUser;
                throw new UnauthorizedAccessException("Cannot register as an Admin");
            }
            default:
                throw new UnauthorizedAccessException("Invalid User Type");
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthenticatedUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<AuthenticatedUserDto> Login([FromBody] LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null)
            throw new UnauthorizedAccessException("Invalid Email or Password");

        if (user.LockoutUntil != null && user.LockoutUntil > DateTime.UtcNow)
        {
            throw new TooManyAttemptsException(
                $"Account is locked until {user.LockoutUntil.Value}"
            );
        }

        await VerifyPassword(request.Password, user);

        user.LoginAttempts = 0;
        user.LockoutUntil = null;
        await _userRepository.UpdateAsync(user);

        return await CreateToken(user);
    }

    [HttpPost("signup")]
    [ProducesResponseType(typeof(AuthenticatedUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<AuthenticatedUserDto> Signup([FromBody] SignupRequest request)
    {
        try
        {
            User user = await GetOrCreateUser(
                email: request.Email,
                image: request.ProfileImage,
                givenName: request.FirstName,
                familyName: request.LastName,
                userType: request.UserType,
                preferredLanguage: request.PreferredLanguage,
                password: request.Password,
                validatePassword: true
            );

            return await CreateToken(user);
        }
        catch (Exception ex)
            when (ex.Message.Contains("duplicate key value violates unique constraint"))
        {
            throw new BadHttpRequestException("Email already in use");
        }
    }

    [HttpPost("authorized-signup")]
    [Authorize()]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<AuthenticatedUserDto> AuthorizedSignup(UserType userType)
    {
        var userId = UserId;
        var user = await _userRepository.GetUserByIdWithRolesAsync(userId);

        await CreateSubProfile(userType, user);
        return await CreateToken(user);
    }

    [HttpPut("info")]
    [Authorize]
    public async Task<UserInfoDto> UpdateLanguage([FromBody] UpdateUserRequest request)
    {
        if (request.Language is null && request.FirstName is null && request.LastName is null)
        {
            throw new BadHttpRequestException(
                "Must provide at least one property to update for the user info"
            );
        }

        var user =
            await _userRepository.GetUserByIdAsync(UserId)
            ?? throw new Exception("User not found");

        if (request.Language != null)
            user.PreferredLanguage = request.Language;

        if (request.FirstName != null)
            user.FirstName = request.FirstName;

        if (request.LastName != null)
            user.LastName = request.LastName;

        await _userRepository.UpdateAsync(user);
        return await GenerateUserInfoDto(user);
    }

    private async Task<AuthenticatedUserDto> CreateToken(User user)
    {
        List<Claim> claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(
                JwtRegisteredClaimNames.Iss,
                _configuration["JWT:Issuer"] ?? throw new Exception("JWT Issuer not found")
            ),
        ];

        if (user.Buyer is not null)
            claims.Add(new Claim(ClaimTypes.Role, UserType.Buyer.ToString()));
        if (user.Broker is not null)
            claims.Add(new Claim(ClaimTypes.Role, UserType.Broker.ToString()));
        if (user.Admin is not null)
            claims.Add(new Claim(ClaimTypes.Role, UserType.Admin.ToString()));

        return new AuthenticatedUserDto
        {
            AccessToken = _jwtService.GenerateToken(claims),
            User = await GenerateUserInfoDto(user),
        };
    }

    private async Task<UserInfoDto> GenerateUserInfoDto(User user)
    {
        return new UserInfoDto
        {
            Image = await _s3Service.CreateDocumentDto(user.ProfileImage),
            FirstName = user.FirstName!,
            LastName = user.LastName!,
            Email = user.Email!,
            Phone = user.Phone,
            Roles = user.Roles,
            PreferredLanguage = user.PreferredLanguage,
        };
    }

    private async Task VerifyPassword(string? password, User user)
    {
        if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            user.LoginAttempts++;
            if (user.LoginAttempts >= 3)
            {
                user.LockoutUntil = DateTime.UtcNow.AddMinutes(15);
            }
            await _userRepository.UpdateAsync(user);
            throw new UnauthorizedAccessException("Invalid Email or Password");
        }
    }
}
