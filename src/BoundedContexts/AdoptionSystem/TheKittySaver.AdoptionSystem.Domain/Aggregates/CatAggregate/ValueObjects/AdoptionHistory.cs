using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class AdoptionHistory : ValueObject
{
    public int ReturnCount { get; }
    public DateTime? LastReturnDate { get; }
    public string? LastReturnReason { get; }
    
    public static AdoptionHistory NeverAdopted() 
        => new(0, null, null);
    
    public static Result<AdoptionHistory> Returned(int count, DateTime lastReturn, string reason)
    {
        if (count < 0)
        {
            return Result.Failure<AdoptionHistory>(DomainErrors.CatEntity.AdoptionHistory.CountTooLow);
        }
        if (lastReturn < DateTime.UnixEpoch)
        {
            return Result.Failure<AdoptionHistory>(DomainErrors.CatEntity.AdoptionHistory.LastReturnTooFarInPast);
        }
        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure<AdoptionHistory>(DomainErrors.CatEntity.AdoptionHistory.NoReasonProvided);
        }
        
        AdoptionHistory instance = new(count, lastReturn, reason);
        return instance;
    }

    private AdoptionHistory(int returnCount, DateTime? lastReturnDate, string? lastReturnReason)
    {
        ReturnCount = returnCount;
        LastReturnDate = lastReturnDate;
        LastReturnReason = lastReturnReason;
    }
    
    public decimal CalculatePriorityPoints()
    {
        if (ReturnCount == 0)
        {
            return 0;
        }
        
        int basePoints = ReturnCount * 10;
        
        return Math.Min(basePoints, 25);
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