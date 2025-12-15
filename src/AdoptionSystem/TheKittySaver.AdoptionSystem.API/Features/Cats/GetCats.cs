using Mediator;
using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Common.Sorting;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Common;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.ReadDbContexts;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.Cats;

internal sealed class GetCats : IEndpoint
{
    internal sealed record Query(int Page, int PageSize, string? Sort)
        : IQuery<Result<PaginationResult<CatResponse>>>, IPagedQuery;

    internal sealed class Handler : IQueryHandler<Query, Result<PaginationResult<CatResponse>>>
    {
        private readonly IApplicationReadDbContext _readDbContext;

        public Handler(IApplicationReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }

        public async ValueTask<Result<PaginationResult<CatResponse>>> Handle(Query query, CancellationToken cancellationToken)
        {
            int totalCount = await _readDbContext.Cats.CountAsync(cancellationToken);

            IOrderedQueryable<CatReadModel>? sortedQuery = _readDbContext.Cats.ApplySortOrNull(query.Sort);

            IOrderedQueryable<CatReadModel> orderedQuery = sortedQuery is not null
                ? sortedQuery.ThenBy(c => c.Id)
                : _readDbContext.Cats.OrderBy(c => c.Id);

            IReadOnlyList<CatResponse> items = await orderedQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(c => new CatResponse(
                    Id: c.Id,
                    PersonId: c.PersonId,
                    AdoptionAnnouncementId: c.AdoptionAnnouncementId,
                    Name: c.Name,
                    Description: c.Description,
                    Age: c.Age,
                    Gender: c.Gender,
                    Color: c.Color,
                    WeightValueInKilograms: c.WeightValueInKilograms,
                    HealthStatus: c.HealthStatus,
                    SpecialNeedsStatusHasSpecialNeeds: c.SpecialNeedsStatusHasSpecialNeeds,
                    SpecialNeedsStatusDescription: c.SpecialNeedsStatusDescription,
                    SpecialNeedsStatusSeverityType: c.SpecialNeedsStatusSeverityType,
                    Temperament: c.Temperament,
                    AdoptionHistoryReturnCount: c.AdoptionHistoryReturnCount,
                    AdoptionHistoryLastReturnDate: c.AdoptionHistoryLastReturnDate,
                    AdoptionHistoryLastReturnReason: c.AdoptionHistoryLastReturnReason,
                    ListingSourceType: c.ListingSourceType,
                    ListingSourceSourceName: c.ListingSourceSourceName,
                    IsNeutered: c.NeuteringStatusIsNeutered,
                    InfectiousDiseaseStatusFivStatus: c.InfectiousDiseaseStatusFivStatus,
                    InfectiousDiseaseStatusFelvStatus: c.InfectiousDiseaseStatusFelvStatus,
                    InfectiousDiseaseStatusLastTestedAt: c.InfectiousDiseaseStatusLastTestedAt))
                .ToListAsync(cancellationToken);

            PaginationResult<CatResponse> response = new()
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };

            return Result.Success(response);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("cats", async (
            [AsParameters] PaginationRequest pagination,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Query query = new(pagination.Page, pagination.PageSize, pagination.Sort);

            Result<PaginationResult<CatResponse>> queryResult = await sender.Send(query, cancellationToken);

            return Results.Ok(queryResult.Value);
        });
    }
}
