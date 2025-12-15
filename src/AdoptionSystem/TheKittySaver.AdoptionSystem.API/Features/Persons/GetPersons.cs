using Mediator;
using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Common;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.ReadDbContexts;

namespace TheKittySaver.AdoptionSystem.API.Features.Persons;

internal sealed class GetPersons : IEndpoint
{
    internal sealed record Query(int Page, int PageSize) : IQuery<Result<PaginationResult<PersonResponse>>>;

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

            IReadOnlyList<PersonResponse> items = await _readDbContext.Persons
                .OrderBy(p => p.Id)
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
            Query query = new(pagination.Page, pagination.PageSize);

            Result<PaginationResult<PersonResponse>> queryResult = await sender.Send(query, cancellationToken);

            return Results.Ok(queryResult.Value);
        });
    }
}
