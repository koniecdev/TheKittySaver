using System.Globalization;
using System.Linq.Expressions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Calculators.Abstractions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Common;
using TheKittySaver.AdoptionSystem.Domain.Core.Extensions;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.ReadDbContexts;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.AdoptionAnnouncementAggregate;
 
namespace TheKittySaver.AdoptionSystem.API.Features.AdoptionAnnouncements;
 
internal sealed class GetAdoptionAnnouncements : IEndpoint
{
    internal sealed record Query(
        ValueMaybe<PersonId> MaybePersonId,
        int Page = 1,
        int PageSize = 50,
        string? Sort = null)
        : IQuery<PaginationResponse<AdoptionAnnouncementListItemResponse>>, IPaginationable, ISortable;

    internal sealed class Handler : IQueryHandler<Query, PaginationResponse<AdoptionAnnouncementListItemResponse>>
    {
        private readonly IApplicationReadDbContext _readDbContext;
        private readonly IAdoptionPriorityScoreCalculator _calculator;

        public Handler(
            IApplicationReadDbContext readDbContext,
            IAdoptionPriorityScoreCalculator calculator)
        {
            _readDbContext = readDbContext;
            _calculator = calculator;
        }

        public async ValueTask<PaginationResponse<AdoptionAnnouncementListItemResponse>> Handle(Query query,
            CancellationToken cancellationToken)
        {
            IQueryable<AdoptionAnnouncementReadModel> baseQuery = _readDbContext.AdoptionAnnouncements
                .WhereIf(query.MaybePersonId.HasValue,
                    aa => aa.PersonId == query.MaybePersonId.Value);
 
            int totalCount = await baseQuery.CountAsync(cancellationToken);
 
            if (!string.IsNullOrWhiteSpace(query.Sort))
            {
                baseQuery = baseQuery.ApplyMultipleSorting(query.Sort, GetSortProperty);
            }
 
            var rawItems = await baseQuery
                .Select(aa => new
                {
                    aa.Id,
                    aa.PersonId,
                    aa.Person.Username,
                    aa.Description,
                    aa.AddressCountryCode,
                    aa.AddressPostalCode,
                    aa.AddressRegion,
                    aa.AddressCity,
                    aa.AddressLine,
                    aa.Email,
                    aa.PhoneNumber,
                    aa.Status,
                    Cats = aa.Cats.Select(cat => new
                    {
                        cat.Name,
                        ThumbnailId = cat.Thumbnail!.Id,
                        cat.AdoptionHistoryReturnCount,
                        cat.Age,
                        cat.Color,
                        cat.Gender,
                        cat.HealthStatus,
                        cat.ListingSourceType,
                        cat.SpecialNeedsStatusSeverityType,
                        cat.Temperament,
                        cat.InfectiousDiseaseStatusFivStatus,
                        cat.InfectiousDiseaseStatusFelvStatus,
                        cat.NeuteringStatusIsNeutered
                    })
                })
                .ToListAsync(cancellationToken);
 
            IEnumerable<AdoptionAnnouncementListItemResponse> items = rawItems.Select(rawAa =>
            {
                string aaTitle = string.Join(", ", rawAa.Cats.Select(c => c.Name));
                decimal aaPriorityScore = rawAa.Cats
                    .Select(cat => _calculator.Calculate(
                        returnCount: cat.AdoptionHistoryReturnCount,
                        age: cat.Age,
                        color: cat.Color,
                        gender: cat.Gender,
                        healthStatus: cat.HealthStatus,
                        listingSourceType: cat.ListingSourceType,
                        specialNeedsSeverityType: cat.SpecialNeedsStatusSeverityType,
                        temperament: cat.Temperament,
                        fivStatus: cat.InfectiousDiseaseStatusFivStatus,
                        felvStatus: cat.InfectiousDiseaseStatusFelvStatus,
                        isNeutered: cat.NeuteringStatusIsNeutered))
                    .DefaultIfEmpty()
                    .Max();
 
                return new AdoptionAnnouncementListItemResponse(
                    Id: rawAa.Id,
                    PersonId: rawAa.PersonId,
                    Username: rawAa.Username,
                    PriorityScore: aaPriorityScore,
                    Title: aaTitle,
                    Description: rawAa.Description,
                    AddressCountryCode: rawAa.AddressCountryCode,
                    AddressPostalCode: rawAa.AddressPostalCode,
                    AddressRegion: rawAa.AddressRegion,
                    AddressCity: rawAa.AddressCity,
                    AddressLine: rawAa.AddressLine,
                    Email: rawAa.Email,
                    PhoneNumber: rawAa.PhoneNumber,
                    Status: rawAa.Status);
            });
 
            if (string.IsNullOrWhiteSpace(query.Sort))
            {
                items = items.OrderByDescending(x => x.PriorityScore);
            }
 
            List<AdoptionAnnouncementListItemResponse> paginatedItems = items
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();
 
            PaginationResponse<AdoptionAnnouncementListItemResponse> paginationResponse = new()
            {
                Items = paginatedItems,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };

            return paginationResponse;
        }
 
        private static Expression<Func<AdoptionAnnouncementReadModel, object>> GetSortProperty(string propertyName)
            => propertyName.ToLower(CultureInfo.InvariantCulture) switch
            {
                "catscount" => aa => aa.Cats.Count,
                "addresscity" => aa => aa.AddressCity,
                _ => aa => aa
            };
    }
 
    private sealed record Filters(Guid? PersonId);
 
    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("adoption-announcements", async (
            [AsParameters] PaginationAndMultipleSorting paginationAndMultipleSorting,
            [AsParameters] Filters filters,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Query query = new(
                filters.PersonId.ToMaybeId<PersonId>(),
                Page: paginationAndMultipleSorting.Page,
                PageSize: paginationAndMultipleSorting.PageSize,
                Sort: paginationAndMultipleSorting.Sort);
 
            PaginationResponse<AdoptionAnnouncementListItemResponse> response = await sender.Send(query, cancellationToken);
 
            return Results.Ok(response);
        });
    }
}
