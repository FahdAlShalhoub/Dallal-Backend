namespace Dallal_Backend_v2.Entities.Users;

public class Admin : BaseEntity
{
    private Admin() { }

    public Admin(Guid id)
    {
        Id = id;
    }
    public User User { get; set; } = default!;
}
