namespace TheKittySaver.AdoptionSystem.API.Extensions;

internal static class EnvironmentsExtensions
{
    public const string LocalName = "Local";
    public const string TestingName = "Testing";
    public const string ProductionName = "Production";
    extension(Environments)
    {
        public static string Local => LocalName;
        public static string Testing => TestingName;
    }
}
