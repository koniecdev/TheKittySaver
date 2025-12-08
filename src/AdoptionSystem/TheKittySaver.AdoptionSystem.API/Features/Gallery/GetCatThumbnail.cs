using Mediator;
using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Gallery.Responses;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.ReadDbContexts;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.Gallery;

internal sealed class GetCatThumbnail : IEndpoint
{
    internal sealed record Query(CatId CatId) : IQuery<Result<CatThumbnailResponse>>;

    internal sealed class Handler : IQueryHandler<Query, Result<CatThumbnailResponse>>
    {
        private readonly IApplicationReadDbContext _readDbContext;

        public Handler(IApplicationReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }

        public async ValueTask<Result<CatThumbnailResponse>> Handle(Query query, CancellationToken cancellationToken)
        {
            bool catExists = await _readDbContext.Cats
                .AnyAsync(c => c.Id == query.CatId, cancellationToken);

            if (!catExists)
            {
                return Result.Failure<CatThumbnailResponse>(
                    DomainErrors.CatEntity.NotFound(query.CatId));
            }

            CatThumbnailResponse? response = await _readDbContext.CatThumbnails
                .Where(t => t.CatId == query.CatId)
                .Select(t => new CatThumbnailResponse(
                    Id: t.Id,
                    CatId: t.CatId))
                .FirstOrDefaultAsync(cancellationToken);

            if (response is null)
            {
                return Result.Failure<CatThumbnailResponse>(
                    DomainErrors.CatEntity.ThumbnailProperty.RequiredForPublishing(query.CatId));
            }

            return Result.Success(response);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("cats/{catId:guid}/thumbnail", async (
            Guid catId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Query query = new(new CatId(catId));

            Result<CatThumbnailResponse> queryResult = await sender.Send(query, cancellationToken);

            return queryResult.IsFailure
                ? Results.Problem(queryResult.Error.ToProblemDetails())
                : Results.Ok(queryResult.Value);
        });
    }
}
