namespace Dallal_Backend_v2.Entities.Users;

public abstract class BaseUser
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Name { get; set; }
    public string Password { get; set; } = default!;
    public string? ProfileImage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? PreferredLanguage { get; set; }
}
