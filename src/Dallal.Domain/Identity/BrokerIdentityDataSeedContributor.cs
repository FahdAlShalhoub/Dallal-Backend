using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Uow;

namespace Dallal.Identity;

/* Creates initial broker identity data with three predefined brokers
 * for demonstration and testing purposes.
 */
public class BrokerIdentityDataSeedContributor(
    IRepository<BrokerIdentity, Guid> _brokerRepository,
    UserManager<IdentityUser> _userManager,
    IdentityRoleManager _roleManager
) : IDataSeedContributor, ITransientDependency
{
    // Public constants for broker IDs to be referenced by other seed contributors
    public static readonly Guid BobBuilderId = Guid.Parse("b0b00000-0000-0000-0000-000000000001");
    public static readonly Guid DoraExplorerId = Guid.Parse("d0d00000-0000-0000-0000-000000000002");
    public static readonly Guid AbdoId = Guid.Parse("abcd0000-0000-0000-0000-000000000003");

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        await CreateBrokersAsync();
    }

    private async Task CreateBrokersAsync()
    {
        // Create the brokers
        await CreateBrokerAsync(
            BobBuilderId,
            "bob.builder@dallal.com",
            "Bob",
            "Builder",
            "966123456789"
        );
        await CreateBrokerAsync(
            DoraExplorerId,
            "dora.explorer@dallal.com",
            "Dora",
            "Explorer",
            "966987654321"
        );
        await CreateBrokerAsync(AbdoId, "abdo@dallal.com", "Abdo", "Lost", "966555123456");
    }

    private async Task<BrokerIdentity> CreateBrokerAsync(
        Guid id,
        string email,
        string name,
        string surname,
        string phoneNumber
    )
    {
        // Check if broker already exists
        var existingBroker = await _brokerRepository.FindAsync(id);
        if (existingBroker != null)
        {
            return existingBroker;
        }

        // Create new broker identity
        var broker = new BrokerIdentity(id, email) { Name = name, Surname = surname };

        // Set phone number using the proper method
        broker.SetPhoneNumber(phoneNumber, true);
        broker.SetEmailConfirmed(true);

        // Create the user through UserManager to handle all necessary setup
        var result = await _userManager.CreateAsync(broker, "Password123!");
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to create broker {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}"
            );
        }

        // Assign broker role
        var brokerRole = await _roleManager.FindByNameAsync("Broker");
        if (brokerRole != null)
        {
            await _userManager.AddToRoleAsync(broker, "Broker");
        }

        return broker;
    }
}
