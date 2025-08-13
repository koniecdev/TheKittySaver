using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;

public sealed class Cat : AggregateRoot<CatId>
{
    public CatName Name { get; private set; }
    public CatAge Age { get; private set; }
    public CatGender Gender { get; private set; }
    public HealthStatus HealthStatus { get; private set; }
    public SpecialNeedsStatus SpecialNeeds { get; private set; }
    public Temperament Temperament { get; private set; }
    public AdoptionHistory AdoptionHistory { get; private set; }
    public ListingSource ListingSource { get; private set; }
    public CatColor Color { get; private set; }

    public Result<AdoptionPriorityScore> CalculateAdoptionPriority()
    {
        Func<Result<AdoptionPriorityScore>>[] calculators =
        [
            () => Age.CalculatePriorityScore(),
            () => HealthStatus.CalculatePriorityScore(),
            () => SpecialNeeds.CalculatePriorityPoints(),
            () => Temperament.CalculatePriorityScore(),
            () => AdoptionHistory.CalculatePriorityPoints(),
            () => ListingSource.CalculatePriorityScore(),
            () => Color.CalculatePriorityScore(),
            () => Gender.CalculatePriorityScore()
        ];

        decimal priority = 0;
        foreach (Func<Result<AdoptionPriorityScore>> calculator in calculators)
        {
            Result<AdoptionPriorityScore> result = calculator();
            if (result.IsFailure)
            {
                return result;
            }
        
            priority += result.Value.Value;
        }

        return AdoptionPriorityScore.Create(priority);
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