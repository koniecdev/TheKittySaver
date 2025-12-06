namespace TheKittySaver.AdoptionSystem.Persistence.Settings;

public sealed class ConnectionStringSettings
{
    public const string ConfigurationSection = nameof(ConnectionStringSettings);

    public required string Database { get; init; }
}
