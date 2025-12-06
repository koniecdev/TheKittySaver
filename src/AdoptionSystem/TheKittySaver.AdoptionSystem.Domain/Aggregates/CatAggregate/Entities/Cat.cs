using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Events;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Abstractions;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Extensions;
using TheKittySaver.AdoptionSystem.Domain.Core.Guards;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;

public sealed class Cat : AggregateRoot<CatId>, IClaimable, IPublishable
{
    public const int MaximumGalleryItemsCount = 20;

    private readonly List<Vaccination> _vaccinations = [];
    private readonly List<CatGalleryItem> _galleryItems = [];

    public PersonId PersonId { get; }
    public AdoptionAnnouncementId? AdoptionAnnouncementId { get; private set; }
    
    public ClaimedAt? ClaimedAt { get; private set; }
    public PublishedAt? PublishedAt { get; private set; }
    
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
    
    public CatThumbnail? Thumbnail { get; private set; }
    
    public IReadOnlyList<CatGalleryItem> GalleryItems => _galleryItems.AsReadOnly();
    public IReadOnlyList<Vaccination> Vaccinations => _vaccinations.AsReadOnly();

    public CatStatusType Status { get; private set; } = CatStatusType.Draft;
    
    public Result AssignToAdoptionAnnouncement(
        AdoptionAnnouncementId adoptionAnnouncementId,
        DateTimeOffset dateTimeOfOperation)
    {
        Ensure.NotEmpty(adoptionAnnouncementId);

        if (Status is not CatStatusType.Draft)
        {
            return Result.Failure(DomainErrors.CatEntity.StatusProperty.MustBeDraftForAssignment(Id));
        }

        if (AdoptionAnnouncementId is not null)
        {
            return Result.Failure(DomainErrors.CatEntity.Assignment.AlreadyAssignedToAnotherAnnouncement(Id));
        }

        if (Thumbnail is null)
        {
            return Result.Failure(DomainErrors.CatEntity.ThumbnailProperty.RequiredForPublishing(Id));
        }

        Result<PublishedAt> publishedAtResult = PublishedAt.Create(dateTimeOfOperation);
        if (publishedAtResult.IsFailure)
        {
            return publishedAtResult;
        }

        Status = CatStatusType.Published;
        PublishedAt = publishedAtResult.Value;

        AdoptionAnnouncementId = adoptionAnnouncementId;

        return Result.Success();
    }

    public Result ReassignToAnotherAdoptionAnnouncement(
        AdoptionAnnouncementId destinationAdoptionAnnouncementId,
        DateTimeOffset dateTimeOfOperation)
    {
        Ensure.NotEmpty(destinationAdoptionAnnouncementId);

        if (Status is not CatStatusType.Published || AdoptionAnnouncementId is null)
        {
            return Result.Failure(DomainErrors.CatEntity.StatusProperty.MustBePublishedForReassignment(Id));
        }

        AdoptionAnnouncementId sourceAdoptionAnnouncementId = AdoptionAnnouncementId.Value;

        Result<PublishedAt> publishedAtResult = PublishedAt.Create(dateTimeOfOperation);
        if (publishedAtResult.IsFailure)
        {
            return publishedAtResult;
        }

        PublishedAt = publishedAtResult.Value;

        AdoptionAnnouncementId = destinationAdoptionAnnouncementId;

        RaiseDomainEvent(new CatReassignedToAnotherAnnouncementDomainEvent(
            Id,
            sourceAdoptionAnnouncementId,
            destinationAdoptionAnnouncementId));

        return Result.Success();
    }

    public Result UnassignFromAdoptionAnnouncement()
    {
        if (Status is not CatStatusType.Published)
        {
            return Result.Failure(DomainErrors.CatEntity.StatusProperty.NotPublished(Id));
        }

        if (AdoptionAnnouncementId is null)
        {
            return Result.Failure(DomainErrors.CatEntity.Assignment.NotAssignedToAdoptionAnnouncement(Id));
        }

        AdoptionAnnouncementId sourceAdoptionAnnouncementId = AdoptionAnnouncementId.Value;

        AdoptionAnnouncementId = null;
        Status = CatStatusType.Draft;
        PublishedAt = null;

        RaiseDomainEvent(new CatUnassignedFromAnnouncementDomainEvent(
            Id,
            sourceAdoptionAnnouncementId));

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

    public Result Claim(ClaimedAt claimedAt)
    {
        ArgumentNullException.ThrowIfNull(claimedAt);

        switch (Status)
        {
            case CatStatusType.Draft:
                return Result.Failure(DomainErrors.CatEntity.StatusProperty.CannotClaimDraftCat(Id));
            case CatStatusType.Claimed:
                return Result.Failure(DomainErrors.CatEntity.StatusProperty.AlreadyClaimed(Id));
            default:
                if (AdoptionAnnouncementId is null)
                {
                    return Result.Failure(DomainErrors.CatEntity.Assignment.NotAssignedToAdoptionAnnouncement(Id));
                }

                Status = CatStatusType.Claimed;
                ClaimedAt = claimedAt;
                RaiseDomainEvent(new CatClaimedDomainEvent(Id, AdoptionAnnouncementId.Value));
                return Result.Success();
        }
    }

    public Result<Vaccination> AddVaccination(
        VaccinationType type,
        DateOnly vaccinationDate,
        DateTimeOffset dateOfOperation,
        VaccinationNote? veterinarianNote = null)
    {

        Result<Vaccination> vaccinationResult = Vaccination.Create(
            Id,
            type,
            vaccinationDate,
            dateOfOperation,
            veterinarianNote);

        if (!vaccinationResult.IsSuccess)
        {
            return vaccinationResult;
        }

        _vaccinations.Add(vaccinationResult.Value);
        return Result.Success(vaccinationResult.Value);
    }

    public Result UpdateVaccinationType(VaccinationId vaccinationId, VaccinationType updatedType)
    {
        Ensure.NotEmpty(vaccinationId);

        Maybe<Vaccination> maybeVaccination = _vaccinations.GetByIdOrDefault(vaccinationId);
        if (maybeVaccination.HasNoValue)
        {
            return Result.Failure(DomainErrors.VaccinationEntity.NotFound(vaccinationId));
        }

        Result updateResult = maybeVaccination.Value.UpdateType(updatedType);
        return updateResult;
    }

    public Result UpdateVaccinationDate(VaccinationId vaccinationId, VaccinationDate updatedDate)
    {
        Maybe<Vaccination> maybeVaccination = _vaccinations.GetByIdOrDefault(vaccinationId);
        if (maybeVaccination.HasNoValue)
        {
            return Result.Failure(DomainErrors.VaccinationEntity.NotFound(vaccinationId));
        }

        Result updateResult = maybeVaccination.Value.UpdateDate(updatedDate);
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
            return Result.Failure(DomainErrors.VaccinationEntity.NotFound(vaccinationId));
        }

        Result updateResult = maybeVaccination.Value.UpdateVeterinarianNote(updatedVeterinarianNote);
        return updateResult;
    }
    
    public Result RemoveVaccination(VaccinationId vaccinationId)
    {
        Ensure.NotEmpty(vaccinationId);

        Maybe<Vaccination> maybeVaccination = _vaccinations.GetByIdOrDefault(vaccinationId);
        if (maybeVaccination.HasNoValue)
        {
            return Result.Failure(DomainErrors.VaccinationEntity.NotFound(vaccinationId));
        }

        return !_vaccinations.Remove(maybeVaccination.Value)
            ? Result.Failure(DomainErrors.DeletionCorruption(nameof(Vaccination)))
            : Result.Success();
    }
    
    public Result<CatThumbnailId> UpsertThumbnail()
    {
        if (Status is not (CatStatusType.Draft or CatStatusType.Published))
        {
            return Result.Failure<CatThumbnailId>(
                DomainErrors.CatEntity.ThumbnailProperty.InvalidStatusForUpsertThumbnailOperation(Id));
        }

        Result<CatThumbnail> newThumbnail = CatThumbnail.Create(Id);
        if (!newThumbnail.IsSuccess)
        {
            return Result.Failure<CatThumbnailId>(newThumbnail.Error);
        }

        Thumbnail = newThumbnail.Value;
        return Result.Success(newThumbnail.Value.Id);
    }
    
    public Result<CatGalleryItemId> AddGalleryItem()
    {
        if (_galleryItems.Count >= MaximumGalleryItemsCount)
        {
            return Result.Failure<CatGalleryItemId>(DomainErrors.CatEntity.GalleryIsFull);
        }

        int nextDisplayOrder = _galleryItems.Count;
        Result<CatGalleryItemDisplayOrder> displayOrderResult =
            CatGalleryItemDisplayOrder.Create(nextDisplayOrder, MaximumGalleryItemsCount);

        if (displayOrderResult.IsFailure)
        {
            return Result.Failure<CatGalleryItemId>(displayOrderResult.Error);
        }

        Result<CatGalleryItem> galleryItemCreationResult =
            CatGalleryItem.Create(Id, displayOrderResult.Value);

        if (galleryItemCreationResult.IsFailure)
        {
            return Result.Failure<CatGalleryItemId>(galleryItemCreationResult.Error);
        }

        _galleryItems.Add(galleryItemCreationResult.Value);
        return Result.Success(galleryItemCreationResult.Value.Id);
    }

    public Result ReorderGalleryItems(Dictionary<CatGalleryItemId, CatGalleryItemDisplayOrder> newOrders)
    {
        ArgumentNullException.ThrowIfNull(newOrders);

        if (newOrders.Count != _galleryItems.Count)
        {
            return Result.Failure(DomainErrors.CatEntity.InvalidReorderOperation);
        }

        HashSet<int> displayOrderValues = newOrders.Values.Select(o => o.Value).ToHashSet();
        if (displayOrderValues.Count != newOrders.Count)
        {
            return Result.Failure(DomainErrors.CatEntity.DuplicateDisplayOrders);
        }

        int minOrder = displayOrderValues.Min();
        int maxOrder = displayOrderValues.Max();
        if (minOrder != 0 || maxOrder != _galleryItems.Count - 1)
        {
            return Result.Failure(DomainErrors.CatEntity.DisplayOrderMustBeContiguous);
        }

        foreach (KeyValuePair<CatGalleryItemId, CatGalleryItemDisplayOrder> kvp in newOrders)
        {
            Maybe<CatGalleryItem> maybeItem = _galleryItems.GetByIdOrDefault(kvp.Key);
            if (maybeItem.HasNoValue)
            {
                return Result.Failure(DomainErrors.CatGalleryItemEntity.NotFound(kvp.Key));
            }

            Result updateDisplayOrderResult = maybeItem.Value.UpdateDisplayOrder(kvp.Value);
            if (updateDisplayOrderResult.IsFailure)
            {
                return updateDisplayOrderResult;
            }
        }

        return Result.Success();
    }

    public Result RemoveGalleryItem(CatGalleryItemId galleryItemId)
    {
        Ensure.NotEmpty(galleryItemId);

        Maybe<CatGalleryItem> maybeGalleryItem = _galleryItems.GetByIdOrDefault(galleryItemId);
        if (maybeGalleryItem.HasNoValue)
        {
            return Result.Failure(DomainErrors.CatGalleryItemEntity.NotFound(galleryItemId));
        }

        int removedDisplayOrder = maybeGalleryItem.Value.DisplayOrder.Value;
        _galleryItems.Remove(maybeGalleryItem.Value);

        foreach (CatGalleryItem item in _galleryItems.Where(i => i.DisplayOrder.Value > removedDisplayOrder))
        {
            Result<CatGalleryItemDisplayOrder> newOrderResult =
                CatGalleryItemDisplayOrder.Create(item.DisplayOrder.Value - 1, MaximumGalleryItemsCount);

            if (newOrderResult.IsFailure)
            {
                return Result.Failure(newOrderResult.Error);
            }

            Result updateDisplayOrderResult = item.UpdateDisplayOrder(newOrderResult.Value);
            if (updateDisplayOrderResult.IsFailure)
            {
                return updateDisplayOrderResult;
            }
        }

        return Result.Success();
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
        InfectiousDiseaseStatus infectiousDiseaseStatus)
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
            infectiousDiseaseStatus);

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
        InfectiousDiseaseStatus infectiousDiseaseStatus) : base(id)
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
    }
}
