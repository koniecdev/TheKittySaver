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
}
