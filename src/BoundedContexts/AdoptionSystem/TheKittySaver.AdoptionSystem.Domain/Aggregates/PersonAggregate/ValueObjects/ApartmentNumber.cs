using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.Results;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ValueObjects;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;

public sealed class ApartmentNumber : ValueObject
{
    public const int MaxLength = 10;
    public string? Value { get; }
    public override string ToString() => Value ?? string.Empty;
    public static implicit operator string?(ApartmentNumber value) => value.Value;

    public static Result<ApartmentNumber> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Success(new ApartmentNumber(null));
        }

        return value.Length > MaxLength
            ? Result.Failure<ApartmentNumber>(
                DomainErrors.PolishAddressEntity.ApartmentNumberProperty.LongerThanAllowed)
            : Result.Success(new ApartmentNumber(value));
    }

    private ApartmentNumber(string? value) => Value = value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        // Dla porównań: null jest traktowane jako osobna wartość
        yield return Value ?? string.Empty;
    }
}