using Mediator;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds.Specifications;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.PersonsAddresses;

internal sealed class UpdatePersonAddress : IEndpoint
{
    internal sealed record Command(
        PersonId PersonId,
        AddressId AddressId,
        string Name,
        string PostalCode,
        string Region,
        string City,
        string? Line) : ICommand<Result<PersonAddressResponse>>;

    internal sealed class Handler : ICommandHandler<Command, Result<PersonAddressResponse>>
    {
        private readonly IPersonRepository _personRepository;
        private readonly IAddressConsistencySpecification _addressConsistencySpecification;
        private readonly IUnitOfWork _unitOfWork;

        public Handler(
            IPersonRepository personRepository,
            IAddressConsistencySpecification addressConsistencySpecification,
            IUnitOfWork unitOfWork)
        {
            _personRepository = personRepository;
            _addressConsistencySpecification = addressConsistencySpecification;
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<Result<PersonAddressResponse>> Handle(Command command, CancellationToken cancellationToken)
        {
            Maybe<Person> maybePerson = await _personRepository.GetByIdAsync(command.PersonId, cancellationToken);
            if (maybePerson.HasNoValue)
            {
                return Result.Failure<PersonAddressResponse>(DomainErrors.PersonEntity.NotFound(command.PersonId));
            }

            Person person = maybePerson.Value;

            Result<AddressName> createNameResult = AddressName.Create(command.Name);
            if (createNameResult.IsFailure)
            {
                return Result.Failure<PersonAddressResponse>(createNameResult.Error);
            }

            Result<AddressPostalCode> createPostalCodeResult = AddressPostalCode.Create(command.PostalCode);
            if (createPostalCodeResult.IsFailure)
            {
                return Result.Failure<PersonAddressResponse>(createPostalCodeResult.Error);
            }

            Result<AddressRegion> createRegionResult = AddressRegion.Create(command.Region);
            if (createRegionResult.IsFailure)
            {
                return Result.Failure<PersonAddressResponse>(createRegionResult.Error);
            }

            Result<AddressCity> createCityResult = AddressCity.Create(command.City);
            if (createCityResult.IsFailure)
            {
                return Result.Failure<PersonAddressResponse>(createCityResult.Error);
            }

            Maybe<AddressLine> maybeLine = Maybe<AddressLine>.None;
            if (!string.IsNullOrWhiteSpace(command.Line))
            {
                Result<AddressLine> createLineResult = AddressLine.Create(command.Line);
                if (createLineResult.IsFailure)
                {
                    return Result.Failure<PersonAddressResponse>(createLineResult.Error);
                }
                maybeLine = Maybe<AddressLine>.From(createLineResult.Value);
            }

            Result updateNameResult = person.UpdateAddressName(command.AddressId, createNameResult.Value);
            if (updateNameResult.IsFailure)
            {
                return Result.Failure<PersonAddressResponse>(updateNameResult.Error);
            }

            Result updateDetailsResult = person.UpdateAddressDetails(
                _addressConsistencySpecification,
                command.AddressId,
                createPostalCodeResult.Value,
                createRegionResult.Value,
                createCityResult.Value,
                maybeLine);

            if (updateDetailsResult.IsFailure)
            {
                return Result.Failure<PersonAddressResponse>(updateDetailsResult.Error);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            Address address = person.Addresses.First(a => a.Id == command.AddressId);

            PersonAddressResponse response = new(
                Id: address.Id,
                PersonId: person.Id,
                CountryCode: address.CountryCode,
                Name: address.Name.Value,
                PostalCode: address.PostalCode.Value,
                Region: address.Region.Value,
                City: address.City.Value,
                Line: address.Line?.Value);

            return response;
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPut("persons/{personId:guid}/addresses/{addressId:guid}", async (
            Guid personId,
            Guid addressId,
            UpdatePersonAddressRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Command command = request.MapToCommand(new PersonId(personId), new AddressId(addressId));

            Result<PersonAddressResponse> commandResult = await sender.Send(command, cancellationToken);

            return commandResult.IsFailure
                ? Results.Problem(commandResult.Error.ToProblemDetails())
                : Results.Ok(commandResult.Value);
        });
    }
}

internal static class UpdatePersonAddressMappings
{
    extension(UpdatePersonAddressRequest request)
    {
        public UpdatePersonAddress.Command MapToCommand(PersonId personId, AddressId addressId)
        {
            UpdatePersonAddress.Command command = new(
                PersonId: personId,
                AddressId: addressId,
                Name: request.Name,
                PostalCode: request.PostalCode,
                Region: request.Region,
                City: request.City,
                Line: request.Line);
            return command;
        }
    }
}
