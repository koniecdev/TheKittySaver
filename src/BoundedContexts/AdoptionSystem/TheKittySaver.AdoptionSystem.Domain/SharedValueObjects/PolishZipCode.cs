using System.Text.RegularExpressions;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;

public sealed partial class PolishZipCode : ValueObject
{
    private static readonly Regex PolishZipCodeRegex = ZipCodeRegex();
    
    public const int RequiredLength = 6;
    public const string ZipCodePattern = @"^\d{2}-\d{3}$";
    
    public string Value { get; }

    public static Result<PolishZipCode> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<PolishZipCode>(
                DomainErrors.PolishAddressEntity.ZipCodeProperty.NullOrEmpty);
        }
        
        value = value.Trim();
        
        if (value.Length != RequiredLength || !PolishZipCodeRegex.IsMatch(value))
        {
            return Result.Failure<PolishZipCode>(
                DomainErrors.PolishAddressEntity.ZipCodeProperty.InvalidFormat);
        }
        
        PolishZipCode instance = new(value);
        return Result.Success(instance);
    }

    private PolishZipCode(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;
    public static implicit operator string(PolishZipCode value) => value.Value;
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    [GeneratedRegex(ZipCodePattern)]
    private static partial Regex ZipCodeRegex();
}