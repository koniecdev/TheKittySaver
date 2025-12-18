using Microsoft.Extensions.Options;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Enums;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Infrastructure.FileUpload;

public interface IFileUploadValidator
{
    Result ValidateGalleryFile(long fileSize, string contentType, string? fileName);
    Result ValidateThumbnailFile(long fileSize, string contentType, string? fileName);
}

internal sealed class FileUploadValidator : IFileUploadValidator
{
    private readonly FileUploadOptions _options;

    public FileUploadValidator(IOptions<FileUploadOptions> options)
    {
        _options = options.Value;
    }

    public Result ValidateGalleryFile(long fileSize, string contentType, string? fileName)
    {
        return ValidateFile(
            fileSize,
            contentType,
            fileName,
            _options.MaxGalleryFileSizeInBytes,
            _options.AllowedGalleryContentTypes,
            _options.AllowedGalleryExtensions);
    }

    public Result ValidateThumbnailFile(long fileSize, string contentType, string? fileName)
    {
        return ValidateFile(
            fileSize,
            contentType,
            fileName,
            _options.MaxThumbnailFileSizeInBytes,
            _options.AllowedThumbnailContentTypes,
            _options.AllowedThumbnailExtensions);
    }

    private static Result ValidateFile(
        long fileSize,
        string contentType,
        string? fileName,
        long maxSizeInBytes,
        string[] allowedContentTypes,
        string[] allowedExtensions)
    {
        if (fileSize <= 0)
        {
            return Result.Failure(FileUploadErrors.EmptyFile);
        }

        if (fileSize > maxSizeInBytes)
        {
            return Result.Failure(FileUploadErrors.FileTooLarge(fileSize, maxSizeInBytes));
        }

        if (!allowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
        {
            return Result.Failure(FileUploadErrors.InvalidContentType(contentType, allowedContentTypes));
        }

        if (!string.IsNullOrWhiteSpace(fileName))
        {
            string extension = Path.GetExtension(fileName);
            if (!allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                return Result.Failure(FileUploadErrors.InvalidFileExtension(extension, allowedExtensions));
            }
        }

        return Result.Success();
    }
}

internal static class FileUploadErrors
{
    public static Error EmptyFile => new(
        "FileUpload.EmptyFile",
        "Uploaded file is empty.",
        TypeOfError.Validation);

    public static Error FileTooLarge(long actualSize, long maxSize) => new(
        "FileUpload.FileTooLarge",
        $"File size ({actualSize / 1024 / 1024.0:F2}MB) exceeds maximum allowed size ({maxSize / 1024 / 1024.0:F2}MB).",
        TypeOfError.Validation);

    public static Error InvalidContentType(string actualType, string[] allowedTypes) => new(
        "FileUpload.InvalidContentType",
        $"Content type '{actualType}' is not allowed. Allowed types: {string.Join(", ", allowedTypes)}.",
        TypeOfError.Validation);

    public static Error InvalidFileExtension(string actualExtension, string[] allowedExtensions) => new(
        "FileUpload.InvalidFileExtension",
        $"File extension '{actualExtension}' is not allowed. Allowed extensions: {string.Join(", ", allowedExtensions)}.",
        TypeOfError.Validation);
}
