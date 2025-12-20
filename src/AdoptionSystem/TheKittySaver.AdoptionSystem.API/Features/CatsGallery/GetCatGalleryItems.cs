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

internal sealed class GetCatGalleryItems : IEndpoint
{
    internal sealed record Query(CatId CatId) : IQuery<Result<IReadOnlyList<CatGalleryItemResponse>>>;

    internal sealed class Handler : IQueryHandler<Query, Result<IReadOnlyList<CatGalleryItemResponse>>>
    {
        private readonly IApplicationReadDbContext _readDbContext;

        public Handler(IApplicationReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }

        public async ValueTask<Result<IReadOnlyList<CatGalleryItemResponse>>> Handle(Query query, CancellationToken cancellationToken)
        {
            bool catExists = await _readDbContext.Cats
                .AnyAsync(c => c.Id == query.CatId, cancellationToken);

            if (!catExists)
            {
                return Result.Failure<IReadOnlyList<CatGalleryItemResponse>>(
                    DomainErrors.CatEntity.NotFound(query.CatId));
            }

            IReadOnlyList<CatGalleryItemResponse> response = await _readDbContext.CatGalleryItems
                .Where(g => g.CatId == query.CatId)
                .OrderBy(g => g.DisplayOrder)
                .Select(g => new CatGalleryItemResponse(
                    Id: g.Id,
                    CatId: g.CatId,
                    DisplayOrder: g.DisplayOrder))
                .ToListAsync(cancellationToken);

            return Result.Success(response);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("cats/{catId:guid}/gallery", async (
            Guid catId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Query query = new(new CatId(catId));

            Result<IReadOnlyList<CatGalleryItemResponse>> queryResult = await sender.Send(query, cancellationToken);

            return queryResult.IsFailure
                ? Results.Problem(queryResult.Error.ToProblemDetails())
                : Results.Ok(queryResult.Value);
        });
    }
}
