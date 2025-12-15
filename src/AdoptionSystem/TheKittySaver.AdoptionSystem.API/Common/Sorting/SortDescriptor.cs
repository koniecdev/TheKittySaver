using System.Diagnostics.CodeAnalysis;

namespace TheKittySaver.AdoptionSystem.API.Common.Sorting;

internal sealed class SortDescriptor
{
    public string PropertyName { get; }
    public bool IsDescending { get; }

    private SortDescriptor(string propertyName, bool isDescending)
    {
        PropertyName = propertyName;
        IsDescending = isDescending;
    }

    public static bool TryParse(string? sortString, [NotNullWhen(true)] out SortDescriptor? descriptor)
    {
        descriptor = null;

        if (string.IsNullOrWhiteSpace(sortString))
        {
            return false;
        }

        ReadOnlySpan<char> span = sortString.AsSpan().Trim();
        int colonIndex = span.IndexOf(':');

        if (colonIndex <= 0 || colonIndex >= span.Length - 1)
        {
            return false;
        }

        ReadOnlySpan<char> propertyName = span[..colonIndex].Trim();
        ReadOnlySpan<char> direction = span[(colonIndex + 1)..].Trim();

        if (propertyName.IsEmpty)
        {
            return false;
        }

        bool isDescending;
        if (direction.Equals("asc", StringComparison.OrdinalIgnoreCase))
        {
            isDescending = false;
        }
        else if (direction.Equals("desc", StringComparison.OrdinalIgnoreCase))
        {
            isDescending = true;
        }
        else
        {
            return false;
        }

        descriptor = new SortDescriptor(propertyName.ToString(), isDescending);
        return true;
    }
}
