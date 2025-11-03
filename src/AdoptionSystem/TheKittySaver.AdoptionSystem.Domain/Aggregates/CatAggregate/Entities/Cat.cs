using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Guards;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;

public sealed class Cat : AggregateRoot<CatId>
{
    public AdoptionAnnouncementId? AdoptionAnnouncementId { get; set; }
    public CatName Name { get; private set; }
    public CatAge Age { get; private set; }
    public CatGender Gender { get; private set; }
    public HealthStatus HealthStatus { get; private set; }
    public SpecialNeedsStatus SpecialNeeds { get; private set; }
    public Temperament Temperament { get; private set; }
    public AdoptionHistory AdoptionHistory { get; private set; }
    public ListingSource ListingSource { get; private set; }
    public CatColor Color { get; private set; }

    public Result ReassignToAdoptionAnnouncement(AdoptionAnnouncementId adoptionAnnouncementId)
    {
        Ensure.NotEmpty(adoptionAnnouncementId);
        AdoptionAnnouncementId = adoptionAnnouncementId;
        return Result.Success();
    }
    
    public Result UnassignFromAdoptionAnnouncement()
    {
        AdoptionAnnouncementId = null;
        return Result.Success();
    }
    
    public Result UpdateName(CatName updatedName)
    {
        ArgumentNullException.ThrowIfNull(updatedName);
        Name = updatedName;
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

    public Result UpdateColor(CatColor updatedColor)
    {
        ArgumentNullException.ThrowIfNull(updatedColor);
        Color = updatedColor;
        return Result.Success();
    }
    
    public static Result<Cat> Create(
        CatName name,
        CatAge age,
        CatGender gender,
        HealthStatus healthStatus,
        SpecialNeedsStatus specialNeeds,
        Temperament temperament,
        AdoptionHistory adoptionHistory,
        ListingSource listingSource,
        CatColor color)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(age);
        ArgumentNullException.ThrowIfNull(gender);
        ArgumentNullException.ThrowIfNull(healthStatus);
        ArgumentNullException.ThrowIfNull(specialNeeds);
        ArgumentNullException.ThrowIfNull(temperament);
        ArgumentNullException.ThrowIfNull(adoptionHistory);
        ArgumentNullException.ThrowIfNull(listingSource);
        ArgumentNullException.ThrowIfNull(color);
        
        CatId id = CatId.New();
        Cat instance = new(
            id, 
            name, 
            age, 
            gender,
            healthStatus,
            specialNeeds,
            temperament,
            adoptionHistory,
            listingSource,
            color);
        
        return Result.Success(instance);
    }
    
    private Cat(
        CatId id,
        CatName name,
        CatAge age,
        CatGender gender,
        HealthStatus healthStatus,
        SpecialNeedsStatus specialNeeds,
        Temperament temperament,
        AdoptionHistory adoptionHistory,
        ListingSource listingSource,
        CatColor color) : base(id)
    {
        Name = name;
        Age = age;
        Gender = gender;
        HealthStatus = healthStatus;
        SpecialNeeds = specialNeeds;
        Temperament = temperament;
        AdoptionHistory = adoptionHistory;
        ListingSource = listingSource;
        Color = color;
    }
}