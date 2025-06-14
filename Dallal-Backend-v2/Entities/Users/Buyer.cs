namespace Dallal_Backend_v2.Entities.Users;

public class Buyer : BaseUser
{
    public List<Listing> FavoriteListings { get; set; } = [];
}
