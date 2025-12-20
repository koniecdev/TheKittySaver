using System.Globalization;
using System.Linq.Expressions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
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
        int Page = 0,
        int PageSize = 50,
        string? Sort = null)
        : IQuery<PaginationResponse<AdoptionAnnouncementResponse>>, IPaginationable, ISortable;

    internal sealed class Handler : IQueryHandler<Query, PaginationResponse<AdoptionAnnouncementResponse>>
    {
        private readonly IApplicationReadDbContext _readDbContext;

        public Handler(IApplicationReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }

        public async ValueTask<PaginationResponse<AdoptionAnnouncementResponse>> Handle(Query query,
            CancellationToken cancellationToken)
        {
            IQueryable<AdoptionAnnouncementReadModel> sortedQuery = _readDbContext.AdoptionAnnouncements
                .WhereIf(query.MaybePersonId.HasValue,
                    aa => aa.PersonId == query.MaybePersonId.Value);

            int totalCount = await sortedQuery.CountAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(query.Sort))
            {
                sortedQuery = sortedQuery.ApplyMultipleSorting(query.Sort, GetSortProperty);
            }
            
            IReadOnlyList<AdoptionAnnouncementResponse> items = await sortedQuery
                .ApplyPagination(page: query.Page, pageSize: query.PageSize)
                .Select(a => new AdoptionAnnouncementResponse(
                    Id: a.Id,
                    PersonId: a.PersonId,
                    Description: a.Description,
                    AddressCountryCode: a.AddressCountryCode,
                    AddressPostalCode: a.AddressPostalCode,
                    AddressRegion: a.AddressRegion,
                    AddressCity: a.AddressCity,
                    AddressLine: a.AddressLine,
                    Email: a.Email,
                    PhoneNumber: a.PhoneNumber,
                    Status: a.Status))
                .ToListAsync(cancellationToken);

            return new PaginationResponse<AdoptionAnnouncementResponse>
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }
        
        private static Expression<Func<AdoptionAnnouncementReadModel, object>> GetSortProperty(string propertyName)
            => propertyName.ToLower(CultureInfo.InvariantCulture) switch
            {
                "catscount" => aa => aa.Cats.Count,
                "addresscity" => aa => aa.AddressCity,
                _ => aa => aa.AddressCity
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

            PaginationResponse<AdoptionAnnouncementResponse> response = await sender.Send(query, cancellationToken);

            return Results.Ok(response);
        });
    }
}
