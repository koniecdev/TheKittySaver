namespace TheKittySaver.AdoptionSystem.API.Settings.ConnectionStringsOption;

internal sealed class ConnectionStrings
{
    public const string ConfigurationSection = nameof(ConnectionStrings);

    public required string Database { get; init; }
}
