using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public record ListingSource
{
    public ListingSourceType Type { get; }
    public string SourceName { get; }
    public string? ContactInfo { get; }
    
    private ListingSource(ListingSourceType type, string sourceName, string? contactInfo)
    {
        Type = type;
        SourceName = sourceName;
        ContactInfo = contactInfo;
    }
    
    public static ListingSource PrivatePerson(string name, string contact, bool isUrgent = false)
        => new(isUrgent ? ListingSourceType.PrivatePersonUrgent : ListingSourceType.PrivatePerson, name, contact);
    
    public static ListingSource Shelter(string shelterName)
        => new(ListingSourceType.Shelter, shelterName, null);
    
    public static ListingSource Foundation(string foundationName)
        => new(ListingSourceType.Foundation, foundationName, null);
    
    public static ListingSource RescueGroup(string groupName)
        => new(ListingSourceType.SmallRescueGroup, groupName, null);
}