namespace TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

[StronglyTypedId(jsonConverter: StronglyTypedIdJsonConverter.SystemTextJson)]
public partial struct VaccinationId : IStronglyTypedId<VaccinationId>
{
    public static VaccinationId Create(Guid id) => new(id);
}
    
