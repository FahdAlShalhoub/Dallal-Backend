using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Dallal.Otps;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Volo.Abp;
using Volo.Abp.OpenIddict;
using Volo.Abp.OpenIddict.Applications;
using Volo.Abp.OpenIddict.ExtensionGrantTypes;
using IdentityUser = Volo.Abp.Identity.IdentityUser;
using SignInResult = Microsoft.AspNetCore.Mvc.SignInResult;

namespace Dallal.Web.Auth;

public abstract class AbstractExtensionGrant : ITokenExtensionGrant
{
    public abstract string Name { get; }

    public async Task<IActionResult> HandleAsync(ExtensionGrantContext context)
    {
        try
        {
            return await InnerHandleAsync(context);
        }
        catch (OtpValidationException ex)
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<
                ILogger<AbstractExtensionGrant>
            >();
            logger.LogInformation("Forbid result {code} {ex}", ex.Message, ex.Code);
            return new ForbidResult(
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(
                    new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = ex.Code,
                    }!
                )
            );
        }
        catch (BusinessException ex)
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<
                ILogger<AbstractExtensionGrant>
            >();
            logger.LogInformation("Forbid result {code} {ex}", ex.Message, ex.Code);
            return new ForbidResult(
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(
                    new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants
                            .Errors
                            .InvalidRequest,
                    }!
                )
            );
        }
        catch (Exception ex)
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<
                ILogger<AbstractExtensionGrant>
            >();
            logger.LogError(ex, "An error occurred while handling the extension grant.");
            return new StatusCodeResult(500);
        }
    }

    private async Task<IActionResult> InnerHandleAsync(ExtensionGrantContext context)
    {
        IServiceProvider services = context.HttpContext.RequestServices;
        var logger = services.GetRequiredService<ILogger<AbstractExtensionGrant>>();

        var entityType = context.Request.GetParameter("entity_type").ToString()!;

        Otp otpObject = await ValidateOtp(context, services);

        var userClaimsPrincipalFactory = services.GetRequiredService<
            IUserClaimsPrincipalFactory<IdentityUser>
        >();
        IdentityUser identity = await GetOrAddUser(services, entityType, otpObject);
        var claimsPrincipal = await userClaimsPrincipalFactory.CreateAsync(identity);
        // AddCustomClaims(claimsPrincipal, identity);

        var applicationScopes = await GetApplicationScopes(context);
        var requestedScopes = context.Request.GetScopes();
        var scopes = requestedScopes.Where(i => applicationScopes.Contains(i));
        claimsPrincipal.SetScopes(scopes.ToImmutableArray());
        claimsPrincipal.SetResources(await GetResourcesAsync(context, scopes.ToImmutableArray()));
        claimsPrincipal.SetAudiences("Dallal");
        await services
            .GetRequiredService<AbpOpenIddictClaimsPrincipalManager>()
            .HandleAsync(context.Request, claimsPrincipal);

        return new SignInResult(
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            claimsPrincipal
        );
    }

    protected abstract Task<IdentityUser> GetOrAddUser(
        IServiceProvider services,
        string entityType,
        Otp otpObject
    );

    private static async Task<Otp> ValidateOtp(
        ExtensionGrantContext context,
        IServiceProvider requestServices
    )
    {
        var otpToken = context.Request.GetParameter("token").ToString();
        var otp = context.Request.GetParameter("otp").ToString();

        if (string.IsNullOrWhiteSpace(otpToken) || string.IsNullOrWhiteSpace(otp))
            throw new BusinessException("Phone number or OTP is missing");

        OtpManager otpManager = requestServices.GetRequiredService<OtpManager>();

        Otp reference = await otpManager.ValidateAsync(
            Guid.Parse(otpToken),
            otp,
            OtpReasonEnum.Login
        );
        return reference;
    }

    private async Task<List<string>> GetApplicationScopes(ExtensionGrantContext context)
    {
        var applicationManager =
            context.HttpContext.RequestServices.GetRequiredService<IOpenIddictApplicationManager>();
        var application = await applicationManager.FindByClientIdAsync(
            context.Request.ClientId,
            default
        );

        var client = (application) as OpenIddictApplicationModel;
        var permissions = JsonConvert.DeserializeObject<List<string>>(client.Permissions);
        return
        [
            .. permissions.Where(i => i.StartsWith("scp:")).Select(i => i.Substring(4)),
            "offline_access",
        ];
    }

    private async Task<IEnumerable<string>> GetResourcesAsync(
        ExtensionGrantContext context,
        ImmutableArray<string> scopes
    )
    {
        var resources = new List<string>();
        if (!scopes.Any())
        {
            return resources;
        }

        await foreach (
            var resource in context
                .HttpContext.RequestServices.GetRequiredService<IOpenIddictScopeManager>()
                .ListResourcesAsync(scopes)
        )
        {
            resources.Add(resource);
        }
        return resources;
    }
}
