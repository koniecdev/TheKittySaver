using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Services;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.Gallery;

internal sealed class GetCatThumbnailFile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("cats/{catId:guid}/thumbnail/{thumbnailId:guid}/file", async (
            Guid catId,
            Guid thumbnailId,
            ICatFileStorage catFileStorage,
            CancellationToken cancellationToken) =>
        {
            CatId catIdTyped = new(catId);
            CatThumbnailId thumbnailIdTyped = new(thumbnailId);

            Result<CatFileData> fileResult = await catFileStorage.GetThumbnailAsync(
                catIdTyped,
                thumbnailIdTyped,
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
