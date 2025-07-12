using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.Results;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ValueObjects;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;

public sealed class BuildingNumber : ValueObject
{
    public const int MaxLength = 10;
    public string? Value { get; }
    public override string ToString() => Value ?? string.Empty;
    public static implicit operator string?(BuildingNumber value) => value.Value;
    
    public static Result<BuildingNumber> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Success(new BuildingNumber(null));
        }
        
        return value.Length > MaxLength 
            ? Result.Failure<BuildingNumber>(DomainErrors.PolishAddressEntity.BuildingNumberProperty.LongerThanAllowed)
            : Result.Success(new BuildingNumber(value));
    }

    private BuildingNumber(string? value) => Value = value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value ?? string.Empty;
    }
}