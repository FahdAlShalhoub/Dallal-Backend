namespace Dallal_Backend_v2.Exceptions;

public class TooManyAttemptsException : Exception
{
    public TooManyAttemptsException(string? message)
        : base(message) { }
}
