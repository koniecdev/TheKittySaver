using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class ListingSource : ValueObject
{
    public enum ListingSourceType
    {
        Unset,
        Shelter,
        Foundation,
        SmallRescueGroup,
        PrivatePerson,
        PrivatePersonUrgent
    }

    public const int MaxSourceNameLength = 100;

    public ListingSourceType Type { get; }
    public string SourceName { get; }

    public Result<AdoptionPriorityScore> CalculatePriorityScore()
    {
        decimal points = Type switch
        {
            ListingSourceType.PrivatePersonUrgent => 20,
            ListingSourceType.PrivatePerson => 10,
            ListingSourceType.SmallRescueGroup => 8,
            ListingSourceType.Foundation => 5,
            ListingSourceType.Shelter => 3,
            _ => 0
        };

        Result<AdoptionPriorityScore> result = AdoptionPriorityScore.Create(points);

        return result;
    }

    public static Result<ListingSource> PrivatePerson(string name, bool isUrgent = false)
        => Create(isUrgent
            ? ListingSourceType.PrivatePersonUrgent
            : ListingSourceType.PrivatePerson, name);

    public static Result<ListingSource> Shelter(string shelterName)
        => Create(ListingSourceType.Shelter, shelterName);

    public static Result<ListingSource> Foundation(string foundationName)
        => Create(ListingSourceType.Foundation, foundationName);

    public static Result<ListingSource> RescueGroup(string groupName)
        => Create(ListingSourceType.SmallRescueGroup, groupName);

    private static Result<ListingSource> Create(ListingSourceType type, string sourceName)
    {
        if (type is ListingSourceType.Unset)
        {
            return Result.Failure<ListingSource>(DomainErrors.CatEntity.ListingSourceProperty.TypeIsUnset);
        }

        if (string.IsNullOrWhiteSpace(sourceName))
        {
            return Result.Failure<ListingSource>(DomainErrors.CatEntity.ListingSourceProperty.SourceNameIsNullOrEmpty);
        }

        sourceName = sourceName.Trim();
        if (sourceName.Length > MaxSourceNameLength)
        {
            return Result.Failure<ListingSource>(
                DomainErrors.CatEntity.ListingSourceProperty.SourceNameIsLongerThanAllowed);
        }

        ListingSource instance = new(type, sourceName);

        return Result.Success(instance);
    }

    private ListingSource(ListingSourceType type, string sourceName)
    {
        Type = type;
        SourceName = sourceName;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Type;
        yield return SourceName;
    }
}