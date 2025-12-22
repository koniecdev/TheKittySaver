namespace TheKittySaver.AdoptionSystem.Infrastructure.FileUpload;

internal readonly struct ImageDimensions
{
    public int Width { get; }
    public int Height { get; }
    public bool IsValid => Width > 0 && Height > 0;

    public ImageDimensions(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public static ImageDimensions Invalid => new(0, 0);
}

internal static class ImageDimensionsReader
{
    public static async Task<ImageDimensions> ReadDimensionsAsync(Stream stream, string contentType, CancellationToken cancellationToken = default)
    {
        if (!stream.CanSeek)
        {
            return ImageDimensions.Invalid;
        }

        long originalPosition = stream.Position;

        try
        {
            stream.Position = 0;

            return contentType.ToLowerInvariant() switch
            {
                "image/png" => await ReadPngDimensionsAsync(stream, cancellationToken),
                "image/jpeg" => await ReadJpegDimensionsAsync(stream, cancellationToken),
                "image/gif" => await ReadGifDimensionsAsync(stream, cancellationToken),
                "image/webp" => await ReadWebPDimensionsAsync(stream, cancellationToken),
                _ => ImageDimensions.Invalid
            };
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }

    private static async Task<ImageDimensions> ReadPngDimensionsAsync(Stream stream, CancellationToken cancellationToken)
    {
        byte[] header = new byte[24];
        int bytesRead = await stream.ReadAsync(header, cancellationToken);

        if (bytesRead < 24)
        {
            return ImageDimensions.Invalid;
        }

        byte[] pngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        if (!header.AsSpan(0, 8).SequenceEqual(pngSignature))
        {
            return ImageDimensions.Invalid;
        }

        int width = ReadBigEndianInt32(header, 16);
        int height = ReadBigEndianInt32(header, 20);

        return new ImageDimensions(width, height);
    }

    private static async Task<ImageDimensions> ReadJpegDimensionsAsync(Stream stream, CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[2];

        int bytesRead = await stream.ReadAsync(buffer, cancellationToken);
        if (bytesRead < 2 || buffer[0] != 0xFF || buffer[1] != 0xD8)
        {
            return ImageDimensions.Invalid;
        }

        while (true)
        {
            int firstByte = stream.ReadByte();
            if (firstByte == -1)
            {
                return ImageDimensions.Invalid;
            }

            if (firstByte != 0xFF)
            {
                return ImageDimensions.Invalid;
            }

            int markerType = stream.ReadByte();
            if (markerType == -1)
            {
                return ImageDimensions.Invalid;
            }

            while (markerType == 0xFF)
            {
                markerType = stream.ReadByte();
                if (markerType == -1)
                {
                    return ImageDimensions.Invalid;
                }
            }

            if (markerType == 0xD9)
            {
                return ImageDimensions.Invalid;
            }

            if (markerType == 0x00 || markerType is >= 0xD0 and <= 0xD7)
            {
                continue;
            }

            byte[] lengthBytes = new byte[2];
            bytesRead = await stream.ReadAsync(lengthBytes, cancellationToken);
            if (bytesRead < 2)
            {
                return ImageDimensions.Invalid;
            }

            int segmentLength = (lengthBytes[0] << 8) | lengthBytes[1];

            bool isSofMarker = markerType is >= 0xC0 and <= 0xCF
                               && markerType != 0xC4
                               && markerType != 0xC8
                               && markerType != 0xCC;

            if (isSofMarker)
            {
                byte[] sofData = new byte[5];
                bytesRead = await stream.ReadAsync(sofData, cancellationToken);
                if (bytesRead < 5)
                {
                    return ImageDimensions.Invalid;
                }

                int height = (sofData[1] << 8) | sofData[2];
                int width = (sofData[3] << 8) | sofData[4];

                return new ImageDimensions(width, height);
            }

            int skipBytes = segmentLength - 2;
            if (skipBytes > 0)
            {
                stream.Seek(skipBytes, SeekOrigin.Current);
            }
        }
    }

    private static async Task<ImageDimensions> ReadGifDimensionsAsync(Stream stream, CancellationToken cancellationToken)
    {
        byte[] header = new byte[10];
        int bytesRead = await stream.ReadAsync(header, cancellationToken);

        if (bytesRead < 10)
        {
            return ImageDimensions.Invalid;
        }

        if (header[0] != 'G' || header[1] != 'I' || header[2] != 'F' || header[3] != '8')
        {
            return ImageDimensions.Invalid;
        }

        int width = header[6] | (header[7] << 8);
        int height = header[8] | (header[9] << 8);

        return new ImageDimensions(width, height);
    }

    private static async Task<ImageDimensions> ReadWebPDimensionsAsync(Stream stream, CancellationToken cancellationToken)
    {
        byte[] header = new byte[30];
        int bytesRead = await stream.ReadAsync(header, cancellationToken);

        if (bytesRead < 16)
        {
            return ImageDimensions.Invalid;
        }

        byte[] riffSignature = [0x52, 0x49, 0x46, 0x46]; // RIFF
        byte[] webpSignature = [0x57, 0x45, 0x42, 0x50]; // WEBP

        if (!header.AsSpan(0, 4).SequenceEqual(riffSignature) ||
            !header.AsSpan(8, 4).SequenceEqual(webpSignature))
        {
            return ImageDimensions.Invalid;
        }

        if (bytesRead >= 30 && header[12] == 'V' && header[13] == 'P' && header[14] == '8' && header[15] == ' ')
        {
            int width = (header[26] | (header[27] << 8)) & 0x3FFF;
            int height = (header[28] | (header[29] << 8)) & 0x3FFF;
            return new ImageDimensions(width, height);
        }

        if (bytesRead >= 25 && header[12] == 'V' && header[13] == 'P' && header[14] == '8' && header[15] == 'L')
        {
            int bits = header[21] | (header[22] << 8) | (header[23] << 16) | (header[24] << 24);
            int width = (bits & 0x3FFF) + 1;
            int height = ((bits >> 14) & 0x3FFF) + 1;
            return new ImageDimensions(width, height);
        }

        if (bytesRead >= 30 && header[12] == 'V' && header[13] == 'P' && header[14] == '8' && header[15] == 'X')
        {
            int width = (header[24] | (header[25] << 8) | (header[26] << 16)) + 1;
            int height = (header[27] | (header[28] << 8) | (header[29] << 16)) + 1;
            return new ImageDimensions(width, height);
        }

        return ImageDimensions.Invalid;
    }

    private static int ReadBigEndianInt32(byte[] buffer, int offset)
    {
        return (buffer[offset] << 24) |
               (buffer[offset + 1] << 16) |
               (buffer[offset + 2] << 8) |
               buffer[offset + 3];
    }
}
