using Mediator;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Responses;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds.Specifications;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Enums;

namespace TheKittySaver.AdoptionSystem.API.Features.AdoptionAnnouncements;

internal sealed class UpdateAdoptionAnnouncement : IEndpoint
{
    internal sealed record Command(
        AdoptionAnnouncementId AdoptionAnnouncementId,
        string? Description,
        CountryCode AddressCountryCode,
        string AddressPostalCode,
        string AddressRegion,
        string AddressCity,
        string? AddressLine,
        string Email,
        string PhoneNumber) : ICommand<Result<AdoptionAnnouncementResponse>>;

    internal sealed class Handler : ICommandHandler<Command, Result<AdoptionAnnouncementResponse>>
    {
        private readonly IAdoptionAnnouncementRepository _adoptionAnnouncementRepository;
        private readonly IAddressConsistencySpecification _addressConsistencySpecification;
        private readonly IPhoneNumberFactory _phoneNumberFactory;
        private readonly IUnitOfWork _unitOfWork;

        public Handler(
            IAdoptionAnnouncementRepository adoptionAnnouncementRepository,
            IAddressConsistencySpecification addressConsistencySpecification,
            IPhoneNumberFactory phoneNumberFactory,
            IUnitOfWork unitOfWork)
        {
            _adoptionAnnouncementRepository = adoptionAnnouncementRepository;
            _addressConsistencySpecification = addressConsistencySpecification;
            _phoneNumberFactory = phoneNumberFactory;
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<Result<AdoptionAnnouncementResponse>> Handle(Command command, CancellationToken cancellationToken)
        {
            Maybe<AdoptionAnnouncement> maybeAnnouncement = await _adoptionAnnouncementRepository.GetByIdAsync(
                command.AdoptionAnnouncementId,
                cancellationToken);

            if (maybeAnnouncement.HasNoValue)
            {
                return Result.Failure<AdoptionAnnouncementResponse>(
                    DomainErrors.AdoptionAnnouncementErrors.NotFound(command.AdoptionAnnouncementId));
            }

            AdoptionAnnouncement announcement = maybeAnnouncement.Value;

            Maybe<AdoptionAnnouncementDescription> maybeDescription = Maybe<AdoptionAnnouncementDescription>.None;
            if (!string.IsNullOrWhiteSpace(command.Description))
            {
                Result<AdoptionAnnouncementDescription> createDescriptionResult = AdoptionAnnouncementDescription.Create(command.Description);
                if (createDescriptionResult.IsFailure)
                {
                    return Result.Failure<AdoptionAnnouncementResponse>(createDescriptionResult.Error);
                }
                maybeDescription = Maybe<AdoptionAnnouncementDescription>.From(createDescriptionResult.Value);
            }

            Result updateDescriptionResult = announcement.UpdateDescription(maybeDescription);
            if (updateDescriptionResult.IsFailure)
            {
                return Result.Failure<AdoptionAnnouncementResponse>(updateDescriptionResult.Error);
            }

            Result<AddressPostalCode> createPostalCodeResult = AddressPostalCode.Create(command.AddressPostalCode);
            if (createPostalCodeResult.IsFailure)
            {
                return Result.Failure<AdoptionAnnouncementResponse>(createPostalCodeResult.Error);
            }

            Result<AddressRegion> createRegionResult = AddressRegion.Create(command.AddressRegion);
            if (createRegionResult.IsFailure)
            {
                return Result.Failure<AdoptionAnnouncementResponse>(createRegionResult.Error);
            }

            Result<AddressCity> createCityResult = AddressCity.Create(command.AddressCity);
            if (createCityResult.IsFailure)
            {
                return Result.Failure<AdoptionAnnouncementResponse>(createCityResult.Error);
            }

            Maybe<AddressLine> maybeLine = Maybe<AddressLine>.None;
            if (!string.IsNullOrWhiteSpace(command.AddressLine))
            {
                Result<AddressLine> createLineResult = AddressLine.Create(command.AddressLine);
                if (createLineResult.IsFailure)
                {
                    return Result.Failure<AdoptionAnnouncementResponse>(createLineResult.Error);
                }
                maybeLine = Maybe<AddressLine>.From(createLineResult.Value);
            }

            Result<AdoptionAnnouncementAddress> createAddressResult = AdoptionAnnouncementAddress.Create(
                _addressConsistencySpecification,
                command.AddressCountryCode,
                createPostalCodeResult.Value,
                createRegionResult.Value,
                createCityResult.Value,
                maybeLine);
            if (createAddressResult.IsFailure)
            {
                return Result.Failure<AdoptionAnnouncementResponse>(createAddressResult.Error);
            }

            Result updateAddressResult = announcement.UpdateAddress(createAddressResult.Value);
            if (updateAddressResult.IsFailure)
            {
                return Result.Failure<AdoptionAnnouncementResponse>(updateAddressResult.Error);
            }

            Result<Email> createEmailResult = Email.Create(command.Email);
            if (createEmailResult.IsFailure)
            {
                return Result.Failure<AdoptionAnnouncementResponse>(createEmailResult.Error);
            }

            Result updateEmailResult = announcement.UpdateEmail(createEmailResult.Value);
            if (updateEmailResult.IsFailure)
            {
                return Result.Failure<AdoptionAnnouncementResponse>(updateEmailResult.Error);
            }

            Result<PhoneNumber> createPhoneNumberResult = _phoneNumberFactory.Create(command.PhoneNumber);
            if (createPhoneNumberResult.IsFailure)
            {
                return Result.Failure<AdoptionAnnouncementResponse>(createPhoneNumberResult.Error);
            }

            Result updatePhoneNumberResult = announcement.UpdatePhoneNumber(createPhoneNumberResult.Value);
            if (updatePhoneNumberResult.IsFailure)
            {
                return Result.Failure<AdoptionAnnouncementResponse>(updatePhoneNumberResult.Error);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            AdoptionAnnouncementResponse response = new(
                Id: announcement.Id,
                PersonId: announcement.PersonId,
                Description: announcement.Description?.Value,
                AddressCountryCode: announcement.Address.CountryCode,
                AddressPostalCode: announcement.Address.PostalCode.Value,
                AddressRegion: announcement.Address.Region.Value,
                AddressCity: announcement.Address.City.Value,
                AddressLine: announcement.Address.Line?.Value,
                Email: announcement.Email.Value,
                PhoneNumber: announcement.PhoneNumber.Value,
                Status: announcement.Status);

            return response;
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPut("adoption-announcements/{adoptionAnnouncementId:guid}", async (
            Guid adoptionAnnouncementId,
            UpdateAdoptionAnnouncementRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Command command = request.MapToCommand(new AdoptionAnnouncementId(adoptionAnnouncementId));

            Result<AdoptionAnnouncementResponse> commandResult = await sender.Send(command, cancellationToken);

            return commandResult.IsFailure
                ? Results.Problem(commandResult.Error.ToProblemDetails())
                : Results.Ok(commandResult.Value);
        });
    }
}

internal static class UpdateAdoptionAnnouncementMappings
{
    extension(UpdateAdoptionAnnouncementRequest request)
    {
        public UpdateAdoptionAnnouncement.Command MapToCommand(AdoptionAnnouncementId adoptionAnnouncementId)
        {
            UpdateAdoptionAnnouncement.Command command = new(
                AdoptionAnnouncementId: adoptionAnnouncementId,
                Description: request.Description,
                AddressCountryCode: request.AddressCountryCode,
                AddressPostalCode: request.AddressPostalCode,
                AddressRegion: request.AddressRegion,
                AddressCity: request.AddressCity,
                AddressLine: request.AddressLine,
                Email: request.Email,
                PhoneNumber: request.PhoneNumber);
            return command;
        }
    }
}
