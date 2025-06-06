namespace Dallal_Backend_v2.Entities;

public class Area
{
    public Guid Id { get; set; }
    public Area? Parent { get; set; }
    public List<Area> Children { get; set; } = [];
    public LocalizedString Name { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
