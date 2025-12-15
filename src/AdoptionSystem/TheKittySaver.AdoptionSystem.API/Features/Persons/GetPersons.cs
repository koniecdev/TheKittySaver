using Mediator;
using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Common.Sorting;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Common;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.ReadDbContexts;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.Persons;

internal sealed class GetPersons : IEndpoint
{
    internal sealed record Query(int Page, int PageSize, string? Sort)
        : IQuery<Result<PaginationResult<PersonResponse>>>, IPagedQuery;

    internal sealed class Handler : IQueryHandler<Query, Result<PaginationResult<PersonResponse>>>
    {
        private readonly IApplicationReadDbContext _readDbContext;

        public Handler(IApplicationReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }

        public async ValueTask<Result<PaginationResult<PersonResponse>>> Handle(
            Query query,
            CancellationToken cancellationToken)
        {
            int totalCount = await _readDbContext.Persons.CountAsync(cancellationToken);

            IOrderedQueryable<PersonReadModel>? sortedQuery = _readDbContext.Persons.ApplySortOrNull(query.Sort);

            IOrderedQueryable<PersonReadModel> orderedQuery = sortedQuery is not null
                ? sortedQuery.ThenBy(p => p.Id)
                : _readDbContext.Persons.OrderBy(p => p.Id);

            IReadOnlyList<PersonResponse> items = await orderedQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(p => new PersonResponse(
                    Id: p.Id,
                    Username: p.Username,
                    Email: p.Email,
                    PhoneNumber: p.PhoneNumber))
                .ToListAsync(cancellationToken);

            PaginationResult<PersonResponse> response = new()
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
        endpointRouteBuilder.MapGet("persons", async (
            [AsParameters] PaginationRequest pagination,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Query query = new(pagination.Page, pagination.PageSize, pagination.Sort);

            Result<PaginationResult<PersonResponse>> queryResult = await sender.Send(query, cancellationToken);

            return Results.Ok(queryResult.Value);
        });
    }
}
