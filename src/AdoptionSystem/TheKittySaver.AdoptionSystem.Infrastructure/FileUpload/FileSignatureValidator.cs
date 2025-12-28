namespace TheKittySaver.AdoptionSystem.Infrastructure.FileUpload;

internal sealed class FileSignature
{
    public string ContentType { get; }
    public string Extension { get; }
    public byte[] MagicBytes { get; }
    public int Offset { get; }

    public FileSignature(string contentType, string extension, byte[] magicBytes, int offset = 0)
    {
        ContentType = contentType;
        Extension = extension;
        MagicBytes = magicBytes;
        Offset = offset;
    }
}

internal static class FileSignatures
{
    public const int RequiredHeaderSize = 16;

    private static readonly List<FileSignature> Signatures =
    [
        // JPEG
        new("image/jpeg", ".jpg", [0xFF, 0xD8, 0xFF]),
        new("image/jpeg", ".jpeg", [0xFF, 0xD8, 0xFF]),

        // PNG
        new("image/png", ".png", [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]),

        // GIF
        new("image/gif", ".gif", "GIF87a"u8.ToArray()),
        new("image/gif", ".gif", "GIF89a"u8.ToArray()),

        // WebP
        new("image/webp", ".webp", "RIFF"u8.ToArray()),

        // MP4 (ftyp box)
        new("video/mp4", ".mp4", "ftyp"u8.ToArray(), 4),

        // WebM (EBML header)
        new("video/webm", ".webm", [0x1A, 0x45, 0xDF, 0xA3]),

        // QuickTime MOV
        new("video/quicktime", ".mov", "ftypqt"u8.ToArray(), 4),
        new("video/quicktime", ".mov", "moov"u8.ToArray(), 4)
    ];

    public static bool IsValidSignature(ReadOnlySpan<byte> fileHeader, string contentType)
    {
        foreach (FileSignature signature in Signatures)
        {
            if (!signature.ContentType.Equals(contentType, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (fileHeader.Length < signature.Offset + signature.MagicBytes.Length)
            {
                continue;
            }

            ReadOnlySpan<byte> headerSlice = fileHeader.Slice(signature.Offset, signature.MagicBytes.Length);
            if (!headerSlice.SequenceEqual(signature.MagicBytes))
            {
                continue;
            }

            if (contentType.Equals("image/webp", StringComparison.OrdinalIgnoreCase) && !ValidateWebPSignature(fileHeader))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    public static bool TryGetContentTypeFromSignature(ReadOnlySpan<byte> fileHeader, out string? detectedContentType)
    {
        detectedContentType = null;

        foreach (FileSignature signature in Signatures)
        {
            if (fileHeader.Length < signature.Offset + signature.MagicBytes.Length)
            {
                continue;
            }

            ReadOnlySpan<byte> headerSlice = fileHeader.Slice(signature.Offset, signature.MagicBytes.Length);
            if (!headerSlice.SequenceEqual(signature.MagicBytes))
            {
                continue;
            }

            if (signature.ContentType.Equals("image/webp", StringComparison.OrdinalIgnoreCase) && !ValidateWebPSignature(fileHeader))
            {
                continue;
            }

            detectedContentType = signature.ContentType;
            return true;
        }

        return false;
    }

    private static bool ValidateWebPSignature(ReadOnlySpan<byte> fileHeader)
    {
        if (fileHeader.Length < 12)
        {
            return false;
        }

        ReadOnlySpan<byte> webpMarker = "WEBP"u8;
        return fileHeader.Slice(8, 4).SequenceEqual(webpMarker);
    }
}
