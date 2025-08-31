using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class AdoptionHistory : ValueObject
{
    public int ReturnCount { get; }
    public DateTime? LastReturnDate { get; }
    public string? LastReturnReason { get; }
    
    public static AdoptionHistory CatHasNeverBeenAdopted 
        => new(0, null, null);
    
    public static Result<AdoptionHistory> CatHasBeenReturned(
        int countHowManyTimesWasTheCatReturned,
        DateTime lastReturn,
        string reason)
    {
        if (countHowManyTimesWasTheCatReturned < 0)
        {
            return Result.Failure<AdoptionHistory>(
                DomainErrors.CatEntity.AdoptionHistoryProperty.CountTooLow);
        }
    
        if (lastReturn < DateTime.UnixEpoch)
        {
            return Result.Failure<AdoptionHistory>(
                DomainErrors.CatEntity.AdoptionHistoryProperty.LastReturnTooFarInPast);
        }
    
        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure<AdoptionHistory>(
                DomainErrors.CatEntity.AdoptionHistoryProperty.LastReturnReasonIsEmpty);
        }
    
        reason = reason.Trim();
        AdoptionHistory adoptionHistory = new(countHowManyTimesWasTheCatReturned, lastReturn, reason);
    
        return Result.Success(adoptionHistory);
    }

    private AdoptionHistory(int returnCount, DateTime? lastReturnDate, string? lastReturnReason)
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