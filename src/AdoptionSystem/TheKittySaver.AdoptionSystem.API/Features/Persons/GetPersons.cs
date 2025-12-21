using System.Globalization;
using System.Linq.Expressions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Common;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.ReadDbContexts;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.Persons;

internal sealed class GetPersons : IEndpoint
{
    internal sealed record Query(int Page = 0, int PageSize = 50, string? Sort = null)
        : IQuery<PaginationResponse<PersonListItemResponse>>, IPaginationable, ISortable;

    internal sealed class Handler : IQueryHandler<Query, PaginationResponse<PersonListItemResponse>>
    {
        private readonly IApplicationReadDbContext _readDbContext;

        public Handler(IApplicationReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }

        public async ValueTask<PaginationResponse<PersonListItemResponse>> Handle(
            Query query,
            CancellationToken cancellationToken)
        {
            IQueryable<PersonReadModel> sortedQuery = _readDbContext.Persons;

            int totalCount = await sortedQuery.CountAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(query.Sort))
            {
                sortedQuery = sortedQuery.ApplyMultipleSorting(query.Sort, GetSortProperty);
            }

            IReadOnlyList<PersonListItemResponse> items = await sortedQuery
                .ApplyPagination(page: query.Page, pageSize: query.PageSize)
                .Select(p => new PersonListItemResponse(
                    Id: p.Id,
                    Username: p.Username,
                    Email: p.Email,
                    PhoneNumber: p.PhoneNumber))
                .ToListAsync(cancellationToken);

            PaginationResponse<PersonListItemResponse> response = new()
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };

            return response;
        }

        private static Expression<Func<PersonReadModel, object>> GetSortProperty(string propertyName)
            => propertyName.ToLower(CultureInfo.InvariantCulture) switch
            {
                "username" => p => p.Username,
                "email" => p => p.Email,
                _ => p => p.Username
            };
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("persons", async (
            [AsParameters] PaginationAndMultipleSorting paginationAndMultipleSorting,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Query query = new(
                Page: paginationAndMultipleSorting.Page,
                PageSize: paginationAndMultipleSorting.PageSize,
                Sort: paginationAndMultipleSorting.Sort);

            PaginationResponse<PersonListItemResponse> response = await sender.Send(query, cancellationToken);

            return Results.Ok(response);
        });
    }
}
