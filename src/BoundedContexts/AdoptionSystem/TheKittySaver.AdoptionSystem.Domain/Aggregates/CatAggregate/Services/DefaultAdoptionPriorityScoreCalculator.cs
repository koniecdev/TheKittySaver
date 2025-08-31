using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Services;

public sealed class DefaultAdoptionPriorityScoreCalculator : IAdoptionPriorityScoreCalculator
{
    public Result<AdoptionPriorityScore> Calculate(Cat cat)
    {
        Result<decimal> adoptionHistoryPointsResult = CalculateAdoptionHistoryPoints(cat.AdoptionHistory);
        if (adoptionHistoryPointsResult.IsFailure)
        {
            return Result.Failure<AdoptionPriorityScore>(adoptionHistoryPointsResult.Error);
        }
        
        Result<decimal> agePointsResult = CalculateAgePoints(cat.Age);
        if (agePointsResult.IsFailure)
        {
            return Result.Failure<AdoptionPriorityScore>(agePointsResult.Error);
        }
        
        Result<decimal> colorPointsResult = CalculateColorPoints(cat.Color);
        if (colorPointsResult.IsFailure)
        {
            return Result.Failure<AdoptionPriorityScore>(colorPointsResult.Error);
        }
        
        Result<decimal> genderPointsResult = CalculateGenderPoints(cat.Gender);
        if (genderPointsResult.IsFailure)
        {
            return Result.Failure<AdoptionPriorityScore>(genderPointsResult.Error);
        }
        
        Result<decimal> healthPointsResult = CalculateHealthPoints(cat.HealthStatus);
        if (healthPointsResult.IsFailure)
        {
            return Result.Failure<AdoptionPriorityScore>(healthPointsResult.Error);
        }
        
        Result<decimal> listingSourcePointsResult = CalculateListingSourcePoints(cat.ListingSource);
        if (listingSourcePointsResult.IsFailure)
        {
            return Result.Failure<AdoptionPriorityScore>(listingSourcePointsResult.Error);
        }
        
        Result<decimal> specialNeedsPointsResult = CalculateSpecialNeedsPoints(cat.SpecialNeeds);
        if (specialNeedsPointsResult.IsFailure)
        {
            return Result.Failure<AdoptionPriorityScore>(specialNeedsPointsResult.Error);
        }
        
        Result<decimal> temperamentPointsResult = CalculateTemperamentPoints(cat.Temperament);
        if (temperamentPointsResult.IsFailure)
        {
            return Result.Failure<AdoptionPriorityScore>(temperamentPointsResult.Error);
        }
        
        decimal totalPoints = 
            adoptionHistoryPointsResult.Value +
            agePointsResult.Value +
            colorPointsResult.Value +
            genderPointsResult.Value +
            healthPointsResult.Value +
            listingSourcePointsResult.Value +
            specialNeedsPointsResult.Value +
            temperamentPointsResult.Value;
        
        return AdoptionPriorityScore.Create(totalPoints);
    }
    
    private static Result<decimal> CalculateAdoptionHistoryPoints(AdoptionHistory adoptionHistory)
    {
        if (adoptionHistory.ReturnCount == 0)
        {
            return Result.Success(0m);
        }
        
        int basePoints = adoptionHistory.ReturnCount * 10;
        int resultPoints = Math.Min(basePoints, 25);
        
        return Result.Success((decimal)resultPoints);
    }
    
    private static Result<decimal> CalculateAgePoints(CatAge age)
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
        
        return Result.Success(points);
    }
    
    private static Result<decimal> CalculateColorPoints(CatColor color)
    {
        decimal points = color.Value switch
        {
            CatColor.ColorType.Black => 10,
            CatColor.ColorType.BlackAndWhite => 7,
            CatColor.ColorType.Tortoiseshell => 5,
            CatColor.ColorType.Tabby => 3,
            _ => 0
        };
        
        return Result.Success(points);
    }
    
    private static Result<decimal> CalculateGenderPoints(CatGender gender)
    {
        decimal points = gender.Value switch
        {
            CatGender.GenderType.Male => 5,
            _ => 0
        };
        
        return Result.Success(points);
    }
    
    private static Result<decimal> CalculateHealthPoints(HealthStatus healthStatus)
    {
        decimal points = healthStatus.Value switch
        {
            HealthStatus.StatusType.Critical => 40,
            HealthStatus.StatusType.ChronicIllness => 35,
            HealthStatus.StatusType.Recovering => 25,
            HealthStatus.StatusType.MinorIssues => 15,
            _ => 0
        };
        
        return Result.Success(points);
    }
    
    private static Result<decimal> CalculateListingSourcePoints(ListingSource listingSource)
    {
        decimal points = listingSource.Type switch
        {
            ListingSource.ListingSourceType.PrivatePersonUrgent => 20,
            ListingSource.ListingSourceType.PrivatePerson => 10,
            ListingSource.ListingSourceType.SmallRescueGroup => 8,
            ListingSource.ListingSourceType.Foundation => 5,
            ListingSource.ListingSourceType.Shelter => 3,
            _ => 0
        };
        
        return Result.Success(points);
    }
    
    private static Result<decimal> CalculateSpecialNeedsPoints(SpecialNeedsStatus specialNeeds)
    {
        int points = specialNeeds.SeverityType switch
        {
            SpecialNeedsStatus.SpecialNeedsSeverityType.Severe => 25,
            SpecialNeedsStatus.SpecialNeedsSeverityType.Moderate => 15,
            SpecialNeedsStatus.SpecialNeedsSeverityType.Minor => 8,
            _ => 0
        };
        
        return Result.Success((decimal)points);
    }
    
    private static Result<decimal> CalculateTemperamentPoints(Temperament temperament)
    {
        decimal points = temperament.Value switch
        {
            Temperament.TemperamentType.Aggressive => 15,
            Temperament.TemperamentType.VeryTimid => 12,
            Temperament.TemperamentType.Timid => 8,
            Temperament.TemperamentType.Independent => 5,
            _ => 0
        };
        
        return Result.Success(points);
    }
}