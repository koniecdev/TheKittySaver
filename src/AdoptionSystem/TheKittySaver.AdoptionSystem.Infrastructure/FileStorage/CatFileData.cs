namespace TheKittySaver.AdoptionSystem.Infrastructure.FileStorage;

public sealed class CatFileData
{
    public required Stream FileStream { get; init; }
    public required string ContentType { get; init; }
    public required string FileName { get; init; }
}