using TheKittySaver.AdoptionSystem.Domain.Core.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;

/// <summary>
/// Represents a concrete domain error.
/// </summary>
public sealed class Error : ValueObject
{
    public string Code { get; }
    public string Message { get; }
    public TypeOfError Type { get; }

    public static Error None => new(string.Empty, string.Empty);

    public Error(string code, string message, TypeOfError type = TypeOfError.Failure)
    {
        Code = code;
        Message = message;
        Type = type;
    }
    
    public override string ToString()
    {
        return $"[{Type}] {Code}: {Message}";
    }
    
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Code;
        yield return Message;
        yield return Type;
    }
}