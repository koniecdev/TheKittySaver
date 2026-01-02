using System.Globalization;
using System.Linq.Expressions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Gallery.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Common;
using TheKittySaver.AdoptionSystem.Domain.Core.Extensions;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.ReadDbContexts;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.Cats;

internal sealed class GetCats : IEndpoint
{
    internal sealed record Query(
        ValueMaybe<PersonId> MaybePersonId,
        ValueMaybe<AdoptionAnnouncementId> MaybeAdoptionAnnouncementId,
        int Page = 1,
        int PageSize = 50,
        string? Sort = null)
        : IQuery<PaginationResponse<CatListItemResponse>>, IPaginationable, ISortable;

    internal sealed class Handler : IQueryHandler<Query, PaginationResponse<CatListItemResponse>>
    {
        private readonly IApplicationReadDbContext _readDbContext;

        public Handler(IApplicationReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }

        public async ValueTask<PaginationResponse<CatListItemResponse>> Handle(Query query,
            CancellationToken cancellationToken)
        {
            IQueryable<CatReadModel> sortedQuery = _readDbContext.Cats
                .WhereIf(query.MaybePersonId.HasValue,
                    cat => cat.PersonId == query.MaybePersonId.Value)
                .WhereIf(query.MaybeAdoptionAnnouncementId.HasValue,
                    cat => cat.AdoptionAnnouncementId == query.MaybeAdoptionAnnouncementId.Value);

            int totalCount = await sortedQuery.CountAsync(cancellationToken);

            sortedQuery = !string.IsNullOrWhiteSpace(query.Sort) 
                ? sortedQuery.ApplyMultipleSorting(query.Sort, GetSortProperty) 
                : sortedQuery.OrderBy(c => c.Name);

            IReadOnlyList<CatListItemResponse> items = await sortedQuery
                .ApplyPagination(page: query.Page, pageSize: query.PageSize)
                .Select(c => new CatListItemResponse(
                    Id: c.Id,
                    PersonId: c.PersonId,
                    AdoptionAnnouncementId: c.AdoptionAnnouncementId,
                    Name: c.Name,
                    Description: c.Description,
                    Age: c.Age,
                    Gender: c.Gender,
                    Color: c.Color,
                    WeightInGrams: c.WeightValueInGrams,
                    HealthStatus: c.HealthStatus,
                    SpecialNeedsStatusHasSpecialNeeds: c.SpecialNeedsStatusHasSpecialNeeds,
                    SpecialNeedsStatusDescription: c.SpecialNeedsStatusDescription,
                    SpecialNeedsStatusSeverityType: c.SpecialNeedsStatusSeverityType,
                    Temperament: c.Temperament,
                    AdoptionHistoryReturnCount: c.AdoptionHistoryReturnCount,
                    AdoptionHistoryLastReturnDate: c.AdoptionHistoryLastReturnDate,
                    AdoptionHistoryLastReturnReason: c.AdoptionHistoryLastReturnReason,
                    IsNeutered: c.NeuteringStatusIsNeutered,
                    InfectiousDiseaseStatusFivStatus: c.InfectiousDiseaseStatusFivStatus,
                    InfectiousDiseaseStatusFelvStatus: c.InfectiousDiseaseStatusFelvStatus,
                    InfectiousDiseaseStatusLastTestedAt: c.InfectiousDiseaseStatusLastTestedAt,
                    GalleryItems: c.GalleryItems
                        .OrderBy(g => g.DisplayOrder)
                        .Select(g => new CatGalleryItemEmbeddedDto(
                            Path: $"cats/{c.Id}/gallery/{g.Id}/file",
                            DisplayOrder: g.DisplayOrder))
                        .ToList()))
                .ToListAsync(cancellationToken);

            PaginationResponse<CatListItemResponse> response = new()
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };

            return response;
        }

        private static Expression<Func<CatReadModel, object>> GetSortProperty(string propertyName)
            => propertyName.ToLower(CultureInfo.InvariantCulture) switch
            {
                "name" => cat => cat.Name,
                "age" => cat => cat.Age,
                _ => cat => cat.Name
            };
    }

    private sealed record Filters(Guid? PersonId, Guid? AdoptionAnnouncementId);

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("cats", async (
            [AsParameters] PaginationAndMultipleSorting paginationAndMultipleSorting,
            [AsParameters] Filters filters,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Query query = new(
                filters.PersonId.ToMaybeId<PersonId>(),
                filters.AdoptionAnnouncementId.ToMaybeId<AdoptionAnnouncementId>(),
                Page: paginationAndMultipleSorting.Page,
                PageSize: paginationAndMultipleSorting.PageSize,
                Sort: paginationAndMultipleSorting.Sort);

            PaginationResponse<CatListItemResponse> response = await sender.Send(query, cancellationToken);
 
            return Results.Ok(response);
        });
    }
}
