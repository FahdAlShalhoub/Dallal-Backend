using System;
using System.Threading.Tasks;
using Dallal.Otps;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace Dallal.Web.Auth;

public class EmailOtpExtensionGrant : AbstractExtensionGrant
{
    public const string ExtensionGrantName = "email_otp";
    public override string Name => ExtensionGrantName;

    protected override async Task<IdentityUser> GetOrAddUser(
        IServiceProvider services,
        string entityType,
        Otp otpObject
    )
    {
        if (otpObject.Email == null)
            throw new InvalidOperationException();

        var user = await services
            .GetRequiredService<IRepository<IdentityUser, Guid>>()
            .FirstOrDefaultAsync(x => x.Email == otpObject.Email);

        if (user == null)
        {
            // TODO CREATE
            throw new NotImplementedException();
        }
        return user;
    }
}
