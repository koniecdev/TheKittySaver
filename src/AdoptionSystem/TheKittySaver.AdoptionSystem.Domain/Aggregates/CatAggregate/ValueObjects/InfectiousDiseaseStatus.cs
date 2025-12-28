using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class InfectiousDiseaseStatus : ValueObject
{
    public FivStatus FivStatus { get; }
    public FelvStatus FelvStatus { get; }
    public DateOnly LastTestedAt { get; }

    public bool HasFiv => FivStatus == FivStatus.Positive;
    public bool HasFelv => FelvStatus == FelvStatus.Positive;
    public bool HasAnyInfectiousDisease => HasFiv || HasFelv;
    public bool IsSafeToMixWithOtherCats => !HasAnyInfectiousDisease;

    public bool IsCompatibleWith(InfectiousDiseaseStatus other)
    {
        bool fivCompatible = FivStatus == other.FivStatus
                             || FivStatus is FivStatus.NotTested
                             || other.FivStatus is FivStatus.NotTested;

        bool felvCompatible = FelvStatus == other.FelvStatus
                              || FelvStatus is FelvStatus.NotTested
                              || other.FelvStatus is FelvStatus.NotTested;

        return fivCompatible && felvCompatible;
    }

    public static Result<InfectiousDiseaseStatus> Create(
        FivStatus fivStatus,
        FelvStatus felvStatus,
        DateOnly lastTestedAt,
        DateOnly currentDate)
    {
        if (lastTestedAt > currentDate)
        {
            return Result.Failure<InfectiousDiseaseStatus>(
                DomainErrors.CatEntity.InfectiousDiseaseStatusProperty.TestDateInFuture(lastTestedAt, currentDate));
        }

        if (CatAge.IsDateTooOldForCat(lastTestedAt, currentDate))
        {
            return Result.Failure<InfectiousDiseaseStatus>(
                DomainErrors.CatEntity.InfectiousDiseaseStatusProperty.TestDateTooOld(lastTestedAt, currentDate));
        }

        InfectiousDiseaseStatus instance = new(fivStatus, felvStatus, lastTestedAt);
        return Result.Success(instance);
    }

    private InfectiousDiseaseStatus(
        FivStatus fivStatus,
        FelvStatus felvStatus,
        DateOnly lastTestedAt)
    {
        FivStatus = fivStatus;
        FelvStatus = felvStatus;
        LastTestedAt = lastTestedAt;
    }

    public override string ToString() => string.Format(
        System.Globalization.CultureInfo.InvariantCulture,
        "FIV: {0}, FeLV: {1} (tested: {2:yyyy-MM-dd})",
        FivStatus,
        FelvStatus,
        LastTestedAt);

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return FivStatus;
        yield return FelvStatus;
        yield return LastTestedAt;
    }
}
