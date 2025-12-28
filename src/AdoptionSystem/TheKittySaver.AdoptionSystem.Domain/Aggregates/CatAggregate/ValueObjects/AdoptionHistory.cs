using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Guards;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class AdoptionHistory : ValueObject
{
    public const int LastReturnReasonMaxLength = 2000;
    public int ReturnCount { get; }
    public DateTimeOffset? LastReturnDate { get; }
    public string? LastReturnReason { get; }

    public static AdoptionHistory CatHasNeverBeenAdopted
        => new(0, null, null);

    public static Result<AdoptionHistory> CatHasBeenReturned(
        int counterHowManyTimesWasTheCatReturned,
        DateTimeOffset currentDate,
        DateTimeOffset lastReturn,
        string reason)
    {
        Ensure.NotEmpty(currentDate);
        Ensure.NotEmpty(lastReturn);

        if (counterHowManyTimesWasTheCatReturned < 0)
        {
            return Result.Failure<AdoptionHistory>(
                DomainErrors.CatEntity.AdoptionHistoryProperty.CountTooLow);
        }

        if (CatAge.IsDateTooOldForCat(lastReturn, currentDate))
        {
            return Result.Failure<AdoptionHistory>(
                DomainErrors.CatEntity.AdoptionHistoryProperty.LastReturnTooFarInPast(lastReturn, currentDate));
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure<AdoptionHistory>(
                DomainErrors.CatEntity.AdoptionHistoryProperty.LastReturnReasonIsEmpty);
        }

        reason = reason.Trim();

        if (reason.Length > LastReturnReasonMaxLength)
        {
            return Result.Failure<AdoptionHistory>(DomainErrors.CatEntity.AdoptionHistoryProperty.LongerThanAllowed);
        }


        AdoptionHistory adoptionHistory = new(counterHowManyTimesWasTheCatReturned, lastReturn, reason);

        return Result.Success(adoptionHistory);
    }

    private AdoptionHistory(int returnCount, DateTimeOffset? lastReturnDate, string? lastReturnReason)
    {
        ReturnCount = returnCount;
        LastReturnDate = lastReturnDate;
        LastReturnReason = lastReturnReason;
    }

    public override string ToString()
    {
        if (ReturnCount == 0)
        {
            return "Never returned";
        }

        return string.Format(
            System.Globalization.CultureInfo.InvariantCulture,
            "Returned {0} time(s), last: {1:yyyy-MM-dd}",
            ReturnCount,
            LastReturnDate);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return ReturnCount;

        if (LastReturnDate is not null)
        {
            yield return LastReturnDate;
        }

        if (LastReturnReason is not null)
        {
            yield return LastReturnReason;
        }
    }
}
