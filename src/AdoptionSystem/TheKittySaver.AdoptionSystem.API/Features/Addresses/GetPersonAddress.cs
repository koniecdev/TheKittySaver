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

internal sealed class GetPersonAddress : IEndpoint
{
    internal sealed record Query(
        PersonId PersonId,
        AddressId AddressId) : IQuery<Result<PersonAddressResponse>>;

    internal sealed class Handler : IQueryHandler<Query, Result<PersonAddressResponse>>
    {
        private readonly IApplicationReadDbContext _readDbContext;

        public Handler(IApplicationReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }

        public async ValueTask<Result<PersonAddressResponse>> Handle(Query query, CancellationToken cancellationToken)
        {
            AddressReadModel? address = await _readDbContext.Addresses
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    a => a.PersonId == query.PersonId && a.Id == query.AddressId,
                    cancellationToken);

            if (address is null)
            {
                return Result.Failure<PersonAddressResponse>(DomainErrors.AddressEntity.NotFound(query.AddressId));
            }

            PersonAddressResponse response = new(
                Id: address.Id,
                PersonId: address.PersonId,
                CountryCode: address.CountryCode,
                Name: address.Name,
                PostalCode: address.PostalCode,
                Region: address.Region,
                City: address.City,
                Line: address.Line);

            return response;
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("persons/{personId:guid}/addresses/{addressId:guid}", async (
            Guid personId,
            Guid addressId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Query query = new(new PersonId(personId), new AddressId(addressId));

            Result<PersonAddressResponse> queryResult = await sender.Send(query, cancellationToken);

            return queryResult.IsFailure
                ? Results.Problem(queryResult.Error.ToProblemDetails())
                : Results.Ok(queryResult.Value);
        });
    }
}
