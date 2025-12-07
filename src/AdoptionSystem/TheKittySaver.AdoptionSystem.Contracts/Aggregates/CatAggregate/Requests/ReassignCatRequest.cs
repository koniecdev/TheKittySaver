using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;

namespace TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Requests;

public sealed record ReassignCatRequest(
    AdoptionAnnouncementId DestinationAdoptionAnnouncementId);
