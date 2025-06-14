namespace Dallal_Backend_v2.Exceptions;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string message)
        : base(message) { }

    public EntityNotFoundException(Type type, object key)
        : base($"Entity '{type.Name}' with key '{key}' was not found.") { }
}
