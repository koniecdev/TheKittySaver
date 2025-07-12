using System.Runtime.CompilerServices;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Guards;

public static class Ensure
{
    /// <summary>
    /// Ensures that the specified <see cref="Guid"/> value is not empty.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="argumentName">Automatically resolved with CallerArgumentExpression
    /// attribute name of the argument being checked.</param>
    /// <exception cref="ArgumentException">if the specified value is empty.</exception>
    public static void NotEmpty(
        Guid value,
        [CallerArgumentExpression(nameof(value))] string argumentName = "")
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException($"{argumentName} cannot be empty.", argumentName);
        }
    }

    /// <summary>
    /// Ensures that the specified <see cref="DateTimeOffset"/> value is not empty.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="argumentName">Automatically resolved with CallerArgumentExpression
    /// attribute name of the argument being checked.</param>
    /// <exception cref="ArgumentException">if the specified value is the default value for the type.</exception>
    public static void NotEmpty(
        DateTimeOffset value,
        [CallerArgumentExpression(nameof(value))] string argumentName = "")
    {
        if (value == default)
        {
            throw new ArgumentException($"{argumentName} cannot be default.", argumentName);
        }
    }
}