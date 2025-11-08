using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Calculators.CatPriorityScore;

internal sealed class DefaultAdoptionPriorityScoreCalculator : IAdoptionPriorityScoreCalculator
{
    public decimal Calculate(
        int returnCount,
        int age,
        ColorType color,
        CatGenderType gender,
        HealthStatusType healthStatus,
        ListingSourceType listingSource,
        SpecialNeedsSeverityType specialNeedsSeverity,
        TemperamentType temperament,
        FIVStatus fivStatus,
        FeLVStatus felvStatus,
        bool isNeutered)
    {
        decimal adoptionHistoryPoints = CalculateAdoptionHistoryPoints(returnCount);
        decimal agePoints = CalculateAgePoints(age);
        decimal colorPoints = CalculateColorPoints(color);
        decimal genderPoints = CalculateGenderPoints(gender);
        decimal healthPoints = CalculateHealthPoints(healthStatus);
        decimal listingSourcePoints = CalculateListingSourcePoints(listingSource);
        decimal specialNeedsPoints = CalculateSpecialNeedsPoints(specialNeedsSeverity);
        decimal temperamentPoints = CalculateTemperamentPoints(temperament);
        decimal infectiousDiseasePoints = CalculateInfectiousDiseasePoints(fivStatus, felvStatus);
        decimal neuteringPoints = CalculateNeuteringPoints(isNeutered);

        decimal totalPoints =
            adoptionHistoryPoints +
            agePoints +
            colorPoints +
            genderPoints +
            healthPoints +
            listingSourcePoints +
            specialNeedsPoints +
            temperamentPoints +
            infectiousDiseasePoints +
            neuteringPoints;

        return totalPoints;
    }
    
    private static decimal CalculateAdoptionHistoryPoints(int returnCount)
    {
        if (returnCount == 0)
        {
            return 0m;
        }

        int basePoints = returnCount * 10;
        int resultPoints = Math.Min(basePoints, 25);

        return resultPoints;
    }
    
    private static decimal CalculateAgePoints(int age)
    {
        decimal points = age switch
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
    
    private static decimal CalculateColorPoints(ColorType color)
    {
        decimal points = color switch
        {
            ColorType.Black => 10,
            ColorType.BlackAndWhite => 7,
            ColorType.Tortoiseshell => 5,
            ColorType.Tabby => 3,
            _ => 0
        };

        return points;
    }
    
    private static decimal CalculateGenderPoints(CatGenderType gender)
    {
        decimal points = gender switch
        {
            CatGenderType.Male => 5,
            _ => 0
        };

        return points;
    }
    
    private static decimal CalculateHealthPoints(HealthStatusType healthStatus)
    {
        decimal points = healthStatus switch
        {
            HealthStatusType.Critical => 40,
            HealthStatusType.ChronicIllness => 35,
            HealthStatusType.Recovering => 25,
            HealthStatusType.MinorIssues => 15,
            _ => 0
        };

        return points;
    }
    
    private static decimal CalculateListingSourcePoints(ListingSourceType listingSource)
    {
        decimal points = listingSource switch
        {
            ListingSourceType.PrivatePersonUrgent => 20,
            ListingSourceType.PrivatePerson => 18,
            ListingSourceType.Foundation => 10,
            ListingSourceType.Shelter => 5,
            _ => 0
        };

        return points;
    }
    
    private static decimal CalculateSpecialNeedsPoints(SpecialNeedsSeverityType specialNeedsSeverity)
    {
        int points = specialNeedsSeverity switch
        {
            SpecialNeedsSeverityType.Severe => 25,
            SpecialNeedsSeverityType.Moderate => 15,
            SpecialNeedsSeverityType.Minor => 8,
            _ => 0
        };

        return points;
    }
    
    private static decimal CalculateTemperamentPoints(TemperamentType temperament)
    {
        decimal points = temperament switch
        {
            TemperamentType.Aggressive => 15,
            TemperamentType.VeryTimid => 12,
            TemperamentType.Timid => 8,
            TemperamentType.Independent => 5,
            _ => 0
        };

        return points;
    }

    private static decimal CalculateInfectiousDiseasePoints(FIVStatus fivStatus, FeLVStatus felvStatus)
    {
        decimal points = 0m;

        if (fivStatus == FIVStatus.Positive)
        {
            points += 35;
        }

        if (felvStatus == FeLVStatus.Positive)
        {
            points += 35;
        }

        return points;
    }

    private static decimal CalculateNeuteringPoints(bool isNeutered)
    {
        return isNeutered ? 0m : 10m;
    }
}