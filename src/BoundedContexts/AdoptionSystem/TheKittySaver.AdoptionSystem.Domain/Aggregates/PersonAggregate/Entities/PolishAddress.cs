using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Enums;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;

public sealed class PolishAddress : Entity<AddressId>
{
    public PolishAddress(AddressId id) : base(id)
    {
    }

    public AddressName Name { get; private set; }
    public PolandVoivodeship Voivodeship { get; private set; }
    public PolandCounty County { get; private set; }
    public PolishZipCode ZipCode { get; private set; }
    public City City { get; private set; }
    public Street Street { get; private set; }
    public BuildingNumber BuildingNumber { get; private set; }
    public ApartmentNumber ApartmentNumber { get; private set; }
}