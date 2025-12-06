using System.Globalization;
using System.Runtime.CompilerServices;

namespace TheKittySaver.AdoptionSystem.Primitives.Guards;

public static class Ensure
{
    /// <summary>
    /// Ensures that the specified <see cref="Guid"/> value is not empty.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="argumentName">Automatically resolved with CallerArgumentExpression
    /// attribute name of the argument being checked.</param>
    /// <exception cref="ArgumentException">if the specified value is empty.</exception>
    public static void IsInEnum<TEnum>(
        TEnum value,
        [CallerArgumentExpression(nameof(value))] string argumentName = "")
        where TEnum : struct, Enum
    {
        if (Convert.ToInt32(value, CultureInfo.InvariantCulture) == 0)
        {
            throw new ArgumentException($"{argumentName} must have a valid value.", argumentName);
        }
    }
    
    /// <summary>
    /// Ensures that the specified <see cref="Guid"/> value is not empty.
    /// </summary>
    /// <param name="id">The ID to check.</param>
    /// <param name="argumentName">Automatically resolved with CallerArgumentExpression
    /// attribute name of the argument being checked.</param>
    /// <exception cref="ArgumentException">if the specified value is empty.</exception>
    public static void NotEmpty<T>(
        T id,
        [CallerArgumentExpression(nameof(id))] string argumentName = "") where T : IStronglyTypedId<T>
    {
        if (id.Value == Guid.Empty)
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
