namespace TheKittySaver.AdoptionSystem.Infrastructure.FileStorage;

public sealed class CatFileStorageOptions
{
    public const string SectionName = "CatFileStorage";

    public string BasePath { get; set; } = string.Empty;
}
