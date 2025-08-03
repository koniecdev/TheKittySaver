using System.Text.RegularExpressions;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;

public sealed partial class PolishZipCode : ValueObject
{
    public const int Length = 6;
    private const string ZipCodePattern = @"^\d{2}-\d{3}$";
    public string Value { get; }
    public override string ToString() => Value;
    public static implicit operator string(PolishZipCode value) => value.Value;

    public static Result<PolishZipCode> Create(string value)
    {
        Result<PolishZipCode> result = Result
            .Create(value, DomainErrors.PolishAddressEntity.ZipCodeProperty.NullOrEmpty)
            .TrimValue()
            .Ensure(v => !string.IsNullOrWhiteSpace(v),
                DomainErrors.PolishAddressEntity.ZipCodeProperty.NullOrEmpty)
            .Ensure(v => ZipCodeRegex().IsMatch(v),
                DomainErrors.PolishAddressEntity.ZipCodeProperty.InvalidFormat)
            .Map(v => new PolishZipCode(v));
        return result;
    }

    private PolishZipCode(string value) => Value = value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    [GeneratedRegex(ZipCodePattern)]
    private static partial Regex ZipCodeRegex();
}