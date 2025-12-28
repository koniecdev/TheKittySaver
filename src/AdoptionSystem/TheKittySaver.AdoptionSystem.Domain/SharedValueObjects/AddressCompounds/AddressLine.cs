using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds;

public sealed class AddressLine : ValueObject
{
    public const int MaxLength = 500;
    public string Value { get; }

    public static Result<AddressLine> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<AddressLine>(DomainErrors.AddressLineValueObject.NullOrEmpty);
        }

        value = value.Trim();

        if (value.Length > MaxLength)
        {
            return Result.Failure<AddressLine>(DomainErrors.AddressLineValueObject.LongerThanAllowed);
        }

        AddressLine instance = new(value);
        return Result.Success(instance);
    }

    private AddressLine(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
