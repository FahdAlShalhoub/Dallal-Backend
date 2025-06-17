using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Dallal_Backend_v2.Controllers.Dtos;
using Dallal_Backend_v2.Entities.Enums;
using Dallal_Backend_v2.Entities.Users;
using Dallal_Backend_v2.Services;
using Dallal_Backend_v2.ThirdParty;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dallal_Backend_v2.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(
    FirebaseTokenVerifier _firebaseTokenVerifier,
    JwtService _jwtService,
    DatabaseContext _context,
    IConfiguration _configuration
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

            return CreateToken(user);
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
        var existingUser = await _context
            .Users.Include(i => i.Buyer)
            .Include(i => i.Broker)
            .Include(i => i.Admin)
            .SingleOrDefaultAsync(x => x.Email == email);

        // Handle default values for first and last name
        string firstName = givenName?.Trim();
        string lastName = familyName?.Trim();

        if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName))
        {
            firstName = email; // Use email as default if no name provided
        }

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
                ProfileImage = image,
                PreferredLanguage = preferredLanguage,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                DeletedAt = null,
                LoginAttempts = 0,
                LockoutUntil = null,
            };
            _context.Users.Add(existingUser);
        }
        else if (validatePassword)
        {
            await VerifyPassword(password, existingUser);
        }

        switch (userType)
        {
            case UserType.Buyer:
            {
                if (existingUser.Buyer is not null)
                    return existingUser;
                var buyer = new Buyer(existingUser.Id);
                existingUser.AddBuyer(buyer);
                await _context.SaveChangesAsync();
                return existingUser;
            }
            case UserType.Broker:
            {
                if (existingUser.Broker is not null)
                    return existingUser;
                var broker = new Broker(existingUser.Id) { Status = BrokerStatus.MissingData };
                existingUser.AddBroker(broker);
                await _context.SaveChangesAsync();
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
    public async Task<AuthenticatedUserDto> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users.SingleOrDefaultAsync(buyer => buyer.Email == request.Email);

        if (user == null)
            throw new UnauthorizedAccessException("Invalid Email or Password");

        if (user.LockoutUntil != null && user.LockoutUntil > DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException(
                $"Account is locked until {user.LockoutUntil.Value}"
            );
        }

        await VerifyPassword(request.Password, user);

        user.LoginAttempts = 0;
        user.LockoutUntil = null;
        await _context.SaveChangesAsync();

        return CreateToken(user);
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

            return CreateToken(user);
        }
        catch (Exception ex)
            when (ex.Message.Contains("duplicate key value violates unique constraint"))
        {
            throw new BadHttpRequestException("Email already in use");
        }
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
            await _context.Users.FirstAsync(i => i.Id == UserId)
            ?? throw new Exception("User not found");

        if (request.Language != null)
            user.PreferredLanguage = request.Language;

        if (request.FirstName != null)
            user.FirstName = request.FirstName;

        if (request.LastName != null)
            user.LastName = request.LastName;

        await _context.SaveChangesAsync();
        return GenerateUserInfoDto(user);
    }

    private AuthenticatedUserDto CreateToken(User user)
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
            User = GenerateUserInfoDto(user),
        };
    }

    private static UserInfoDto GenerateUserInfoDto(User user)
    {
        return new UserInfoDto
        {
            Image = user.ProfileImage!,
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
            await _context.SaveChangesAsync();
            throw new UnauthorizedAccessException("Invalid Email or Password");
        }
    }
}

public record struct UpdateUserRequest(string? FirstName, string? LastName, string? Language);
