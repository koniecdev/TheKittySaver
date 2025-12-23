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
    internal sealed record Query(PersonId PersonId) : IQuery<Result<PersonDetailsResponse>>;

    internal sealed class Handler : IQueryHandler<Query, Result<PersonDetailsResponse>>
    {
        private readonly IApplicationReadDbContext _readDbContext;

        public Handler(IApplicationReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }

        public async ValueTask<Result<PersonDetailsResponse>> Handle(Query query, CancellationToken cancellationToken)
        {
            PersonDetailsResponse? response = await _readDbContext.Persons
                .Where(p => p.Id == query.PersonId)
                .Select(person => new PersonDetailsResponse(
                    Id: person.Id,
                    Username: person.Username,
                    Email: person.Email,
                    PhoneNumber: person.PhoneNumber,
                    CreatedAt: person.CreatedAt,
                    Addresses: person.Addresses
                        .Select(a => new PersonAddressEmbeddedDto(
                            Id: a.Id,
                            CountryCode: a.CountryCode,
                            Name: a.Name,
                            PostalCode: a.PostalCode,
                            Region: a.Region,
                            City: a.City,
                            Line: a.Line))
                        .ToList()))
                .FirstOrDefaultAsync(cancellationToken);

            if (response is null)
            {
                return Result.Failure<PersonDetailsResponse>(
                    DomainErrors.PersonEntity.NotFound(query.PersonId));
            }

            return Result.Success(response);
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

            Result<PersonDetailsResponse> queryResult = await sender.Send(query, cancellationToken);

            return queryResult.IsFailure
                ? Results.Problem(queryResult.Error.ToProblemDetails())
                : Results.Ok(queryResult.Value);
        });
    }
}
