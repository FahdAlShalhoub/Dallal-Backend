using System.ComponentModel.DataAnnotations;
using Dallal_Backend_v2.Entities.Enums;

namespace Dallal_Backend_v2.Entities.Users;

public class User : BaseEntity
{
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? PhoneNumber { get; set; }
    public string? FirebaseUid { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Password { get; set; } = default!;
    public Document? ProfileImage { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string PreferredLanguage { get; set; } = default!;
    public int LoginAttempts { get; set; }
    public DateTime? LockoutUntil { get; set; }

    public Buyer? Buyer { get; private set; }
    public Broker? Broker { get; private set; }
    public Admin? Admin { get; private set; }

    public List<UserType> Roles { get; set; } = [];

    public void AddBuyer(Buyer buyer)
    {
        Buyer = buyer;
        Roles.Add(UserType.Buyer);
    }

    public void AddBroker(Broker broker)
    {
        Broker = broker;
        Roles.Add(UserType.Broker);
    }

    public void AddAdmin(Admin admin)
    {
        Admin = admin;
        Roles.Add(UserType.Admin);
    }
}
