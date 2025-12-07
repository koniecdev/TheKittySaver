using Mediator;
using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.ReadDbContexts;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.Persons;

internal sealed class GetPersons : IEndpoint
{
    internal sealed record Query : IQuery<Result<IReadOnlyList<PersonResponse>>>;

    internal sealed class Handler : IQueryHandler<Query, Result<IReadOnlyList<PersonResponse>>>
    {
        private readonly IApplicationReadDbContext _readDbContext;

        public Handler(IApplicationReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }

        public async ValueTask<Result<IReadOnlyList<PersonResponse>>> Handle(
            Query query,
            CancellationToken cancellationToken)
        {
            IReadOnlyList<PersonResponse> response = await _readDbContext.Persons
                .Select(p => new PersonResponse(
                    Id: p.Id,
                    Username: p.Username,
                    Email: p.Email,
                    PhoneNumber: p.PhoneNumber))
                .ToListAsync(cancellationToken);

            return Result.Success(response);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("persons", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Query query = new();

            Result<IReadOnlyList<PersonResponse>> queryResult = await sender.Send(query, cancellationToken);

            return Results.Ok(queryResult.Value);
        });
    }
}
