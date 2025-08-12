using System.Text.RegularExpressions;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;

public sealed partial class Email : ValueObject
{
    public const int MaxLength = 250;
    public const string RegexPattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
    private static readonly Regex EmailRegex = MailRegex();
    public string Value { get; }
    
    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<Email>(DomainErrors.PersonEntity.EmailProperty.NullOrEmpty);
        }
        
        string trimmedValue = value.Trim();
        
        if (trimmedValue.Length > MaxLength)
        {
            return Result.Failure<Email>(DomainErrors.PersonEntity.EmailProperty.LongerThanAllowed);
        }
        
        if (!EmailRegex.IsMatch(trimmedValue))
        {
            return Result.Failure<Email>(DomainErrors.PersonEntity.EmailProperty.InvalidFormat);
        }
        
        Email instance = new(trimmedValue);
        return Result.Success(instance);
    }

    private Email(string value)
    {
        Value = value;
    }

    public static implicit operator string(Email value) => value.Value;
    public override string ToString() => Value;
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
    
    [GeneratedRegex(RegexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex MailRegex();
}