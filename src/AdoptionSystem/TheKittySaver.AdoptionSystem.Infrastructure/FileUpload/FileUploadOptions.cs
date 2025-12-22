namespace TheKittySaver.AdoptionSystem.Infrastructure.FileUpload;

public sealed class FileUploadOptions
{
    public const string SectionName = "FileUpload";

    public long MaxGalleryFileSizeInBytes { get; init; } = 25 * 1024 * 1024; // 25MB (images + videos)
    public long MaxThumbnailFileSizeInBytes { get; init; } = 2 * 1024 * 1024; // 2MB (images only)

    public string[] AllowedGalleryContentTypes { get; init; } =
    [
        "image/jpeg", "image/png", "image/gif", "image/webp",
        "video/mp4", "video/webm", "video/quicktime"
    ];

    public string[] AllowedGalleryExtensions { get; init; } =
    [
        ".jpg", ".jpeg", ".png", ".gif", ".webp",
        ".mp4", ".webm", ".mov"
    ];

    public string[] AllowedThumbnailContentTypes { get; init; } =
    [
        "image/jpeg", "image/png", "image/gif", "image/webp"
    ];

    public string[] AllowedThumbnailExtensions { get; init; } =
    [
        ".jpg", ".jpeg", ".png", ".gif", ".webp"
    ];

    public ImageDimensionLimits GalleryImageDimensions { get; init; } = new();
    public ImageDimensionLimits ThumbnailImageDimensions { get; init; } = new()
    {
        MinWidth = 100,
        MinHeight = 100,
        MaxWidth = 2000,
        MaxHeight = 2000
    };

    public bool ValidateMagicBytes { get; init; } = true;
    public bool ValidateFileName { get; init; } = true;
    public bool ValidateImageDimensions { get; init; } = true;
}

public sealed class ImageDimensionLimits
{
    public int MinWidth { get; init; } = 1;
    public int MinHeight { get; init; } = 1;
    public int MaxWidth { get; init; } = 8192;
    public int MaxHeight { get; init; } = 8192;
}
