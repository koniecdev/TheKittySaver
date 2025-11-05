using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;

public sealed class Description : ValueObject
{
    public const int MaxLength = 400;
    public string Value { get; }

    public static Result<Description> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<Description>(DomainErrors.CatEntity.DescriptionValueObject.NullOrEmpty);
        }

        if (value.Length > MaxLength)
        {
            return Result.Failure<Description>(DomainErrors.CatEntity.DescriptionValueObject.LongerThanAllowed);
        }

        value = value.Trim();

        Description instance = new(value);
        return Result.Success(instance);
    }

    private Description(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}