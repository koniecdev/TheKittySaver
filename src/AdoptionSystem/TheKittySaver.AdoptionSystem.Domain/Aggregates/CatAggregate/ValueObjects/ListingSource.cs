using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class ListingSource : ValueObject
{
    public const int MaxSourceNameLength = 100;

    public ListingSourceType Type { get; }
    public string SourceName { get; }

    public static Result<ListingSource> PrivatePerson(string name, bool isUrgent = false)
        => Create(isUrgent
            ? ListingSourceType.PrivatePersonUrgent
            : ListingSourceType.PrivatePerson, name);

    public static Result<ListingSource> Shelter(string shelterName)
        => Create(ListingSourceType.Shelter, shelterName);

    public static Result<ListingSource> Foundation(string foundationName)
        => Create(ListingSourceType.Foundation, foundationName);

    private static Result<ListingSource> Create(ListingSourceType type, string sourceName)
    {
        if (type is ListingSourceType.Unset)
        {
            return Result.Failure<ListingSource>(DomainErrors.CatEntity.ListingSourceProperty.TypeIsUnset);
        }

        if (string.IsNullOrWhiteSpace(sourceName))
        {
            return Result.Failure<ListingSource>(DomainErrors.CatEntity.ListingSourceProperty.SourceNameNullOrEmpty);
        }

        sourceName = sourceName.Trim();

        if (sourceName.Length > MaxSourceNameLength)
        {
            return Result.Failure<ListingSource>(
                DomainErrors.CatEntity.ListingSourceProperty.SourceNameLongerThanAllowed);
        }

        ListingSource instance = new(type, sourceName);

        return Result.Success(instance);
    }

    private ListingSource(ListingSourceType type, string sourceName)
    {
        Type = type;
        SourceName = sourceName;
    }

    public override string ToString() => $"{Type}: {SourceName}";

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Type;
        yield return SourceName;
    }
}
