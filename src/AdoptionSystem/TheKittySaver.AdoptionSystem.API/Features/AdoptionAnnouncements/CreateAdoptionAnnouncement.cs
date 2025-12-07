using Mediator;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Responses;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.Services.AdoptionAnnouncementCreationServices;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds.Specifications;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Enums;

namespace TheKittySaver.AdoptionSystem.API.Features.AdoptionAnnouncements;

internal sealed class CreateAdoptionAnnouncement : IEndpoint
{
    internal sealed record Command(
        CatId CatId,
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
        private readonly ICatRepository _catRepository;
        private readonly IAdoptionAnnouncementRepository _adoptionAnnouncementRepository;
        private readonly IAdoptionAnnouncementCreationService _adoptionAnnouncementCreationService;
        private readonly IAddressConsistencySpecification _addressConsistencySpecification;
        private readonly IPhoneNumberFactory _phoneNumberFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public Handler(
            ICatRepository catRepository,
            IAdoptionAnnouncementRepository adoptionAnnouncementRepository,
            IAdoptionAnnouncementCreationService adoptionAnnouncementCreationService,
            IAddressConsistencySpecification addressConsistencySpecification,
            IPhoneNumberFactory phoneNumberFactory,
            IUnitOfWork unitOfWork,
            TimeProvider timeProvider)
        {
            _catRepository = catRepository;
            _adoptionAnnouncementRepository = adoptionAnnouncementRepository;
            _adoptionAnnouncementCreationService = adoptionAnnouncementCreationService;
            _addressConsistencySpecification = addressConsistencySpecification;
            _phoneNumberFactory = phoneNumberFactory;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public async ValueTask<Result<AdoptionAnnouncementResponse>> Handle(Command command, CancellationToken cancellationToken)
        {
            Maybe<Cat> maybeCat = await _catRepository.GetByIdAsync(command.CatId, cancellationToken);
            if (maybeCat.HasNoValue)
            {
                return Result.Failure<AdoptionAnnouncementResponse>(DomainErrors.CatEntity.NotFound(command.CatId));
            }

            Cat cat = maybeCat.Value;

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

            Result<Email> createEmailResult = Email.Create(command.Email);
            if (createEmailResult.IsFailure)
            {
                return Result.Failure<AdoptionAnnouncementResponse>(createEmailResult.Error);
            }

            Result<PhoneNumber> createPhoneNumberResult = _phoneNumberFactory.Create(command.PhoneNumber);
            if (createPhoneNumberResult.IsFailure)
            {
                return Result.Failure<AdoptionAnnouncementResponse>(createPhoneNumberResult.Error);
            }

            Result<AdoptionAnnouncement> createAnnouncementResult = _adoptionAnnouncementCreationService.Create(
                cat,
                createAddressResult.Value,
                createEmailResult.Value,
                createPhoneNumberResult.Value,
                maybeDescription,
                _timeProvider.GetUtcNow());

            if (createAnnouncementResult.IsFailure)
            {
                return Result.Failure<AdoptionAnnouncementResponse>(createAnnouncementResult.Error);
            }

            AdoptionAnnouncement announcement = createAnnouncementResult.Value;

            _adoptionAnnouncementRepository.Insert(announcement);
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
        endpointRouteBuilder.MapPost("adoption-announcements", async (
            CreateAdoptionAnnouncementRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Command command = request.MapToCommand();

            Result<AdoptionAnnouncementResponse> commandResult = await sender.Send(command, cancellationToken);

            return commandResult.IsFailure
                ? Results.Problem(commandResult.Error.ToProblemDetails())
                : Results.Created($"/api/v1/adoption-announcements/{commandResult.Value.Id}", commandResult.Value);
        });
    }
}

internal static class CreateAdoptionAnnouncementMappings
{
    extension(CreateAdoptionAnnouncementRequest request)
    {
        public CreateAdoptionAnnouncement.Command MapToCommand()
        {
            CreateAdoptionAnnouncement.Command command = new(
                CatId: request.CatId,
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
