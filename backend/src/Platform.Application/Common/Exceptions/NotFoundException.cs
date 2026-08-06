namespace Platform.Application.Common.Exceptions;

public sealed class NotFoundException : Exception
{
    public NotFoundException()
        : base("Requested resource was not found.")
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.")
    {
    }
}
