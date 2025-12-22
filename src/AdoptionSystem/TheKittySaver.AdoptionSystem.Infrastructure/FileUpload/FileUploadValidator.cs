using Microsoft.Extensions.Options;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Enums;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Infrastructure.FileUpload;

public interface IFileUploadValidator
{
    Result ValidateGalleryFile(long fileSize, string contentType, string? fileName);
    Result ValidateThumbnailFile(long fileSize, string contentType, string? fileName);

    Task<Result> ValidateGalleryFileWithContentAsync(
        Stream fileStream,
        long fileSize,
        string contentType,
        string? fileName,
        CancellationToken cancellationToken = default);

    Task<Result> ValidateThumbnailFileWithContentAsync(
        Stream fileStream,
        long fileSize,
        string contentType,
        string? fileName,
        CancellationToken cancellationToken = default);
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
        return ValidateFileMetadata(
            fileSize,
            contentType,
            fileName,
            _options.MaxGalleryFileSizeInBytes,
            _options.AllowedGalleryContentTypes,
            _options.AllowedGalleryExtensions);
    }

    public Result ValidateThumbnailFile(long fileSize, string contentType, string? fileName)
    {
        return ValidateFileMetadata(
            fileSize,
            contentType,
            fileName,
            _options.MaxThumbnailFileSizeInBytes,
            _options.AllowedThumbnailContentTypes,
            _options.AllowedThumbnailExtensions);
    }

    public async Task<Result> ValidateGalleryFileWithContentAsync(
        Stream fileStream,
        long fileSize,
        string contentType,
        string? fileName,
        CancellationToken cancellationToken = default)
    {
        Result metadataValidation = ValidateGalleryFile(fileSize, contentType, fileName);
        if (metadataValidation.IsFailure)
        {
            return metadataValidation;
        }

        return await ValidateFileContentAsync(
            fileStream,
            contentType,
            _options.GalleryImageDimensions,
            isImageRequired: false,
            cancellationToken);
    }

    public async Task<Result> ValidateThumbnailFileWithContentAsync(
        Stream fileStream,
        long fileSize,
        string contentType,
        string? fileName,
        CancellationToken cancellationToken = default)
    {
        Result metadataValidation = ValidateThumbnailFile(fileSize, contentType, fileName);
        if (metadataValidation.IsFailure)
        {
            return metadataValidation;
        }

        return await ValidateFileContentAsync(
            fileStream,
            contentType,
            _options.ThumbnailImageDimensions,
            isImageRequired: true,
            cancellationToken);
    }

    private Result ValidateFileMetadata(
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

        if (string.IsNullOrWhiteSpace(contentType))
        {
            return Result.Failure(FileUploadErrors.MissingContentType);
        }

        if (!allowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
        {
            return Result.Failure(FileUploadErrors.InvalidContentType(contentType, allowedContentTypes));
        }

        if (_options.ValidateFileName && !string.IsNullOrWhiteSpace(fileName))
        {
            FileNameValidationResult fileNameValidation = FileNameValidator.Validate(fileName);
            if (!fileNameValidation.IsValid)
            {
                return Result.Failure(new Error(
                    fileNameValidation.ErrorCode!,
                    fileNameValidation.ErrorMessage!,
                    TypeOfError.Validation));
            }

            string extension = Path.GetExtension(fileName);
            if (!string.IsNullOrEmpty(extension) && !allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                return Result.Failure(FileUploadErrors.InvalidFileExtension(extension, allowedExtensions));
            }

            Result extensionContentTypeMatch = ValidateExtensionMatchesContentType(extension, contentType);
            if (extensionContentTypeMatch.IsFailure)
            {
                return extensionContentTypeMatch;
            }
        }

        return Result.Success();
    }

    private async Task<Result> ValidateFileContentAsync(
        Stream fileStream,
        string contentType,
        ImageDimensionLimits dimensionLimits,
        bool isImageRequired,
        CancellationToken cancellationToken)
    {
        if (!fileStream.CanSeek || !fileStream.CanRead)
        {
            return Result.Failure(FileUploadErrors.StreamNotReadable);
        }

        long originalPosition = fileStream.Position;

        try
        {
            fileStream.Position = 0;

            if (_options.ValidateMagicBytes)
            {
                Result magicBytesValidation = await ValidateMagicBytesAsync(fileStream, contentType, cancellationToken);
                if (magicBytesValidation.IsFailure)
                {
                    return magicBytesValidation;
                }
            }

            bool isImage = contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

            if (_options.ValidateImageDimensions && isImage)
            {
                fileStream.Position = 0;
                Result dimensionsValidation = await ValidateImageDimensionsAsync(
                    fileStream,
                    contentType,
                    dimensionLimits,
                    cancellationToken);

                if (dimensionsValidation.IsFailure)
                {
                    return dimensionsValidation;
                }
            }
            else if (isImageRequired && !isImage)
            {
                return Result.Failure(FileUploadErrors.ImageRequired);
            }

            return Result.Success();
        }
        finally
        {
            fileStream.Position = originalPosition;
        }
    }

    private static async Task<Result> ValidateMagicBytesAsync(
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken)
    {
        byte[] headerBuffer = new byte[FileSignatures.RequiredHeaderSize];

        int bytesRead = await fileStream.ReadAsync(headerBuffer, cancellationToken);
        if (bytesRead < 4)
        {
            return Result.Failure(FileUploadErrors.CorruptedFile);
        }

        bool isValidSignature = FileSignatures.IsValidSignature(headerBuffer.AsSpan(0, bytesRead), contentType);
        if (!isValidSignature)
        {
            bool detected = FileSignatures.TryGetContentTypeFromSignature(headerBuffer.AsSpan(0, bytesRead), out string? detectedType);

            if (detected && detectedType != null)
            {
                return Result.Failure(FileUploadErrors.ContentTypeMismatch(contentType, detectedType));
            }

            return Result.Failure(FileUploadErrors.InvalidFileSignature(contentType));
        }

        return Result.Success();
    }

    private static async Task<Result> ValidateImageDimensionsAsync(
        Stream fileStream,
        string contentType,
        ImageDimensionLimits limits,
        CancellationToken cancellationToken)
    {
        ImageDimensions dimensions = await ImageDimensionsReader.ReadDimensionsAsync(fileStream, contentType, cancellationToken);

        if (!dimensions.IsValid)
        {
            return Result.Failure(FileUploadErrors.UnableToReadImageDimensions);
        }

        if (dimensions.Width < limits.MinWidth || dimensions.Height < limits.MinHeight)
        {
            return Result.Failure(FileUploadErrors.ImageTooSmall(
                dimensions.Width,
                dimensions.Height,
                limits.MinWidth,
                limits.MinHeight));
        }

        if (dimensions.Width > limits.MaxWidth || dimensions.Height > limits.MaxHeight)
        {
            return Result.Failure(FileUploadErrors.ImageTooLarge(
                dimensions.Width,
                dimensions.Height,
                limits.MaxWidth,
                limits.MaxHeight));
        }

        return Result.Success();
    }

    private static Result ValidateExtensionMatchesContentType(string extension, string contentType)
    {
        if (string.IsNullOrEmpty(extension))
        {
            return Result.Success();
        }

        Dictionary<string, string[]> extensionToContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            { ".jpg", ["image/jpeg"] },
            { ".jpeg", ["image/jpeg"] },
            { ".png", ["image/png"] },
            { ".gif", ["image/gif"] },
            { ".webp", ["image/webp"] },
            { ".mp4", ["video/mp4"] },
            { ".webm", ["video/webm"] },
            { ".mov", ["video/quicktime"] }
        };

        if (!extensionToContentTypes.TryGetValue(extension, out string[]? expectedContentTypes))
        {
            return Result.Success();
        }

        if (!expectedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
        {
            return Result.Failure(FileUploadErrors.ExtensionContentTypeMismatch(extension, contentType));
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

    public static Error MissingContentType => new(
        "FileUpload.MissingContentType",
        "Content-Type header is missing or empty.",
        TypeOfError.Validation);

    public static Error FileTooLarge(long actualSize, long maxSize) => new(
        "FileUpload.FileTooLarge",
        $"File size ({FormatFileSize(actualSize)}) exceeds maximum allowed size ({FormatFileSize(maxSize)}).",
        TypeOfError.Validation);

    public static Error InvalidContentType(string actualType, string[] allowedTypes) => new(
        "FileUpload.InvalidContentType",
        $"Content type '{actualType}' is not allowed. Allowed types: {string.Join(", ", allowedTypes)}.",
        TypeOfError.Validation);

    public static Error InvalidFileExtension(string actualExtension, string[] allowedExtensions) => new(
        "FileUpload.InvalidFileExtension",
        $"File extension '{actualExtension}' is not allowed. Allowed extensions: {string.Join(", ", allowedExtensions)}.",
        TypeOfError.Validation);

    public static Error StreamNotReadable => new(
        "FileUpload.StreamNotReadable",
        "File stream must be readable and seekable for content validation.",
        TypeOfError.Failure);

    public static Error CorruptedFile => new(
        "FileUpload.CorruptedFile",
        "File appears to be corrupted or empty.",
        TypeOfError.Validation);

    public static Error InvalidFileSignature(string expectedContentType) => new(
        "FileUpload.InvalidFileSignature",
        $"File content does not match the expected format for '{expectedContentType}'.",
        TypeOfError.Validation);

    public static Error ContentTypeMismatch(string declaredType, string detectedType) => new(
        "FileUpload.ContentTypeMismatch",
        $"Declared content type '{declaredType}' does not match actual file type '{detectedType}'.",
        TypeOfError.Validation);

    public static Error ExtensionContentTypeMismatch(string extension, string contentType) => new(
        "FileUpload.ExtensionContentTypeMismatch",
        $"File extension '{extension}' does not match content type '{contentType}'.",
        TypeOfError.Validation);

    public static Error ImageRequired => new(
        "FileUpload.ImageRequired",
        "An image file is required for this upload.",
        TypeOfError.Validation);

    public static Error UnableToReadImageDimensions => new(
        "FileUpload.UnableToReadImageDimensions",
        "Unable to read image dimensions. The file may be corrupted.",
        TypeOfError.Validation);

    public static Error ImageTooSmall(int width, int height, int minWidth, int minHeight) => new(
        "FileUpload.ImageTooSmall",
        $"Image dimensions ({width}x{height}) are below minimum required ({minWidth}x{minHeight}).",
        TypeOfError.Validation);

    public static Error ImageTooLarge(int width, int height, int maxWidth, int maxHeight) => new(
        "FileUpload.ImageTooLarge",
        $"Image dimensions ({width}x{height}) exceed maximum allowed ({maxWidth}x{maxHeight}).",
        TypeOfError.Validation);

    private static string FormatFileSize(long bytes)
    {
        return bytes switch
        {
            < 1024 => $"{bytes} B",
            < 1024 * 1024 => $"{bytes / 1024.0:F2} KB",
            _ => $"{bytes / 1024.0 / 1024.0:F2} MB"
        };
    }
}
