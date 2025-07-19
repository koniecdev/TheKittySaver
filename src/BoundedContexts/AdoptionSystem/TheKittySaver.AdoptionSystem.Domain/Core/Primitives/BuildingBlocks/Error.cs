namespace TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;

/// <summary>
/// Represents a concrete domain error.
/// </summary>
public sealed class Error(string code, string message) : ValueObject
{
    public string Code { get; } = code;
    
    public string Message { get; } = message;

    public static implicit operator string(Error error) => error.Code;
    
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Code;
        yield return Message;
    }

    /// <summary>
    /// Gets the empty error instance.
    /// </summary>
    internal static Error None => new(string.Empty, string.Empty);
}