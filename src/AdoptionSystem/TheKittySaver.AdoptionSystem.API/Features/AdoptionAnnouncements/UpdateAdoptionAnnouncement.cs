using Mediator;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Requests;
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
using TheKittySaver.AdoptionSystem.Primitives.Guards;

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
        string PhoneNumber) : ICommand<Result>;

    internal sealed class Handler : ICommandHandler<Command, Result>
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

        public async ValueTask<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            Maybe<AdoptionAnnouncement> maybeAnnouncement = await _adoptionAnnouncementRepository.GetByIdAsync(
                command.AdoptionAnnouncementId,
                cancellationToken);

            if (maybeAnnouncement.HasNoValue)
            {
                return Result.Failure(
                    DomainErrors.AdoptionAnnouncementEntity.NotFound(command.AdoptionAnnouncementId));
            }

            AdoptionAnnouncement announcement = maybeAnnouncement.Value;

            Maybe<AdoptionAnnouncementDescription> maybeDescription = Maybe<AdoptionAnnouncementDescription>.None;
            if (!string.IsNullOrWhiteSpace(command.Description))
            {
                Result<AdoptionAnnouncementDescription> createDescriptionResult = 
                    AdoptionAnnouncementDescription.Create(command.Description);
                if (createDescriptionResult.IsFailure)
                {
                    return Result.Failure(createDescriptionResult.Error);
                }
                maybeDescription = Maybe<AdoptionAnnouncementDescription>.From(createDescriptionResult.Value);
            }

            Result updateDescriptionResult = announcement.UpdateDescription(maybeDescription);
            if (updateDescriptionResult.IsFailure)
            {
                return updateDescriptionResult;
            }

            Result<AddressPostalCode> createPostalCodeResult = AddressPostalCode.Create(command.AddressPostalCode);
            if (createPostalCodeResult.IsFailure)
            {
                return Result.Failure(createPostalCodeResult.Error);
            }

            Result<AddressRegion> createRegionResult = AddressRegion.Create(command.AddressRegion);
            if (createRegionResult.IsFailure)
            {
                return Result.Failure(createRegionResult.Error);
            }

            Result<AddressCity> createCityResult = AddressCity.Create(command.AddressCity);
            if (createCityResult.IsFailure)
            {
                return Result.Failure(createCityResult.Error);
            }

            Maybe<AddressLine> maybeLine = Maybe<AddressLine>.None;
            if (!string.IsNullOrWhiteSpace(command.AddressLine))
            {
                Result<AddressLine> createLineResult = AddressLine.Create(command.AddressLine);
                if (createLineResult.IsFailure)
                {
                    return Result.Failure(createLineResult.Error);
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
                return Result.Failure(createAddressResult.Error);
            }

            Result updateAddressResult = announcement.UpdateAddress(createAddressResult.Value);
            if (updateAddressResult.IsFailure)
            {
                return updateAddressResult;
            }

            Result<Email> createEmailResult = Email.Create(command.Email);
            if (createEmailResult.IsFailure)
            {
                return Result.Failure(createEmailResult.Error);
            }

            Result updateEmailResult = announcement.UpdateEmail(createEmailResult.Value);
            if (updateEmailResult.IsFailure)
            {
                return updateEmailResult;
            }

            Result<PhoneNumber> createPhoneNumberResult = _phoneNumberFactory.Create(command.PhoneNumber);
            if (createPhoneNumberResult.IsFailure)
            {
                return Result.Failure(createPhoneNumberResult.Error);
            }

            Result updatePhoneNumberResult = announcement.UpdatePhoneNumber(createPhoneNumberResult.Value);
            if (updatePhoneNumberResult.IsFailure)
            {
                return updatePhoneNumberResult;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
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

            Result commandResult = await sender.Send(command, cancellationToken);

            return commandResult.IsFailure
                ? Results.Problem(commandResult.Error.ToProblemDetails())
                : Results.NoContent();
        });
    }
}

internal static class UpdateAdoptionAnnouncementMappings
{
    extension(UpdateAdoptionAnnouncementRequest request)
    {
        public UpdateAdoptionAnnouncement.Command MapToCommand(AdoptionAnnouncementId adoptionAnnouncementId)
        {
            Ensure.NotEmpty(adoptionAnnouncementId);
            ArgumentNullException.ThrowIfNull(request);
            
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
