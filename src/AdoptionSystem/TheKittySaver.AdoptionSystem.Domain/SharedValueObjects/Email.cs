using System.Text.RegularExpressions;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;

public sealed partial class Email : ValueObject
{
    private static readonly Regex EmailRegex = MailRegex();
    
    public const int MaxLength = 250;
    public const string RegexPattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
    
    public string Value { get; }
    
    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<Email>(DomainErrors.EmailValueObject.NullOrEmpty);
        }

        value = value.Trim();

        if (value.Length > MaxLength)
        {
            return Result.Failure<Email>(DomainErrors.EmailValueObject.LongerThanAllowed);
        }

        if (!EmailRegex.IsMatch(value))
        {
            return Result.Failure<Email>(DomainErrors.EmailValueObject.InvalidFormat);
        }
        
        Email instance = new(value);
        return Result.Success(instance);
    }

    private Email(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
    
    [GeneratedRegex(RegexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex MailRegex();
}