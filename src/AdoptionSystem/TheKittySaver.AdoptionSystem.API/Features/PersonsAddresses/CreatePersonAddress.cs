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
using TheKittySaver.AdoptionSystem.Primitives.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Guards;

namespace TheKittySaver.AdoptionSystem.API.Features.PersonsAddresses;

internal sealed class CreatePersonAddress : IEndpoint
{
    internal sealed record Command(
        PersonId PersonId,
        CountryCode TwoLetterIsoCountryCode,
        string Name,
        string PostalCode,
        string Region,
        string City,
        string? Line) : ICommand<Result<AddressId>>;

    internal sealed class Handler : ICommandHandler<Command, Result<AddressId>>
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

        public async ValueTask<Result<AddressId>> Handle(Command command, CancellationToken cancellationToken)
        {
            Maybe<Person> maybePerson = await _personRepository.GetByIdAsync(command.PersonId, cancellationToken);
            if (maybePerson.HasNoValue)
            {
                return Result.Failure<AddressId>(DomainErrors.PersonEntity.NotFound(command.PersonId));
            }

            Person person = maybePerson.Value;

            Result<AddressName> createNameResult = AddressName.Create(command.Name);
            if (createNameResult.IsFailure)
            {
                return Result.Failure<AddressId>(createNameResult.Error);
            }

            Result<AddressPostalCode> createPostalCodeResult = AddressPostalCode.Create(command.PostalCode);
            if (createPostalCodeResult.IsFailure)
            {
                return Result.Failure<AddressId>(createPostalCodeResult.Error);
            }

            Result<AddressRegion> createRegionResult = AddressRegion.Create(command.Region);
            if (createRegionResult.IsFailure)
            {
                return Result.Failure<AddressId>(createRegionResult.Error);
            }

            Result<AddressCity> createCityResult = AddressCity.Create(command.City);
            if (createCityResult.IsFailure)
            {
                return Result.Failure<AddressId>(createCityResult.Error);
            }

            Maybe<AddressLine> maybeLine = Maybe<AddressLine>.None;
            if (!string.IsNullOrWhiteSpace(command.Line))
            {
                Result<AddressLine> createLineResult = AddressLine.Create(command.Line);
                if (createLineResult.IsFailure)
                {
                    return Result.Failure<AddressId>(createLineResult.Error);
                }
                maybeLine = Maybe<AddressLine>.From(createLineResult.Value);
            }

            Result<Address> addAddressResult = person.AddAddress(
                _addressConsistencySpecification,
                command.TwoLetterIsoCountryCode,
                createNameResult.Value,
                createPostalCodeResult.Value,
                createRegionResult.Value,
                createCityResult.Value,
                maybeLine);

            if (addAddressResult.IsFailure)
            {
                return Result.Failure<AddressId>(addAddressResult.Error);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return addAddressResult.Value.Id;
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("persons/{personId:guid}/addresses", async (
            Guid personId,
            CreatePersonAddressRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Command command = request.MapToCommand(new PersonId(personId));

            Result<AddressId> commandResult = await sender.Send(command, cancellationToken);

            return commandResult.IsFailure
                ? Results.Problem(commandResult.Error.ToProblemDetails())
                : Results.Created($"/api/v1/persons/{personId}/addresses/{commandResult}", commandResult.Value);
        });
    }
}

internal static class CreatePersonAddressMappings
{
    extension(CreatePersonAddressRequest request)
    {
        public CreatePersonAddress.Command MapToCommand(PersonId personId)
        {
            Ensure.NotEmpty(personId);
            ArgumentNullException.ThrowIfNull(request);

            CreatePersonAddress.Command command = new(
                PersonId: personId,
                TwoLetterIsoCountryCode: request.TwoLetterIsoCountryCode,
                Name: request.Name,
                PostalCode: request.PostalCode,
                Region: request.Region,
                City: request.City,
                Line: request.Line);
            return command;
        }
    }
}
