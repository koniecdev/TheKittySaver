using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Services;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Infrastructure.FileStorage;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.ReadDbContexts;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.CatsGallery;

internal sealed class GetCatThumbnail : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("cats/{catId:guid}/thumbnail", async (
            Guid catId,
            IApplicationReadDbContext readDbContext,
            ICatFileStorage catFileStorage,
            CancellationToken cancellationToken) =>
        {
            CatId catIdTyped = new(catId);

            CatThumbnailReadModel? thumbnail = await readDbContext.CatThumbnails
                .FirstOrDefaultAsync(t => t.CatId == catIdTyped, cancellationToken);

            if (thumbnail is null)
            {
                return Results.Problem(
                    DomainErrors.CatEntity.ThumbnailProperty.NotUploaded(catIdTyped).ToProblemDetails());
            }

            Result<CatFileData> fileResult = await catFileStorage.GetThumbnailAsync(
                catIdTyped,
                thumbnail.Id,
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
