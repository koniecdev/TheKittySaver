using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class InfectiousDiseaseStatus : ValueObject
{
    public FIVStatus FivStatus { get; }
    public FeLVStatus FelvStatus { get; }
    public DateOnly LastTestedAt { get; }
    
    public bool HasFiv => FivStatus == FIVStatus.Positive;
    public bool HasFelv => FelvStatus == FeLVStatus.Positive;
    public bool HasAnyInfectiousDisease => HasFiv || HasFelv;
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

        if (CatAge.IsDateTooOldForCat(lastTestedAt, currentDate))
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
        FivStatus = fivStatus;
        FelvStatus = felvStatus;
        LastTestedAt = lastTestedAt;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return FivStatus;
        yield return FelvStatus;
        yield return LastTestedAt;
    }
}