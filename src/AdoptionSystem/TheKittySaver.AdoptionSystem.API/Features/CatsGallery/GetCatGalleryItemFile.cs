using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Services;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Infrastructure.FileStorage;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.CatsGallery;

internal sealed class GetCatGalleryItemFile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("cats/{catId:guid}/gallery/{galleryItemId:guid}/file", async (
            Guid catId,
            Guid galleryItemId,
            ICatFileStorage catFileStorage,
            CancellationToken cancellationToken) =>
        {
            CatId catIdTyped = new(catId);
            CatGalleryItemId galleryItemIdTyped = new(galleryItemId);

            Result<CatFileData> fileResult = await catFileStorage.GetGalleryItemAsync(
                catIdTyped,
                galleryItemIdTyped,
                cancellationToken);

            if (fileResult.IsFailure)
            {
                return Results.Problem(fileResult.Error.ToProblemDetails());
            }

            CatFileData fileData = fileResult.Value;
            return Results.File(fileData.FileStream, fileData.ContentType, fileData.FileName);
        });
    }
}
