namespace Dallal_Backend_v2.Entities.Users;

public class Buyer : BaseEntity
{
    private Buyer() { }

    public Buyer(Guid id)
    {
        Id = id;
    }
    public List<Listing> FavoriteListings { get; set; } = [];
}
