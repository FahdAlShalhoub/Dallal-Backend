namespace Dallal_Backend_v2.Entities;

public class Area
{
    public Guid Id { get; set; }
    public Area? Parent { get; set; }
    public List<Area> Children { get; set; } = [];
    public LocalizedString Name { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public LocalizedString FullName
    {
        get
        {
            if (Parent == null)
                return Name;

            var fullName = new LocalizedString();
            foreach (var kvp in Name.Values)
            {
                var language = kvp.Key;
                var nameValue = kvp.Value;
                var parentValue = Parent.FullName.Values.TryGetValue(language, out var parentName)
                    ? parentName
                    : Parent.FullName.GetValue(language);

                fullName.SetValue(language, $"{parentValue} - {nameValue}");
            }

            return fullName;
        }
        set { }
    }
}
