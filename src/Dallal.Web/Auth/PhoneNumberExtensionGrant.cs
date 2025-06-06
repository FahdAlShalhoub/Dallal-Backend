using System;
using System.Threading.Tasks;
using Dallal.Identity;
using Dallal.Otps;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace Dallal.Web.Auth;

public class PhoneNumberExtensionGrant : AbstractExtensionGrant
{
    public const string ExtensionGrantName = "phone_number";
    public override string Name => ExtensionGrantName;

    protected override async Task<IdentityUser> GetOrAddUser(
        IServiceProvider services,
        string entityType,
        Otp otpObject
    )
    {
        if (otpObject.MobileNumber == null)
            throw new InvalidOperationException();

        var user = await services
            .GetRequiredService<IRepository<IdentityUser, Guid>>()
            .FirstOrDefaultAsync(x =>
                x.PhoneNumber == otpObject.MobileNumber
                && (
                    (entityType == "broker" && x is BrokerIdentity)
                    || (entityType == "customer" && x is CustomerIdentity)
                )
            );

        if (user == null)
        {
            user = entityType?.ToLower() switch
            {
                "broker" => new BrokerIdentity(Guid.NewGuid(), ""),
                "customer" => new CustomerIdentity(Guid.NewGuid(), ""),
                _ => throw new InvalidOperationException("Invalid entity type"),
            };

            user.SetPhoneNumber(otpObject.MobileNumber, true);
            await services.GetRequiredService<UserManager<IdentityUser>>().CreateAsync(user);
        }
        return user;
    }
}
