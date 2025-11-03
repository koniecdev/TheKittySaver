using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class AdoptionHistory : ValueObject
{
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
        if (counterHowManyTimesWasTheCatReturned < 0)
        {
            return Result.Failure<AdoptionHistory>(
                DomainErrors.CatEntity.AdoptionHistoryValueObject.CountTooLow);
        }

        const double averageOfDaysInOneYear = 365.2425;
        if (lastReturn < currentDate.Subtract(TimeSpan.FromDays(averageOfDaysInOneYear * CatAge.MaximumAllowedValue)))
        {
            return Result.Failure<AdoptionHistory>(
                DomainErrors.CatEntity.AdoptionHistoryValueObject.LastReturnTooFarInPast);
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure<AdoptionHistory>(
                DomainErrors.CatEntity.AdoptionHistoryValueObject.LastReturnReasonIsEmpty);
        }
    
        reason = reason.Trim();
        AdoptionHistory adoptionHistory = new(counterHowManyTimesWasTheCatReturned, lastReturn, reason);
    
        return Result.Success(adoptionHistory);
    }

    private AdoptionHistory(int returnCount, DateTimeOffset? lastReturnDate, string? lastReturnReason)
    {
        ReturnCount = returnCount;
        LastReturnDate = lastReturnDate;
        LastReturnReason = lastReturnReason;
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