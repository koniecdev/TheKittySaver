using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Services;

public interface ICatFileStorage //todo: I dont think it is domain
{
    Task<Result> SaveThumbnailAsync(
        CatId catId,
        CatThumbnailId thumbnailId,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken);

    Task<Result> SaveGalleryItemAsync(
        CatId catId,
        CatGalleryItemId galleryItemId,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken);

    Task<Result<CatFileData>> GetThumbnailAsync(
        CatId catId,
        CatThumbnailId thumbnailId,
        CancellationToken cancellationToken);

    Task<Result<CatFileData>> GetGalleryItemAsync(
        CatId catId,
        CatGalleryItemId galleryItemId,
        CancellationToken cancellationToken);

    Task<Result> DeleteThumbnailAsync(
        CatId catId,
        CatThumbnailId thumbnailId,
        CancellationToken cancellationToken);

    Task<Result> DeleteGalleryItemAsync(
        CatId catId,
        CatGalleryItemId galleryItemId,
        CancellationToken cancellationToken);
}

public sealed class CatFileData
{
    public required Stream FileStream { get; init; }
    public required string ContentType { get; init; }
    public required string FileName { get; init; }
}
