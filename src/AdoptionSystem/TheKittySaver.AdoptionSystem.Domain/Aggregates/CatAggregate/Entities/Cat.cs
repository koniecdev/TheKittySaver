using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Events;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Extensions;
using TheKittySaver.AdoptionSystem.Domain.Core.Guards;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;

public sealed class Cat : AggregateRoot<CatId>
{
    private readonly List<Vaccination> _vaccinations;
    
    public PersonId PersonId { get; }
    public AdoptionAnnouncementId? AdoptionAnnouncementId { get; private set; }
    public CatName Name { get; private set; }
    public CatDescription Description { get; private set; }
    public CatAge Age { get; private set; }
    public CatGender Gender { get; private set; }
    public CatColor Color { get; private set; }
    public CatWeight Weight { get; private set; }
    public HealthStatus HealthStatus { get; private set; }
    public SpecialNeedsStatus SpecialNeeds { get; private set; }
    public Temperament Temperament { get; private set; }
    public AdoptionHistory AdoptionHistory { get; private set; }
    public ListingSource ListingSource { get; private set; }
    public NeuteringStatus NeuteringStatus { get; private set; }
    public InfectiousDiseaseStatus InfectiousDiseaseStatus { get; private set; }
    
    public IReadOnlyList<Vaccination> Vaccinations => _vaccinations.AsReadOnly();

    public CatStatus Status { get; private set; }

    public Result ReassignToAdoptionAnnouncement(AdoptionAnnouncementId adoptionAnnouncementId)
    {
        Ensure.NotEmpty(adoptionAnnouncementId);
        AdoptionAnnouncementId = adoptionAnnouncementId;
        return Result.Success();
    }
    
    public Result UnassignFromAdoptionAnnouncement()
    {
        AdoptionAnnouncementId = null;
        RaiseDomainEvent(new CatUnassignedFromAnnouncementDomainEvent(this));
        return Result.Success();
    }
    
    public Result UpdateName(CatName updatedName)
    {
        ArgumentNullException.ThrowIfNull(updatedName);
        Name = updatedName;
        return Result.Success();
    }

    public Result UpdateDescription(CatDescription updatedDescription)
    {
        ArgumentNullException.ThrowIfNull(updatedDescription);
        Description = updatedDescription;
        return Result.Success();
    }

    public Result UpdateAge(CatAge updatedAge)
    {
        ArgumentNullException.ThrowIfNull(updatedAge);
        Age = updatedAge;
        return Result.Success();
    }

    public Result UpdateGender(CatGender updatedGender)
    {
        ArgumentNullException.ThrowIfNull(updatedGender);
        Gender = updatedGender;
        return Result.Success();
    }

    public Result UpdateColor(CatColor updatedColor)
    {
        ArgumentNullException.ThrowIfNull(updatedColor);
        Color = updatedColor;
        return Result.Success();
    }

    public Result UpdateWeight(CatWeight updatedWeight)
    {
        ArgumentNullException.ThrowIfNull(updatedWeight);
        Weight = updatedWeight;
        return Result.Success();
    }

    public Result UpdateHealthStatus(HealthStatus updatedHealthStatus)
    {
        ArgumentNullException.ThrowIfNull(updatedHealthStatus);
        HealthStatus = updatedHealthStatus;
        return Result.Success();
    }

    public Result UpdateSpecialNeeds(SpecialNeedsStatus updatedSpecialNeeds)
    {
        ArgumentNullException.ThrowIfNull(updatedSpecialNeeds);
        SpecialNeeds = updatedSpecialNeeds;
        return Result.Success();
    }

    public Result UpdateTemperament(Temperament updatedTemperament)
    {
        ArgumentNullException.ThrowIfNull(updatedTemperament);
        Temperament = updatedTemperament;
        return Result.Success();
    }

    public Result UpdateAdoptionHistory(AdoptionHistory updatedAdoptionHistory)
    {
        ArgumentNullException.ThrowIfNull(updatedAdoptionHistory);
        AdoptionHistory = updatedAdoptionHistory;
        return Result.Success();
    }

    public Result UpdateListingSource(ListingSource updatedListingSource)
    {
        ArgumentNullException.ThrowIfNull(updatedListingSource);
        ListingSource = updatedListingSource;
        return Result.Success();
    }

    public Result UpdateNeuteringStatus(NeuteringStatus updatedNeuteringStatus)
    {
        ArgumentNullException.ThrowIfNull(updatedNeuteringStatus);
        NeuteringStatus = updatedNeuteringStatus;
        return Result.Success();
    }

    public Result UpdateInfectiousDiseaseStatus(InfectiousDiseaseStatus updatedInfectiousDiseaseStatus)
    {
        ArgumentNullException.ThrowIfNull(updatedInfectiousDiseaseStatus);
        InfectiousDiseaseStatus = updatedInfectiousDiseaseStatus;
        return Result.Success();
    }

    public Result Publish(DateTimeOffset changedAt)
    {
        if (Status.IsPublished)
        {
            return Result.Failure(DomainErrors.CatEntity.CatAlreadyPublished);
        }

        Result<CatStatus> updatedStatus = CatStatus.Published(changedAt);
        if (updatedStatus.IsFailure)
        {
            return updatedStatus;
        }

        Status = updatedStatus.Value;
        RaiseDomainEvent(new CatPublishedDomainEvent(this));
        return Result.Success();
    }

    public Result Unpublish(DateTimeOffset changedAt)
    {
        if (Status.IsDraft)
        {
            return Result.Failure(DomainErrors.CatEntity.CatAlreadyDraft);
        }

        if (Status.IsAdopted)
        {
            return Result.Failure(DomainErrors.CatEntity.CannotUnpublishAdoptedCat);
        }

        Result<CatStatus> updatedStatus = CatStatus.Draft(changedAt);
        if (updatedStatus.IsFailure)
        {
            return updatedStatus;
        }

        Status = updatedStatus.Value;
        RaiseDomainEvent(new CatUnpublishedDomainEvent(this));
        return Result.Success();
    }

    public Result Adopt(DateTimeOffset changedAt, string? note = null)
    {
        if (Status.IsAdopted)
        {
            return Result.Failure(DomainErrors.CatEntity.CatAlreadyAdopted);
        }

        Result<CatStatus> updatedStatus = CatStatus.Adopted(changedAt, note);
        if (updatedStatus.IsFailure)
        {
            return updatedStatus;
        }

        Status = updatedStatus.Value;
        RaiseDomainEvent(new CatAdoptedDomainEvent(this));
        return Result.Success();
    }

    public Result<Vaccination> AddVaccination(
        VaccinationType type,
        DateTimeOffset vaccinationDate,
        CreatedAt createdAt,
        DateTimeOffset? nextDueDate = null,
        VaccinationNote? veterinarianNote = null)
    {
        ArgumentNullException.ThrowIfNull(createdAt);

        Result<Vaccination> vaccinationResult = Vaccination.Create(
            type,
            vaccinationDate,
            createdAt,
            nextDueDate,
            veterinarianNote);

        if (!vaccinationResult.IsSuccess)
        {
            return vaccinationResult;
        }

        _vaccinations.Add(vaccinationResult.Value);
        return Result.Success(vaccinationResult.Value);
    }

    public Result RemoveVaccination(VaccinationId vaccinationId)
    {
        Ensure.NotEmpty(vaccinationId);

        Maybe<Vaccination> maybeVaccination = _vaccinations.GetByIdOrDefault(vaccinationId);
        if (maybeVaccination.HasNoValue)
        {
            return Result.Failure(DomainErrors.CatEntity.VaccinationNotFound(vaccinationId));
        }

        return !_vaccinations.Remove(maybeVaccination.Value)
            ? Result.Failure(DomainErrors.DeletionCorruption(nameof(Vaccination)))
            : Result.Success();
    }

    public Result UpdateVaccinationType(VaccinationId vaccinationId, VaccinationType updatedType)
    {
        Ensure.NotEmpty(vaccinationId);

        Maybe<Vaccination> maybeVaccination = _vaccinations.GetByIdOrDefault(vaccinationId);
        if (maybeVaccination.HasNoValue)
        {
            return Result.Failure(DomainErrors.CatEntity.VaccinationNotFound(vaccinationId));
        }

        Result updateResult = maybeVaccination.Value.UpdateType(updatedType);
        return updateResult;
    }

    public Result UpdateVaccinationDate(
        VaccinationId vaccinationId,
        DateTimeOffset updatedVaccinationDate,
        DateTimeOffset currentDate)
    {
        Ensure.NotEmpty(vaccinationId);

        Maybe<Vaccination> maybeVaccination = _vaccinations.GetByIdOrDefault(vaccinationId);
        if (maybeVaccination.HasNoValue)
        {
            return Result.Failure(DomainErrors.CatEntity.VaccinationNotFound(vaccinationId));
        }

        Result updateResult = maybeVaccination.Value.UpdateVaccinationDate(updatedVaccinationDate, currentDate);
        return updateResult;
    }

    public Result UpdateVaccinationNextDueDate(
        VaccinationId vaccinationId,
        DateTimeOffset? updatedNextDueDate,
        DateTimeOffset currentDate)
    {
        Ensure.NotEmpty(vaccinationId);

        Maybe<Vaccination> maybeVaccination = _vaccinations.GetByIdOrDefault(vaccinationId);
        if (maybeVaccination.HasNoValue)
        {
            return Result.Failure(DomainErrors.CatEntity.VaccinationNotFound(vaccinationId));
        }

        Result updateResult = maybeVaccination.Value.UpdateNextDueDate(updatedNextDueDate, currentDate);
        return updateResult;
    }

    public Result UpdateVaccinationVeterinarianNote(
        VaccinationId vaccinationId,
        VaccinationNote? updatedVeterinarianNote)
    {
        Ensure.NotEmpty(vaccinationId);

        Maybe<Vaccination> maybeVaccination = _vaccinations.GetByIdOrDefault(vaccinationId);
        if (maybeVaccination.HasNoValue)
        {
            return Result.Failure(DomainErrors.CatEntity.VaccinationNotFound(vaccinationId));
        }

        Result updateResult = maybeVaccination.Value.UpdateVeterinarianNote(updatedVeterinarianNote);
        return updateResult;
    }

    public static Result<Cat> Create(
        PersonId personId,
        CatName name,
        CatDescription description,
        CatAge age,
        CatGender gender,
        CatColor color,
        CatWeight weight,
        HealthStatus healthStatus,
        SpecialNeedsStatus specialNeeds,
        Temperament temperament,
        AdoptionHistory adoptionHistory,
        ListingSource listingSource,
        NeuteringStatus neuteringStatus,
        InfectiousDiseaseStatus infectiousDiseaseStatus,
        CreatedAt createdAt,
        List<Vaccination>? vaccinations = null)
    {
        Ensure.NotEmpty(personId);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(description);
        ArgumentNullException.ThrowIfNull(age);
        ArgumentNullException.ThrowIfNull(gender);
        ArgumentNullException.ThrowIfNull(color);
        ArgumentNullException.ThrowIfNull(weight);
        ArgumentNullException.ThrowIfNull(healthStatus);
        ArgumentNullException.ThrowIfNull(specialNeeds);
        ArgumentNullException.ThrowIfNull(temperament);
        ArgumentNullException.ThrowIfNull(adoptionHistory);
        ArgumentNullException.ThrowIfNull(listingSource);
        ArgumentNullException.ThrowIfNull(neuteringStatus);
        ArgumentNullException.ThrowIfNull(infectiousDiseaseStatus);
        ArgumentNullException.ThrowIfNull(createdAt);

        Result<CatStatus> statusResult = CatStatus.Draft(createdAt.Value);
        if (!statusResult.IsSuccess)
        {
            return Result.Failure<Cat>(statusResult.Error);
        }
        
        CatId id = CatId.New();
        Cat instance = new(
            id,
            personId,
            name,
            description,
            age,
            gender,
            color,
            weight,
            healthStatus,
            specialNeeds,
            temperament,
            adoptionHistory,
            listingSource,
            neuteringStatus,
            infectiousDiseaseStatus,
            createdAt,
            statusResult.Value,
            vaccinations ?? []);

        return Result.Success(instance);
    }

    private Cat(
        CatId id,
        PersonId personId,
        CatName name,
        CatDescription description,
        CatAge age,
        CatGender gender,
        CatColor color,
        CatWeight weight,
        HealthStatus healthStatus,
        SpecialNeedsStatus specialNeeds,
        Temperament temperament,
        AdoptionHistory adoptionHistory,
        ListingSource listingSource,
        NeuteringStatus neuteringStatus,
        InfectiousDiseaseStatus infectiousDiseaseStatus,
        CreatedAt createdAt,
        CatStatus status,
        List<Vaccination> vaccinations) : base(id, createdAt)
    {
        PersonId = personId;
        Name = name;
        Description = description;
        Age = age;
        Gender = gender;
        Color = color;
        Weight = weight;
        HealthStatus = healthStatus;
        SpecialNeeds = specialNeeds;
        Temperament = temperament;
        AdoptionHistory = adoptionHistory;
        ListingSource = listingSource;
        NeuteringStatus = neuteringStatus;
        InfectiousDiseaseStatus = infectiousDiseaseStatus;
        _vaccinations = vaccinations;
        Status = status;
    }
}