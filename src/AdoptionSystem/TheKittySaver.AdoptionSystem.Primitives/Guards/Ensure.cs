using System.Runtime.CompilerServices;

namespace TheKittySaver.AdoptionSystem.Primitives.Guards;

public static class Ensure
{
    /// <summary>
    /// Ensures that the specified enum value is defined and not Unset (0).
    /// </summary>
    /// <remarks>
    /// Do not use with <see cref="FlagsAttribute"/> enums - combined flag values will fail validation.
    /// </remarks>
    /// <param name="value">The enum value to check.</param>
    /// <param name="argumentName">Automatically resolved with CallerArgumentExpression
    /// attribute name of the argument being checked.</param>
    /// <exception cref="ArgumentException">if the specified value is Unset (0).</exception>
    /// <exception cref="ArgumentOutOfRangeException">if the specified value is not defined in the enum.</exception>
    public static void IsValidNonDefaultEnum<TEnum>(
        TEnum value,
        [CallerArgumentExpression(nameof(value))] string argumentName = "")
        where TEnum : struct, Enum
    {
        if (EqualityComparer<TEnum>.Default.Equals(value, default))
        {
            throw new ArgumentException($"{argumentName} cannot be default.", argumentName);
        }

        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(argumentName, value,
                $"{argumentName} must be a defined enum value.");
        }
    }

    /// <summary>
    /// Ensures that the specified strongly-typed ID value is not empty.
    /// </summary>
    public static void NotEmpty<T>(
        T id,
        [CallerArgumentExpression(nameof(id))] string argumentName = "")
        where T : IStronglyTypedId<T>
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException($"{argumentName} cannot be empty.", argumentName);
        }
    }

    /// <summary>
    /// Ensures that the specified <see cref="DateTimeOffset"/> value is not the default value.
    /// </summary>
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
