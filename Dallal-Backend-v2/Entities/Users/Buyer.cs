namespace Dallal_Backend_v2.Entities.Users;

public class Buyer
{
    private Buyer() { }

    public Buyer(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; set; }
    public List<Listing> FavoriteListings { get; set; } = [];
}
