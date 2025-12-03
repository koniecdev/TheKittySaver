using Bogus;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Extensions;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Factories;

public static class AdoptionAnnouncementFactory
{
    public static AdoptionAnnouncement CreateRandom(
        Faker faker,
        PersonId? personId = null,
        bool replacePersonIdWithEmpty = false,
        bool replaceDescriptionWithNull = false,
        bool replaceAddressWithNull = false,
        bool replaceEmailWithNull = false,
        bool replacePhoneNumberWithNull = false,
        bool replaceCreatedAtWithNull = false)
    {
        PersonId thePersonId = personId ?? PersonId.New();

        AdoptionAnnouncementDescription description = CreateRandomDescription(faker);
        Maybe<AdoptionAnnouncementDescription> maybeDescription 
            = Maybe<AdoptionAnnouncementDescription>.From(description);

        AdoptionAnnouncementAddress address = CreateRandomAddress(faker);
        Email email = CreateRandomEmail(faker);
        PhoneNumber phoneNumber = CreateRandomPhoneNumber(faker);
        CreatedAt createdAt = CreateDefaultCreatedAt();

        Result<AdoptionAnnouncement> result = AdoptionAnnouncement.Create(
            personId: replacePersonIdWithEmpty ? PersonId.Empty : thePersonId,
            description: replaceDescriptionWithNull ? null! : maybeDescription,
            address: replaceAddressWithNull ? null! : address,
            email: replaceEmailWithNull ? null! : email,
            phoneNumber: replacePhoneNumberWithNull ? null! : phoneNumber,
            createdAt: replaceCreatedAtWithNull ? null! : createdAt);
        result.EnsureSuccess();

        return result.Value;
    }

    public static AdoptionAnnouncementDescription CreateRandomDescription(Faker faker)
    {
        Result<AdoptionAnnouncementDescription> result = AdoptionAnnouncementDescription.Create(
            faker.Lorem.Sentence(10));
        result.EnsureSuccess();
        return result.Value;
    }

    public static AdoptionAnnouncementAddress CreateRandomAddress(Faker faker)
    {
        Result<AddressCity> cityResult = AddressCity.Create(faker.Address.City());
        cityResult.EnsureSuccess();

        Result<AddressRegion> regionResult = AddressRegion.Create(faker.Address.State());
        regionResult.EnsureSuccess();

        Result<AddressLine> lineResult = AddressLine.Create(faker.Address.StreetAddress());
        lineResult.EnsureSuccess();

        Result<AdoptionAnnouncementAddress> result = AdoptionAnnouncementAddress.Create(
            cityResult.Value,
            regionResult.Value,
            Maybe<AddressLine>.From(lineResult.Value));
        result.EnsureSuccess();

        return result.Value;
    }

    public static Email CreateRandomEmail(Faker faker)
    {
        Result<Email> result = Email.Create(faker.Person.Email);
        result.EnsureSuccess();
        return result.Value;
    }

    public static PhoneNumber CreateRandomPhoneNumber(Faker faker)
    {
        PhoneNumber phoneNumber = PhoneNumber.CreateUnsafe(faker.Person.Phone);
        return phoneNumber;
    }

    public static CreatedAt CreateDefaultCreatedAt()
    {
        Result<CreatedAt> result = CreatedAt.Create(
            new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero));
        result.EnsureSuccess();
        return result.Value;
    }

    public static ClaimedAt CreateDefaultClaimedAt()
    {
        Result<ClaimedAt> result = ClaimedAt.Create(
            new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero));
        result.EnsureSuccess();
        return result.Value;
    }
}
