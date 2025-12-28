using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds;

public sealed class AddressPostalCode : ValueObject
{
    public const int MaxLength = 20;
    public string Value { get; }

    public static Result<AddressPostalCode> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<AddressPostalCode>(DomainErrors.AddressPostalCodeValueObject.NullOrEmpty);
        }

        value = value.Trim();

        if (value.Length > MaxLength)
        {
            return Result.Failure<AddressPostalCode>(DomainErrors.AddressPostalCodeValueObject.LongerThanAllowed);
        }

        AddressPostalCode instance = new(value);
        return Result.Success(instance);
    }

    private AddressPostalCode(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
