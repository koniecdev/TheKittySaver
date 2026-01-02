using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.Infrastructure.FileStorage;

public interface ICatFileStorage
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