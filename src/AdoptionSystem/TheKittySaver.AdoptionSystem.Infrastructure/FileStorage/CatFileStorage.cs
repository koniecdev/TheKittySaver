using Microsoft.Extensions.Options;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Services;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Guards;

namespace TheKittySaver.AdoptionSystem.Infrastructure.FileStorage;

internal sealed class CatFileStorage : ICatFileStorage
{
    private readonly CatFileStorageOptions _options;

    public CatFileStorage(IOptionsSnapshot<CatFileStorageOptions> options)
    {
        _options = options.Value;
    }

    public async Task<Result> SaveThumbnailAsync(
        CatId catId,
        CatThumbnailId thumbnailId,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken)
    {
        Ensure.NotEmpty(catId);
        Ensure.NotEmpty(thumbnailId);
        ArgumentNullException.ThrowIfNull(fileStream);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        string directory = GetThumbnailDirectory(catId);
        string filePath = GetThumbnailFilePath(catId, thumbnailId, contentType);

        return await SaveFileAsync(directory, filePath, fileStream, cancellationToken);
    }

    public async Task<Result> SaveGalleryItemAsync(
        CatId catId,
        CatGalleryItemId galleryItemId,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken)
    {
        Ensure.NotEmpty(catId);
        Ensure.NotEmpty(galleryItemId);
        ArgumentNullException.ThrowIfNull(fileStream);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        string directory = GetGalleryDirectory(catId);
        string filePath = GetGalleryItemFilePath(catId, galleryItemId, contentType);

        return await SaveFileAsync(directory, filePath, fileStream, cancellationToken);
    }

    public async Task<Result<CatFileData>> GetThumbnailAsync(
        CatId catId,
        CatThumbnailId thumbnailId,
        CancellationToken cancellationToken)
    {
        Ensure.NotEmpty(catId);
        Ensure.NotEmpty(thumbnailId);

        string directory = GetThumbnailDirectory(catId);
        string filePattern = $"{thumbnailId.Value}.*";

        return await GetFileAsync(directory, filePattern);
    }

    public async Task<Result<CatFileData>> GetGalleryItemAsync(
        CatId catId,
        CatGalleryItemId galleryItemId,
        CancellationToken cancellationToken)
    {
        Ensure.NotEmpty(catId);
        Ensure.NotEmpty(galleryItemId);

        string directory = GetGalleryDirectory(catId);
        string filePattern = $"{galleryItemId.Value}.*";

        return await GetFileAsync(directory, filePattern);
    }

    public Task<Result> DeleteThumbnailAsync(
        CatId catId,
        CatThumbnailId thumbnailId,
        CancellationToken cancellationToken)
    {
        Ensure.NotEmpty(catId);
        Ensure.NotEmpty(thumbnailId);

        string directory = GetThumbnailDirectory(catId);
        string filePattern = $"{thumbnailId.Value}.*";

        return Task.FromResult(DeleteFile(directory, filePattern));
    }

    public Task<Result> DeleteGalleryItemAsync(
        CatId catId,
        CatGalleryItemId galleryItemId,
        CancellationToken cancellationToken)
    {
        Ensure.NotEmpty(catId);
        Ensure.NotEmpty(galleryItemId);

        string directory = GetGalleryDirectory(catId);
        string filePattern = $"{galleryItemId.Value}.*";

        return Task.FromResult(DeleteFile(directory, filePattern));
    }

    private string GetThumbnailDirectory(CatId catId)
    {
        return Path.Combine(_options.BasePath, "CatFiles", catId.Value.ToString(), "thumbnail");
    }

    private string GetGalleryDirectory(CatId catId)
    {
        return Path.Combine(_options.BasePath, "CatFiles", catId.Value.ToString(), "gallery");
    }

    private string GetThumbnailFilePath(CatId catId, CatThumbnailId thumbnailId, string contentType)
    {
        string extension = GetExtensionFromContentType(contentType);
        string directory = GetThumbnailDirectory(catId);
        return Path.Combine(directory, $"{thumbnailId.Value}{extension}");
    }

    private string GetGalleryItemFilePath(CatId catId, CatGalleryItemId galleryItemId, string contentType)
    {
        string extension = GetExtensionFromContentType(contentType);
        string directory = GetGalleryDirectory(catId);
        return Path.Combine(directory, $"{galleryItemId.Value}{extension}");
    }

    private static string GetExtensionFromContentType(string contentType)
    {
        return contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            "image/webp" => ".webp",
            _ => ".bin"
        };
    }

    private static string GetContentTypeFromExtension(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }

    private static async Task<Result> SaveFileAsync(
        string directory,
        string filePath,
        Stream fileStream,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string[] existingFiles = Directory.GetFiles(
                directory, Path.GetFileNameWithoutExtension(filePath) + ".*");
            foreach (string existingFile in existingFiles)
            {
                File.Delete(existingFile);
            }

            await using FileStream fs = new(filePath, FileMode.Create, FileAccess.Write);
            await fileStream.CopyToAsync(fs, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(CatFileStorageErrors.FailedToSaveFile(ex.Message));
        }
    }

    private static Task<Result<CatFileData>> GetFileAsync(
        string directory,
        string filePattern)
    {
        try
        {
            if (!Directory.Exists(directory))
            {
                return Task.FromResult(Result.Failure<CatFileData>(CatFileStorageErrors.FileNotFound));
            }

            string[] files = Directory.GetFiles(directory, filePattern);
            if (files.Length == 0)
            {
                return Task.FromResult(Result.Failure<CatFileData>(CatFileStorageErrors.FileNotFound));
            }

            string filePath = files[0];
            string extension = Path.GetExtension(filePath);
            string contentType = GetContentTypeFromExtension(extension);
            string fileName = Path.GetFileName(filePath);

            FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read);

            CatFileData fileData = new()
            {
                FileStream = fileStream,
                ContentType = contentType,
                FileName = fileName
            };

            return Task.FromResult(Result.Success(fileData));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result.Failure<CatFileData>(CatFileStorageErrors.FailedToReadFile(ex.Message)));
        }
    }

    private static Result DeleteFile(string directory, string filePattern)
    {
        try
        {
            if (!Directory.Exists(directory))
            {
                return Result.Success();
            }

            string[] files = Directory.GetFiles(directory, filePattern);
            foreach (string file in files)
            {
                File.Delete(file);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(CatFileStorageErrors.FailedToDeleteFile(ex.Message));
        }
    }
}
