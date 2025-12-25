using Mediator;
using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Gallery.Responses;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.ReadDbContexts;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.CatsGallery;

internal sealed class GetCatGalleryItem : IEndpoint
{
    internal sealed record Query(
        CatId CatId,
        CatGalleryItemId GalleryItemId) : IQuery<Result<CatGalleryItemResponse>>;

    internal sealed class Handler : IQueryHandler<Query, Result<CatGalleryItemResponse>>
    {
        private readonly IApplicationReadDbContext _readDbContext;

        public Handler(IApplicationReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }

        public async ValueTask<Result<CatGalleryItemResponse>> Handle(Query query, CancellationToken cancellationToken)
        {
            CatGalleryItemResponse? response = await _readDbContext.CatGalleryItems
                .Where(g =>
                    g.CatId == query.CatId
                    && g.Id == query.GalleryItemId)
                .Select(g => new CatGalleryItemResponse(
                    Id: g.Id,
                    CatId: g.CatId,
                    DisplayOrder: g.DisplayOrder))
                .FirstOrDefaultAsync(cancellationToken);

            return response is null
                ? Result.Failure<CatGalleryItemResponse>(
                    DomainErrors.CatGalleryItemEntity.NotFound(query.GalleryItemId))
                : Result.Success(response);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("cats/{catId:guid}/gallery/{galleryItemId:guid}", async (
            Guid catId,
            Guid galleryItemId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Query query = new(new CatId(catId), new CatGalleryItemId(galleryItemId));

            Result<CatGalleryItemResponse> queryResult = await sender.Send(query, cancellationToken);

            return queryResult.IsFailure
                ? Results.Problem(queryResult.Error.ToProblemDetails())
                : Results.Ok(queryResult.Value);
        });
    }
}
