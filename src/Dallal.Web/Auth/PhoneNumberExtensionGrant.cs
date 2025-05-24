using System;
using System.Threading.Tasks;
using Dallal.Otps;
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
            .FirstOrDefaultAsync(x => x.PhoneNumber == otpObject.MobileNumber);

        if (user == null)
        {
            // TODO CREATE
            throw new NotImplementedException();
        }
        return user;
    }
}
