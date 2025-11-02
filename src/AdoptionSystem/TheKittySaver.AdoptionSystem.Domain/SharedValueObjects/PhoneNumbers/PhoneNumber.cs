// Domain/Aggregates/PersonAggregate/ValueObjects/PhoneNumber.cs

using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;

namespace TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;

public sealed class PhoneNumber : ValueObject
{
    public const int MaxLength = 30;
    
    public string Value { get; }
    
    /// <summary>
    /// This method should only be called by respected PhoneNumberFactory that check for phoneNumber invariants. 
    /// Unsafe factory static method for creating Phone Number.
    /// Method do not perform any invariants validation.
    /// </summary>
    /// <param name="value">Actual phone number</param>
    /// <returns>Instance of PhoneNumber made out of <paramref name="value"/></returns>
    internal static PhoneNumber CreateUnsafe(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        PhoneNumber instance = new(value.Trim());
        return instance;
    }
    
    private PhoneNumber(string value)
    {
        Value = value;
    }
    
    public override string ToString() => Value;
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}