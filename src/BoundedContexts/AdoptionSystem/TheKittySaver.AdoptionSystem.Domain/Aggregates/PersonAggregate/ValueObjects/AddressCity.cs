using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;

public sealed class City : ValueObject
{
    public const int MaxLength = 100;
    public string Value { get; }
    
    public override string ToString() => Value;
    public static implicit operator string(City value) => value.Value;
    
    public static Result<City> Create(string value)
    {
        Result<City> result = Result.Create(value, DomainErrors.PolishAddressEntity.CityProperty.NullOrEmpty)
            .TrimValue()
            .Ensure(v => !string.IsNullOrWhiteSpace(v), DomainErrors.PolishAddressEntity.CityProperty.NullOrEmpty)
            .Ensure(v => v.Length <= MaxLength, DomainErrors.PolishAddressEntity.CityProperty.LongerThanAllowed)
            .Map(v => new City(v));
        return result;
    }

    private City(string value) => Value = value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}