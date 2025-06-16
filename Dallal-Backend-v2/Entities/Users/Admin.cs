namespace Dallal_Backend_v2.Entities.Users;

public class Admin
{
    private Admin() { }

    public Admin(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; set; }
    public User User { get; set; }
}
