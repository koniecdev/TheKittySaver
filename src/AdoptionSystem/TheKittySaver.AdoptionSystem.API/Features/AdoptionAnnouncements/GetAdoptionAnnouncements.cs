using Mediator;
using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Common.Sorting;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Common;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.ReadDbContexts;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.AdoptionAnnouncementAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.AdoptionAnnouncements;

internal sealed class GetAdoptionAnnouncements : IEndpoint
{
    internal sealed record Query(int Page, int PageSize, string? Sort)
        : IQuery<Result<PaginationResult<AdoptionAnnouncementResponse>>>, IPagedQuery;

    internal sealed class Handler : IQueryHandler<Query, Result<PaginationResult<AdoptionAnnouncementResponse>>>
    {
        private readonly IApplicationReadDbContext _readDbContext;

        public Handler(IApplicationReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }

        public async ValueTask<Result<PaginationResult<AdoptionAnnouncementResponse>>> Handle(Query query, CancellationToken cancellationToken)
        {
            int totalCount = await _readDbContext.AdoptionAnnouncements.CountAsync(cancellationToken);

            IOrderedQueryable<AdoptionAnnouncementReadModel>? sortedQuery =
                _readDbContext.AdoptionAnnouncements.ApplySortOrNull(query.Sort);

            IOrderedQueryable<AdoptionAnnouncementReadModel> orderedQuery = sortedQuery is not null
                ? sortedQuery.ThenBy(a => a.Id)
                : _readDbContext.AdoptionAnnouncements.OrderBy(a => a.Id);

            IReadOnlyList<AdoptionAnnouncementResponse> items = await orderedQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
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

            PaginationResult<AdoptionAnnouncementResponse> response = new()
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
        endpointRouteBuilder.MapGet("adoption-announcements", async (
            [AsParameters] PaginationRequest pagination,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Query query = new(pagination.Page, pagination.PageSize, pagination.Sort);

            Result<PaginationResult<AdoptionAnnouncementResponse>> queryResult = await sender.Send(query, cancellationToken);

            return Results.Ok(queryResult.Value);
        });
    }
}
