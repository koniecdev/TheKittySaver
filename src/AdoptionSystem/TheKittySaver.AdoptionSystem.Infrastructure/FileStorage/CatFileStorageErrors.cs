using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Enums;

namespace TheKittySaver.AdoptionSystem.Infrastructure.FileStorage;

internal static class CatFileStorageErrors
{
    public static Error FileNotFound
        => new("CatFileStorage.FileNotFound", "The requested file was not found.", TypeOfError.NotFound);

    public static Error FailedToSaveFile(string reason)
        => new("CatFileStorage.FailedToSaveFile", $"Failed to save file: {reason}", TypeOfError.Failure);

    public static Error FailedToReadFile(string reason)
        => new("CatFileStorage.FailedToReadFile", $"Failed to read file: {reason}", TypeOfError.Failure);

    public static Error FailedToDeleteFile(string reason)
        => new("CatFileStorage.FailedToDeleteFile", $"Failed to delete file: {reason}", TypeOfError.Failure);

    public static Error InvalidContentType(string contentType)
        => new("CatFileStorage.InvalidContentType", $"Invalid content type: {contentType}. Only image files are allowed.", TypeOfError.Validation);
}
