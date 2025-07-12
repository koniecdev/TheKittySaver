using System.Text.RegularExpressions;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.Results;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ValueObjects;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;

public sealed partial class Email : ValueObject
{
    public const int MaxLength = 250;
    public const string RegexPattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
    private static readonly Regex EmailRegex = MailRegex();
    public string Value { get; }
    
    public override string ToString() => Value;
    public static implicit operator string(Email value) => value.Value;
    
    public static Result<Email> Create(string value) =>
        Result.Create(value, DomainErrors.PersonEntity.EmailProperty.NullOrEmpty)
            .Ensure(v => !string.IsNullOrWhiteSpace(v), DomainErrors.PersonEntity.EmailProperty.NullOrEmpty)
            .Ensure(v => v.Length <= MaxLength, DomainErrors.PersonEntity.EmailProperty.LongerThanAllowed)
            .Ensure(v => EmailRegex.IsMatch(v), DomainErrors.PersonEntity.EmailProperty.InvalidFormat)
            .Map(v => new Email(v));
    
    private Email(string value) => Value = value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
    
    [GeneratedRegex(RegexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex MailRegex();
}