namespace PhoeNix.Persistence.Options;

public class DatabaseOptions
{
    public required string Host { get; init; }
    public int Port { get; init; }
    public required string UserName { get; init; }
    public required string Password { get; init; }
    public required string Database { get; init; }
}