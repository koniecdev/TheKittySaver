using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Services;

public sealed class DefaultAdoptionPriorityScoreCalculator : IAdoptionPriorityScoreCalculator
{
    public Result<AdoptionPriorityScore> Calculate(Cat cat)
    {
        decimal adoptionHistoryPoints = CalculateAdoptionHistoryPoints(cat.AdoptionHistory);
        decimal agePoints = CalculateAgePoints(cat.Age);
        decimal colorPoints = CalculateColorPoints(cat.Color);
        decimal genderPoints = CalculateGenderPoints(cat.Gender);
        decimal healthPoints = CalculateHealthPoints(cat.HealthStatus);
        decimal listingSourcePoints = CalculateListingSourcePoints(cat.ListingSource);
        decimal specialNeedsPoints = CalculateSpecialNeedsPoints(cat.SpecialNeeds);
        decimal temperamentPoints = CalculateTemperamentPoints(cat.Temperament);
        
        decimal totalPoints = 
            adoptionHistoryPoints +
            agePoints +
            colorPoints +
            genderPoints +
            healthPoints +
            listingSourcePoints +
            specialNeedsPoints +
            temperamentPoints;

        Result<AdoptionPriorityScore> result = AdoptionPriorityScore.Create(totalPoints);
        return result;
    }
    
    private static decimal CalculateAdoptionHistoryPoints(AdoptionHistory adoptionHistory)
    {
        if (adoptionHistory.ReturnCount == 0)
        {
            return 0m;
        }
        
        int basePoints = adoptionHistory.ReturnCount * 10;
        int resultPoints = Math.Min(basePoints, 25);
        
        return resultPoints;
    }
    
    private static decimal CalculateAgePoints(CatAge age)
    {
        decimal points = age.Value switch
        {
            >= 10 => 30,
            >= 7 => 25,
            >= 5 => 20,
            >= 3 => 15,
            >= 1 => 10,
            _ => 5
        };
        
        return points;
    }
    
    private static decimal CalculateColorPoints(CatColor color)
    {
        decimal points = color.Value switch
        {
            ColorType.Black => 10,
            ColorType.BlackAndWhite => 7,
            ColorType.Tortoiseshell => 5,
            ColorType.Tabby => 3,
            _ => 0
        };
        
        return points;
    }
    
    private static decimal CalculateGenderPoints(CatGender gender)
    {
        decimal points = gender.Value switch
        {
            CatGenderType.Male => 5,
            _ => 0
        };
        
        return points;
    }
    
    private static decimal CalculateHealthPoints(HealthStatus healthStatus)
    {
        decimal points = healthStatus.Value switch
        {
            HealthStatusType.Critical => 40,
            HealthStatusType.ChronicIllness => 35,
            HealthStatusType.Recovering => 25,
            HealthStatusType.MinorIssues => 15,
            _ => 0
        };
        
        return points;
    }
    
    private static decimal CalculateListingSourcePoints(ListingSource listingSource)
    {
        decimal points = listingSource.Type switch
        {
            ListingSourceType.PrivatePersonUrgent => 20,
            ListingSourceType.PrivatePerson => 18,
            ListingSourceType.Foundation => 10,
            ListingSourceType.Shelter => 5,
            _ => 0
        };
        
        return points;
    }
    
    private static decimal CalculateSpecialNeedsPoints(SpecialNeedsStatus specialNeeds)
    {
        int points = specialNeeds.SeverityType switch
        {
            SpecialNeedsSeverityType.Severe => 25,
            SpecialNeedsSeverityType.Moderate => 15,
            SpecialNeedsSeverityType.Minor => 8,
            _ => 0
        };
        
        return points;
    }
    
    private static decimal CalculateTemperamentPoints(Temperament temperament)
    {
        decimal points = temperament.Value switch
        {
            TemperamentType.Aggressive => 15,
            TemperamentType.VeryTimid => 12,
            TemperamentType.Timid => 8,
            TemperamentType.Independent => 5,
            _ => 0
        };
        
        return points;
    }
}