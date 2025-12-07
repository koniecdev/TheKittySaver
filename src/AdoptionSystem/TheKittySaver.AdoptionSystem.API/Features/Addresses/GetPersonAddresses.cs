using Mediator;
using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.ReadDbContexts;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.Addresses;

internal sealed class GetPersonAddresses : IEndpoint
{
    internal sealed record Query(PersonId PersonId) : IQuery<Result<IReadOnlyList<PersonAddressResponse>>>;

    internal sealed class Handler : IQueryHandler<Query, Result<IReadOnlyList<PersonAddressResponse>>>
    {
        private readonly IApplicationReadDbContext _readDbContext;

        public Handler(IApplicationReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }

        public async ValueTask<Result<IReadOnlyList<PersonAddressResponse>>> Handle(Query query, CancellationToken cancellationToken)
        {
            bool personExists = await _readDbContext.Persons
                .AnyAsync(p => p.Id == query.PersonId, cancellationToken);

            if (!personExists)
            {
                return Result.Failure<IReadOnlyList<PersonAddressResponse>>(
                    DomainErrors.PersonEntity.NotFound(query.PersonId));
            }
            
            IReadOnlyList<PersonAddressResponse> response = await _readDbContext.Addresses
                .Where(a => a.PersonId == query.PersonId)
                .Select(a => new PersonAddressResponse(
                    Id: a.Id,
                    PersonId: a.PersonId,
                    CountryCode: a.CountryCode,
                    Name: a.Name,
                    PostalCode: a.PostalCode,
                    Region: a.Region,
                    City: a.City,
                    Line: a.Line))
                .ToListAsync(cancellationToken);

            return Result.Success(response);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("persons/{personId:guid}/addresses", async (
            Guid personId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Query query = new(new PersonId(personId));

            Result<IReadOnlyList<PersonAddressResponse>> queryResult = await sender.Send(query, cancellationToken);

            return queryResult.IsFailure
                ? Results.Problem(queryResult.Error.ToProblemDetails())
                : Results.Ok(queryResult.Value);
        });
    }
}
