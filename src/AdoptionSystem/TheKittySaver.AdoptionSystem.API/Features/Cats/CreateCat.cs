using Mediator;
using TheKittySaver.AdoptionSystem.API.Common;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Requests;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
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
        DateOnly InfectiousDiseaseStatusLastTestedAt) : ICommand<Result<CatId>>;

    internal sealed class Handler : ICommandHandler<Command, Result<CatId>>
    {
        private readonly ICatRepository _catRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public Handler(
            ICatRepository catRepository,
            IPersonRepository personRepository,
            IUnitOfWork unitOfWork,
            TimeProvider timeProvider)
        {
            _catRepository = catRepository;
            _personRepository = personRepository;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public async ValueTask<Result<CatId>> Handle(Command command, CancellationToken cancellationToken)
        {
            if (!await _personRepository.ExistsAsync(command.PersonId, cancellationToken))
            {
                return Result.Failure<CatId>(DomainErrors.PersonEntity.NotFound(command.PersonId));
            }
            
            Result<CatName> createNameResult = CatName.Create(command.Name);
            if (createNameResult.IsFailure)
            {
                return Result.Failure<CatId>(createNameResult.Error);
            }

            Result<CatDescription> createDescriptionResult = CatDescription.Create(command.Description);
            if (createDescriptionResult.IsFailure)
            {
                return Result.Failure<CatId>(createDescriptionResult.Error);
            }

            Result<CatAge> createAgeResult = CatAge.Create(command.Age);
            if (createAgeResult.IsFailure)
            {
                return Result.Failure<CatId>(createAgeResult.Error);
            }

            CatGender gender = command.Gender is CatGenderType.Male
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
                return Result.Failure<CatId>(createWeightResult.Error);
            }

            HealthStatus healthStatus = command.HealthStatus switch
            {
                HealthStatusType.Healthy => HealthStatus.Healthy(),
                HealthStatusType.MinorIssues => HealthStatus.MinorIssues(),
                HealthStatusType.Recovering => HealthStatus.Recovering(),
                HealthStatusType.ChronicIllness => HealthStatus.ChronicIllness(),
                _ => HealthStatus.Critical()
            };

            SpecialNeedsStatus specialNeeds;
            if (command.SpecialNeedsStatusHasSpecialNeeds)
            {
                Result<SpecialNeedsStatus> createSpecialNeedsResult = SpecialNeedsStatus.Create(
                    command.SpecialNeedsStatusDescription ?? string.Empty,
                    command.SpecialNeedsStatusSeverityType);
                if (createSpecialNeedsResult.IsFailure)
                {
                    return Result.Failure<CatId>(createSpecialNeedsResult.Error);
                }
                specialNeeds = createSpecialNeedsResult.Value;
            }
            else
            {
                specialNeeds = SpecialNeedsStatus.None();
            }

            Temperament temperament = command.Temperament switch
            {
                TemperamentType.Friendly => Temperament.Friendly(),
                TemperamentType.Independent => Temperament.Independent(),
                TemperamentType.Timid => Temperament.Timid(),
                TemperamentType.VeryTimid => Temperament.VeryTimid(),
                _ => Temperament.Aggressive()
            };

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
                    return Result.Failure<CatId>(createAdoptionHistoryResult.Error);
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
                return Result.Failure<CatId>(createListingSourceResult.Error);
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
                return Result.Failure<CatId>(createInfectiousDiseaseStatusResult.Error);
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
                return Result.Failure<CatId>(createCatResult.Error);
            }

            Cat cat = createCatResult.Value;

            _catRepository.Insert(cat);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return cat.Id;
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

            Result<CatId> commandResult = await sender.Send(command, cancellationToken);

            return commandResult.IsFailure
                ? Results.Problem(commandResult.Error.ToProblemDetails())
                : Results.Created($"/api/v1/cats/{commandResult.Value}", commandResult.Value);
        });
    }
}

internal static class CreateCatMappings
{
    extension(CreateCatRequest request)
    {
        public CreateCat.Command MapToCommand()
        {
            ArgumentNullException.ThrowIfNull(request);
            
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
