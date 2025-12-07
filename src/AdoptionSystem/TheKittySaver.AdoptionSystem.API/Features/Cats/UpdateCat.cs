using Mediator;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Responses;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.API.Features.Cats;

internal sealed class UpdateCat : IEndpoint
{
    internal sealed record Command(
        CatId CatId,
        string Name,
        string Description,
        int Age,
        CatGenderType Gender,
        ColorType Color,
        decimal WeightValueInKilograms,
        HealthStatusType HealthStatus,
        bool HasSpecialNeeds,
        string? SpecialNeedsDescription,
        SpecialNeedsSeverityType SpecialNeedsSeverityType,
        TemperamentType Temperament,
        int AdoptionHistoryReturnCount,
        DateTimeOffset? AdoptionHistoryLastReturnDate,
        string? AdoptionHistoryLastReturnReason,
        ListingSourceType ListingSourceType,
        string ListingSourceSourceName,
        bool IsNeutered,
        FivStatus FivStatus,
        FelvStatus FelvStatus,
        DateOnly InfectiousDiseaseStatusLastTestedAt) : ICommand<Result<CatResponse>>;

    internal sealed class Handler : ICommandHandler<Command, Result<CatResponse>>
    {
        private readonly ICatRepository _catRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public Handler(
            ICatRepository catRepository,
            IUnitOfWork unitOfWork,
            TimeProvider timeProvider)
        {
            _catRepository = catRepository;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public async ValueTask<Result<CatResponse>> Handle(Command command, CancellationToken cancellationToken)
        {
            Maybe<Cat> maybeCat = await _catRepository.GetByIdAsync(command.CatId, cancellationToken);
            if (maybeCat.HasNoValue)
            {
                return Result.Failure<CatResponse>(DomainErrors.CatEntity.NotFound(command.CatId));
            }

            Cat cat = maybeCat.Value;

            Result<CatName> createNameResult = CatName.Create(command.Name);
            if (createNameResult.IsFailure)
            {
                return Result.Failure<CatResponse>(createNameResult.Error);
            }

            Result updateNameResult = cat.UpdateName(createNameResult.Value);
            if (updateNameResult.IsFailure)
            {
                return Result.Failure<CatResponse>(updateNameResult.Error);
            }

            Result<CatDescription> createDescriptionResult = CatDescription.Create(command.Description);
            if (createDescriptionResult.IsFailure)
            {
                return Result.Failure<CatResponse>(createDescriptionResult.Error);
            }

            Result updateDescriptionResult = cat.UpdateDescription(createDescriptionResult.Value);
            if (updateDescriptionResult.IsFailure)
            {
                return Result.Failure<CatResponse>(updateDescriptionResult.Error);
            }

            Result<CatAge> createAgeResult = CatAge.Create(command.Age);
            if (createAgeResult.IsFailure)
            {
                return Result.Failure<CatResponse>(createAgeResult.Error);
            }

            Result updateAgeResult = cat.UpdateAge(createAgeResult.Value);
            if (updateAgeResult.IsFailure)
            {
                return Result.Failure<CatResponse>(updateAgeResult.Error);
            }

            CatGender gender = command.Gender == CatGenderType.Male
                ? CatGender.Male()
                : CatGender.Female();

            Result updateGenderResult = cat.UpdateGender(gender);
            if (updateGenderResult.IsFailure)
            {
                return Result.Failure<CatResponse>(updateGenderResult.Error);
            }

            CatColor color = command.Color switch
            {
                ColorType.Black => CatColor.Black(),
                ColorType.White => CatColor.White(),
                ColorType.Orange => CatColor.Orange(),
                ColorType.Gray => CatColor.Gray(),
                ColorType.Tabby => CatColor.Tabby(),
                ColorType.Calico => CatColor.Calico(),
                ColorType.Tortoiseshell => CatColor.Tortoiseshell(),
                ColorType.BlackAndWhite => CatColor.BlackAndWhite(),
                _ => CatColor.Other()
            };

            Result updateColorResult = cat.UpdateColor(color);
            if (updateColorResult.IsFailure)
            {
                return Result.Failure<CatResponse>(updateColorResult.Error);
            }

            Result<CatWeight> createWeightResult = CatWeight.Create(command.WeightValueInKilograms);
            if (createWeightResult.IsFailure)
            {
                return Result.Failure<CatResponse>(createWeightResult.Error);
            }

            Result updateWeightResult = cat.UpdateWeight(createWeightResult.Value);
            if (updateWeightResult.IsFailure)
            {
                return Result.Failure<CatResponse>(updateWeightResult.Error);
            }

            HealthStatus healthStatus = command.HealthStatus switch
            {
                HealthStatusType.Healthy => HealthStatus.Healthy(),
                HealthStatusType.MinorIssues => HealthStatus.MinorIssues(),
                HealthStatusType.Recovering => HealthStatus.Recovering(),
                HealthStatusType.ChronicIllness => HealthStatus.ChronicIllness(),
                _ => HealthStatus.Critical()
            };

            Result updateHealthStatusResult = cat.UpdateHealthStatus(healthStatus);
            if (updateHealthStatusResult.IsFailure)
            {
                return Result.Failure<CatResponse>(updateHealthStatusResult.Error);
            }

            SpecialNeedsStatus specialNeeds;
            if (command.HasSpecialNeeds)
            {
                Result<SpecialNeedsStatus> createSpecialNeedsResult = SpecialNeedsStatus.Create(
                    command.SpecialNeedsDescription ?? string.Empty,
                    command.SpecialNeedsSeverityType);
                if (createSpecialNeedsResult.IsFailure)
                {
                    return Result.Failure<CatResponse>(createSpecialNeedsResult.Error);
                }
                specialNeeds = createSpecialNeedsResult.Value;
            }
            else
            {
                specialNeeds = SpecialNeedsStatus.None();
            }

            Result updateSpecialNeedsResult = cat.UpdateSpecialNeeds(specialNeeds);
            if (updateSpecialNeedsResult.IsFailure)
            {
                return Result.Failure<CatResponse>(updateSpecialNeedsResult.Error);
            }

            Temperament temperament = command.Temperament switch
            {
                TemperamentType.Friendly => Temperament.Friendly(),
                TemperamentType.Independent => Temperament.Independent(),
                TemperamentType.Timid => Temperament.Timid(),
                TemperamentType.VeryTimid => Temperament.VeryTimid(),
                _ => Temperament.Aggressive()
            };

            Result updateTemperamentResult = cat.UpdateTemperament(temperament);
            if (updateTemperamentResult.IsFailure)
            {
                return Result.Failure<CatResponse>(updateTemperamentResult.Error);
            }

            AdoptionHistory adoptionHistory;
            if (command is { AdoptionHistoryReturnCount: > 0, AdoptionHistoryLastReturnDate: not null } 
                && !string.IsNullOrWhiteSpace(command.AdoptionHistoryLastReturnReason))
            {
                Result<AdoptionHistory> createAdoptionHistoryResult = AdoptionHistory.CatHasBeenReturned(
                    command.AdoptionHistoryReturnCount,
                    _timeProvider.GetUtcNow(),
                    command.AdoptionHistoryLastReturnDate.Value,
                    command.AdoptionHistoryLastReturnReason);
                if (createAdoptionHistoryResult.IsFailure)
                {
                    return Result.Failure<CatResponse>(createAdoptionHistoryResult.Error);
                }
                adoptionHistory = createAdoptionHistoryResult.Value;
            }
            else
            {
                adoptionHistory = AdoptionHistory.CatHasNeverBeenAdopted;
            }

            Result updateAdoptionHistoryResult = cat.UpdateAdoptionHistory(adoptionHistory);
            if (updateAdoptionHistoryResult.IsFailure)
            {
                return Result.Failure<CatResponse>(updateAdoptionHistoryResult.Error);
            }

            Result<ListingSource> createListingSourceResult = command.ListingSourceType switch
            {
                ListingSourceType.PrivatePerson => ListingSource.PrivatePerson(command.ListingSourceSourceName),
                ListingSourceType.PrivatePersonUrgent => ListingSource.PrivatePerson(command.ListingSourceSourceName, isUrgent: true),
                ListingSourceType.Shelter => ListingSource.Shelter(command.ListingSourceSourceName),
                ListingSourceType.Foundation => ListingSource.Foundation(command.ListingSourceSourceName),
                _ => ListingSource.PrivatePerson(command.ListingSourceSourceName)
            };
            if (createListingSourceResult.IsFailure)
            {
                return Result.Failure<CatResponse>(createListingSourceResult.Error);
            }

            Result updateListingSourceResult = cat.UpdateListingSource(createListingSourceResult.Value);
            if (updateListingSourceResult.IsFailure)
            {
                return Result.Failure<CatResponse>(updateListingSourceResult.Error);
            }

            NeuteringStatus neuteringStatus = command.IsNeutered
                ? NeuteringStatus.Neutered()
                : NeuteringStatus.NotNeutered();

            Result updateNeuteringStatusResult = cat.UpdateNeuteringStatus(neuteringStatus);
            if (updateNeuteringStatusResult.IsFailure)
            {
                return Result.Failure<CatResponse>(updateNeuteringStatusResult.Error);
            }

            DateOnly currentDate = DateOnly.FromDateTime(_timeProvider.GetUtcNow().DateTime);
            Result<InfectiousDiseaseStatus> createInfectiousDiseaseStatusResult = InfectiousDiseaseStatus.Create(
                command.FivStatus,
                command.FelvStatus,
                command.InfectiousDiseaseStatusLastTestedAt,
                currentDate);
            if (createInfectiousDiseaseStatusResult.IsFailure)
            {
                return Result.Failure<CatResponse>(createInfectiousDiseaseStatusResult.Error);
            }

            Result updateInfectiousDiseaseStatusResult = cat.UpdateInfectiousDiseaseStatus(createInfectiousDiseaseStatusResult.Value);
            if (updateInfectiousDiseaseStatusResult.IsFailure)
            {
                return Result.Failure<CatResponse>(updateInfectiousDiseaseStatusResult.Error);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            CatResponse response = new(
                Id: cat.Id,
                PersonId: cat.PersonId,
                AdoptionAnnouncementId: cat.AdoptionAnnouncementId,
                Name: cat.Name.Value,
                Description: cat.Description.Value,
                Age: cat.Age.Value,
                Gender: cat.Gender.Value,
                Color: cat.Color.Value,
                WeightValueInKilograms: cat.Weight.ValueInKilograms,
                HealthStatus: cat.HealthStatus.Value,
                SpecialNeedsStatusHasSpecialNeeds: cat.SpecialNeeds.HasSpecialNeeds,
                SpecialNeedsStatusDescription: cat.SpecialNeeds.Description,
                SpecialNeedsStatusSeverityType: cat.SpecialNeeds.SeverityType,
                Temperament: cat.Temperament.Value,
                AdoptionHistoryReturnCount: cat.AdoptionHistory.ReturnCount,
                AdoptionHistoryLastReturnDate: cat.AdoptionHistory.LastReturnDate,
                AdoptionHistoryLastReturnReason: cat.AdoptionHistory.LastReturnReason,
                ListingSourceType: cat.ListingSource.Type,
                ListingSourceSourceName: cat.ListingSource.SourceName,
                IsNeutered: cat.NeuteringStatus.IsNeutered,
                InfectiousDiseaseStatusFivStatus: cat.InfectiousDiseaseStatus.FivStatus,
                InfectiousDiseaseStatusFelvStatus: cat.InfectiousDiseaseStatus.FelvStatus,
                InfectiousDiseaseStatusLastTestedAt: cat.InfectiousDiseaseStatus.LastTestedAt);

            return response;
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPut("cats/{catId:guid}", async (
            Guid catId,
            UpdateCatRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Command command = request.MapToCommand(new CatId(catId));

            Result<CatResponse> commandResult = await sender.Send(command, cancellationToken);

            return commandResult.IsFailure
                ? Results.Problem(commandResult.Error.ToProblemDetails())
                : Results.Ok(commandResult.Value);
        });
    }
}

internal static class UpdateCatMappings
{
    extension(UpdateCatRequest request)
    {
        public UpdateCat.Command MapToCommand(CatId catId)
        {
            UpdateCat.Command command = new(
                CatId: catId,
                Name: request.Name,
                Description: request.Description,
                Age: request.Age,
                Gender: request.Gender,
                Color: request.Color,
                WeightValueInKilograms: request.WeightValueInKilograms,
                HealthStatus: request.HealthStatus,
                HasSpecialNeeds: request.HasSpecialNeeds,
                SpecialNeedsDescription: request.SpecialNeedsDescription,
                SpecialNeedsSeverityType: request.SpecialNeedsSeverityType,
                Temperament: request.Temperament,
                AdoptionHistoryReturnCount: request.AdoptionHistoryReturnCount,
                AdoptionHistoryLastReturnDate: request.AdoptionHistoryLastReturnDate,
                AdoptionHistoryLastReturnReason: request.AdoptionHistoryLastReturnReason,
                ListingSourceType: request.ListingSourceType,
                ListingSourceSourceName: request.ListingSourceSourceName,
                IsNeutered: request.IsNeutered,
                FivStatus: request.FivStatus,
                FelvStatus: request.FelvStatus,
                InfectiousDiseaseStatusLastTestedAt: request.InfectiousDiseaseStatusLastTestedAt);
            return command;
        }
    }
}
