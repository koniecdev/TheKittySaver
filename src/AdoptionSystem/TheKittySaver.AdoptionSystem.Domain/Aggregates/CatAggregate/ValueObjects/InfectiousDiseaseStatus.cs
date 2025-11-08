using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class InfectiousDiseaseStatus : ValueObject
{
    public FIVStatus FIVStatus { get; }
    public FeLVStatus FeLVStatus { get; }
    public DateOnly LastTestedAt { get; }
    
    public bool HasFIV => FIVStatus == FIVStatus.Positive;
    public bool HasFeLV => FeLVStatus == FeLVStatus.Positive;
    public bool HasAnyInfectiousDisease => HasFIV || HasFeLV;
    public bool IsSafeToMixWithOtherCats => !HasAnyInfectiousDisease;
    
    public static Result<InfectiousDiseaseStatus> Create(
        FIVStatus fivStatus,
        FeLVStatus felvStatus,
        DateOnly lastTestedAt,
        DateOnly currentDate)
    {
        if (lastTestedAt > currentDate)
        {
            return Result.Failure<InfectiousDiseaseStatus>(
                DomainErrors.CatEntity.InfectiousDiseaseStatusValueObject.TestDateInFuture);
        }
        
        //todo: I believe this could be exctracted
        const double averageOfDaysInOneYear = 365.2425;
        if (lastTestedAt < currentDate.AddDays((int)(-averageOfDaysInOneYear * CatAge.MaximumAllowedValue)))
        {
            return Result.Failure<InfectiousDiseaseStatus>(
                DomainErrors.CatEntity.InfectiousDiseaseStatusValueObject.TestDateTooOld);
        }

        InfectiousDiseaseStatus instance = new(fivStatus, felvStatus, lastTestedAt);
        return Result.Success(instance);
    }

    private InfectiousDiseaseStatus(
        FIVStatus fivStatus,
        FeLVStatus felvStatus,
        DateOnly lastTestedAt)
    {
        FIVStatus = fivStatus;
        FeLVStatus = felvStatus;
        LastTestedAt = lastTestedAt;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return FIVStatus;
        yield return FeLVStatus;
        yield return LastTestedAt;
    }
}