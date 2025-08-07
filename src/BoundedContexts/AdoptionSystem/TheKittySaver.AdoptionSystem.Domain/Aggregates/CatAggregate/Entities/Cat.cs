using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Enums;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;

public sealed class Cat : AggregateRoot<CatId>
{
    public CatName Name { get; private set; }
    public CatAge Age { get; private set; }
    public bool IsMale { get; private set; }
    public HealthStatus HealthStatus { get; private set; }
    public SpecialNeedsStatus SpecialNeeds { get; private set; }
    public Temperament Temperament { get; private set; }
    public AdoptionHistory AdoptionHistory { get; private set; }
    public ListingSource ListingSource { get; private set; }
    public CatColor Color { get; private set; }

    public static Cat Create(
        CatName name,
        CatAge age,
        bool isMale,
        HealthStatus healthStatus,
        SpecialNeedsStatus specialNeeds,
        Temperament temperament,
        AdoptionHistory adoptionHistory,
        ListingSource listingSource,
        CatColor color)
    {
        CatId id = CatId.New();
        Cat instance = new(
            id, 
            name, 
            age, 
            isMale,
            healthStatus,
            specialNeeds,
            temperament,
            adoptionHistory,
            listingSource,
            color);
        return instance;
    }
    
    private Cat(
        CatId id,
        CatName name,
        CatAge age,
        bool isMale,
        HealthStatus healthStatus,
        SpecialNeedsStatus specialNeeds,
        Temperament temperament,
        AdoptionHistory adoptionHistory,
        ListingSource listingSource,
        CatColor color) : base(id)
    {
        Name = name;
        Age = age;
        IsMale = isMale;
        HealthStatus = healthStatus;
        SpecialNeeds = specialNeeds;
        Temperament = temperament;
        AdoptionHistory = adoptionHistory;
        ListingSource = listingSource;
        Color = color;
    }
    
    /// <summary>
    /// Oblicza priorytet adopcji dla kota. Im wyższa wartość, tym większa potrzeba adopcji.
    /// </summary>
    public AdoptionPriorityScore CalculateAdoptionPriority()
    {
        decimal priority = 0;

        priority += Age.Value switch
        {
            >= 10 => 30,
            >= 7 => 25,
            >= 5 => 20,
            >= 3 => 15,
            >= 1 => 10,
            _ => 5
        };

        priority += HealthStatus switch
        {
            HealthStatus.Critical => 40,
            HealthStatus.ChronicIllness => 35,
            HealthStatus.Recovering => 25,
            HealthStatus.MinorIssues => 15,
            _ => 0
        };
        
        priority += SpecialNeeds.CalculatePriorityPoints();
        
        priority += Temperament switch
        {
            Temperament.Aggressive => 15,
            Temperament.VeryTimid => 12,
            Temperament.Timid => 8,
            Temperament.Independent => 5,
            _ => 0
        };
        
        priority += AdoptionHistory.CalculatePriorityPoints();
        
        priority += ListingSource.Type switch
        {
            ListingSourceType.PrivatePersonUrgent => 20,
            ListingSourceType.PrivatePerson => 10,
            ListingSourceType.SmallRescueGroup => 8,
            ListingSourceType.Foundation => 5,
            ListingSourceType.Shelter => 3,
            _ => 0
        };
        
        priority += Color switch
        {
            CatColor.Black => 10,
            CatColor.BlackAndWhite => 7,
            CatColor.Tortoiseshell => 5,
            CatColor.Tabby => 3,
            _ => 0
        };

        if (IsMale)
        {
            priority += 5;
        }

        AdoptionPriorityScore score = AdoptionPriorityScore.Create(priority);
        return score;
    }
}