using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;

public sealed class AdoptionAnnouncementDescription : ValueObject
{
    public const int MaxLength = 250;
    public string Value { get; }

    public static Result<AdoptionAnnouncementDescription> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<AdoptionAnnouncementDescription>(DomainErrors.AdoptionAnnouncementEntity.DescriptionProperty.NullOrEmpty);
        }

        value = value.Trim();
        
        if (value.Length > MaxLength)
        {
            return Result.Failure<AdoptionAnnouncementDescription>(DomainErrors.AdoptionAnnouncementEntity.DescriptionProperty.LongerThanAllowed);
        }
        
        AdoptionAnnouncementDescription instance = new(value);
        return Result.Success(instance);
    }

    private AdoptionAnnouncementDescription(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
