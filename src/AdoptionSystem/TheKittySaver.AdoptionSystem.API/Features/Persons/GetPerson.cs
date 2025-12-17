using Mediator;
using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.ReadDbContexts;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.Persons;

internal sealed class GetPerson : IEndpoint
{
    internal sealed record Query(PersonId PersonId) : IQuery<Result<PersonResponse>>;

    internal sealed class Handler : IQueryHandler<Query, Result<PersonResponse>>
    {
        private readonly IApplicationReadDbContext _readDbContext;

        public Handler(IApplicationReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }

        public async ValueTask<Result<PersonResponse>> Handle(Query query, CancellationToken cancellationToken)
        {
            PersonResponse? response = await _readDbContext.Persons
                .Where(p => p.Id == query.PersonId)
                .Select(person => new PersonResponse(
                    Id: person.Id,
                    Username: person.Username,
                    Email: person.Email,
                    PhoneNumber: person.PhoneNumber))
                .FirstOrDefaultAsync(cancellationToken);

            return response ?? Result.Failure<PersonResponse>(
                DomainErrors.PersonEntity.NotFound(query.PersonId));
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("persons/{personId:guid}", async (
            Guid personId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Query query = new(new PersonId(personId));

            Result<PersonResponse> queryResult = await sender.Send(query, cancellationToken);

            return queryResult.IsFailure
                ? Results.Problem(queryResult.Error.ToProblemDetails())
                : Results.Ok(queryResult.Value);
        });
    }
}
