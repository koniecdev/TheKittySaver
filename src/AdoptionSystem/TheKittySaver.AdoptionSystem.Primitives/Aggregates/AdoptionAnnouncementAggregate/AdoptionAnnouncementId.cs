namespace TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;

[StronglyTypedId(jsonConverter: StronglyTypedIdJsonConverter.SystemTextJson)]
public partial struct AdoptionAnnouncementId : IStronglyTypedId<AdoptionAnnouncementId>
{
    public static AdoptionAnnouncementId Create() => new(Guid.CreateVersion7());
    public static AdoptionAnnouncementId Create(Guid id) => new(id);
}
