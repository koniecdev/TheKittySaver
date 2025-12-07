using Mediator;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Responses;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Features.Cats;

internal sealed class CreateCat : IEndpoint
{
    internal sealed record Command(
        PersonId PersonId,
        string Name,
        string Description,
        int Age,
        CatGenderType Gender,
        ColorType Color,
        decimal WeightValueInKilograms,
        HealthStatusType HealthStatus,
        bool SpecialNeedsStatusHasSpecialNeeds,
        string? SpecialNeedsStatusDescription,
        SpecialNeedsSeverityType SpecialNeedsStatusSeverityType,
        TemperamentType Temperament,
        int AdoptionHistoryReturnCount,
        DateTimeOffset? AdoptionHistoryLastReturnDate,
        string? AdoptionHistoryLastReturnReason,
        ListingSourceType ListingSourceType,
        string ListingSourceSourceName,
        bool IsNeutered,
        FivStatus InfectiousDiseaseStatusFivStatus,
        FelvStatus InfectiousDiseaseStatusFelvStatus,
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
            Result<CatName> createNameResult = CatName.Create(command.Name);
            if (createNameResult.IsFailure)
            {
                return Result.Failure<CatResponse>(createNameResult.Error);
            }

            Result<CatDescription> createDescriptionResult = CatDescription.Create(command.Description);
            if (createDescriptionResult.IsFailure)
            {
                return Result.Failure<CatResponse>(createDescriptionResult.Error);
            }

            Result<CatAge> createAgeResult = CatAge.Create(command.Age);
            if (createAgeResult.IsFailure)
            {
                return Result.Failure<CatResponse>(createAgeResult.Error);
            }

            CatGender gender = command.Gender == CatGenderType.Male
                ? CatGender.Male()
                : CatGender.Female();

            CatColor color = new Func<CatColor>(() => command.Color switch
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
            })();

            Result<CatWeight> createWeightResult = CatWeight.Create(command.WeightValueInKilograms);
            if (createWeightResult.IsFailure)
            {
                return Result.Failure<CatResponse>(createWeightResult.Error);
            }

            HealthStatus healthStatus = command.HealthStatus switch
            {
                HealthStatusType.Healthy => Domain.Aggregates.CatAggregate.ValueObjects.HealthStatus.Healthy(),
                HealthStatusType.MinorIssues => Domain.Aggregates.CatAggregate.ValueObjects.HealthStatus.MinorIssues(),
                HealthStatusType.Recovering => Domain.Aggregates.CatAggregate.ValueObjects.HealthStatus.Recovering(),
                HealthStatusType.ChronicIllness => Domain.Aggregates.CatAggregate.ValueObjects.HealthStatus.ChronicIllness(),
                _ => Domain.Aggregates.CatAggregate.ValueObjects.HealthStatus.Critical()
            };

            SpecialNeedsStatus specialNeeds;
            if (command.SpecialNeedsStatusHasSpecialNeeds)
            {
                Result<SpecialNeedsStatus> createSpecialNeedsResult = SpecialNeedsStatus.Create(
                    command.SpecialNeedsStatusDescription ?? string.Empty,
                    command.SpecialNeedsStatusSeverityType);
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

            Temperament temperament = command.Temperament switch
            {
                TemperamentType.Friendly => Domain.Aggregates.CatAggregate.ValueObjects.Temperament.Friendly(),
                TemperamentType.Independent => Domain.Aggregates.CatAggregate.ValueObjects.Temperament.Independent(),
                TemperamentType.Timid => Domain.Aggregates.CatAggregate.ValueObjects.Temperament.Timid(),
                TemperamentType.VeryTimid => Domain.Aggregates.CatAggregate.ValueObjects.Temperament.VeryTimid(),
                _ => Domain.Aggregates.CatAggregate.ValueObjects.Temperament.Aggressive()
            };

            AdoptionHistory adoptionHistory;
            if (command.AdoptionHistoryReturnCount > 0 &&
                command.AdoptionHistoryLastReturnDate.HasValue &&
                !string.IsNullOrWhiteSpace(command.AdoptionHistoryLastReturnReason))
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

            NeuteringStatus neuteringStatus = command.IsNeutered
                ? NeuteringStatus.Neutered()
                : NeuteringStatus.NotNeutered();

            DateOnly currentDate = DateOnly.FromDateTime(_timeProvider.GetUtcNow().DateTime);
            Result<InfectiousDiseaseStatus> createInfectiousDiseaseStatusResult = InfectiousDiseaseStatus.Create(
                command.InfectiousDiseaseStatusFivStatus,
                command.InfectiousDiseaseStatusFelvStatus,
                command.InfectiousDiseaseStatusLastTestedAt,
                currentDate);
            if (createInfectiousDiseaseStatusResult.IsFailure)
            {
                return Result.Failure<CatResponse>(createInfectiousDiseaseStatusResult.Error);
            }

            Result<Cat> createCatResult = Cat.Create(
                command.PersonId,
                createNameResult.Value,
                createDescriptionResult.Value,
                createAgeResult.Value,
                gender,
                color,
                createWeightResult.Value,
                healthStatus,
                specialNeeds,
                temperament,
                adoptionHistory,
                createListingSourceResult.Value,
                neuteringStatus,
                createInfectiousDiseaseStatusResult.Value);

            if (createCatResult.IsFailure)
            {
                return Result.Failure<CatResponse>(createCatResult.Error);
            }

            Cat cat = createCatResult.Value;

            _catRepository.Insert(cat);
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
        endpointRouteBuilder.MapPost("cats", async (
            CreateCatRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Command command = request.MapToCommand();

            Result<CatResponse> commandResult = await sender.Send(command, cancellationToken);

            return commandResult.IsFailure
                ? Results.Problem(commandResult.Error.ToProblemDetails())
                : Results.Created($"/api/v1/cats/{commandResult.Value.Id}", commandResult.Value);
        });
    }
}

internal static class CreateCatMappings
{
    extension(CreateCatRequest request)
    {
        public CreateCat.Command MapToCommand()
        {
            CreateCat.Command command = new(
                PersonId: request.PersonId,
                Name: request.Name,
                Description: request.Description,
                Age: request.Age,
                Gender: request.Gender,
                Color: request.Color,
                WeightValueInKilograms: request.WeightValueInKilograms,
                HealthStatus: request.HealthStatus,
                SpecialNeedsStatusHasSpecialNeeds: request.SpecialNeedsStatusHasSpecialNeeds,
                SpecialNeedsStatusDescription: request.SpecialNeedsStatusDescription,
                SpecialNeedsStatusSeverityType: request.SpecialNeedsStatusSeverityType,
                Temperament: request.Temperament,
                AdoptionHistoryReturnCount: request.AdoptionHistoryReturnCount,
                AdoptionHistoryLastReturnDate: request.AdoptionHistoryLastReturnDate,
                AdoptionHistoryLastReturnReason: request.AdoptionHistoryLastReturnReason,
                ListingSourceType: request.ListingSourceType,
                ListingSourceSourceName: request.ListingSourceSourceName,
                IsNeutered: request.IsNeutered,
                InfectiousDiseaseStatusFivStatus: request.InfectiousDiseaseStatusFivStatus,
                InfectiousDiseaseStatusFelvStatus: request.InfectiousDiseaseStatusFelvStatus,
                InfectiousDiseaseStatusLastTestedAt: request.InfectiousDiseaseStatusLastTestedAt);
            return command;
        }
    }
}
