using Bogus;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Extensions;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Factories;

internal static class CatFactory
{
    public static Cat CreateRandom(
        Faker faker,
        PersonId? personId = null,
        bool replacePersonIdWithEmpty = false,
        bool replaceNameWithNull = false,
        bool replaceDescriptionWithNull = false,
        bool replaceAgeWithNull = false,
        bool replaceGenderWithNull = false,
        bool replaceColorWithNull = false,
        bool replaceWeightWithNull = false,
        bool replaceHealthStatusWithNull = false,
        bool replaceSpecialNeedsWithNull = false,
        bool replaceTemperamentWithNull = false,
        bool replaceAdoptionHistoryWithNull = false,
        bool replaceListingSourceWithNull = false,
        bool replaceNeuteringStatusWithNull = false,
        bool replaceInfectiousDiseaseStatusWithNull = false)
    {
        PersonId thePersonId = personId ?? PersonId.New();

        CatName name = CreateRandomName(faker);
        CatDescription description = CreateRandomDescription(faker);
        CatAge age = CreateRandomAge(faker);
        CatGender gender = faker.PickRandomParam(CatGender.Male(), CatGender.Female());
        CatColor color = faker.PickRandomParam(
            CatColor.Black(),
            CatColor.BlackAndWhite(),
            CatColor.Calico(),
            CatColor.Gray(),
            CatColor.Orange(),
            CatColor.Other(),
            CatColor.Tabby(),
            CatColor.Tortoiseshell(),
            CatColor.White());
        CatWeight weight = CreateRandomWeight(faker);
        HealthStatus healthStatus = faker.PickRandomParam(
            HealthStatus.Healthy(),
            HealthStatus.MinorIssues(),
            HealthStatus.ChronicIllness(),
            HealthStatus.Recovering(),
            HealthStatus.Critical());
        SpecialNeedsStatus specialNeeds = faker.PickRandomParam(
            SpecialNeedsStatus.None(),
            SpecialNeedsStatus.Create(
                faker.Lorem.Word(), faker.PickRandomWithout(SpecialNeedsSeverityType.Unset)).Value);
        Temperament temperament = faker.PickRandomParam(
            Temperament.Friendly(),
            Temperament.Aggressive(),
            Temperament.Independent(),
            Temperament.Timid(),
            Temperament.VeryTimid());
        AdoptionHistory adoptionHistory = faker.PickRandomParam(
            AdoptionHistory.CatHasNeverBeenAdopted,
            AdoptionHistory.CatHasBeenReturned(
                counterHowManyTimesWasTheCatReturned: faker.PickRandomParam(1, 2, 3),
                currentDate: new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero),
                lastReturn: new DateTimeOffset(2025, 5, 1, 0, 0, 0, TimeSpan.Zero),
                reason: faker.Lorem.Sentence()).Value);
        ListingSource listingSource = CreateRandomListingSource(faker);
        NeuteringStatus neuteringStatus = faker.PickRandomParam(
            NeuteringStatus.NotNeutered(),
            NeuteringStatus.Neutered());
        InfectiousDiseaseStatus infectiousDiseaseStatus = CreateFixedNormalInfectiousDiseaseStatus();

        Result<Cat> catResult = Cat.Create(
            personId: replacePersonIdWithEmpty ? PersonId.Empty : thePersonId,
            name: replaceNameWithNull ? null! : name,
            description: replaceDescriptionWithNull ? null! : description,
            age: replaceAgeWithNull ? null! : age,
            gender: replaceGenderWithNull ? null! : gender,
            color: replaceColorWithNull ? null! : color,
            weight: replaceWeightWithNull ? null! : weight,
            healthStatus: replaceHealthStatusWithNull ? null! : healthStatus,
            specialNeeds: replaceSpecialNeedsWithNull ? null! : specialNeeds,
            temperament: replaceTemperamentWithNull ? null! : temperament,
            adoptionHistory: replaceAdoptionHistoryWithNull ? null! : adoptionHistory,
            listingSource: replaceListingSourceWithNull ? null! : listingSource,
            neuteringStatus: replaceNeuteringStatusWithNull ? null! : neuteringStatus,
            infectiousDiseaseStatus: replaceInfectiousDiseaseStatusWithNull ? null! : infectiousDiseaseStatus);
        catResult.EnsureSuccess();

        return catResult.Value;
    }

    public static Cat CreateWithThumbnail(Faker faker, PersonId? personId = null)
    {
        Cat cat = CreateRandom(faker, personId: personId);
        cat.UpsertThumbnail();
        return cat;
    }

    public static CatName CreateRandomName(Faker faker)
    {
        Result<CatName> result = CatName.Create(faker.Name.FirstName());
        result.EnsureSuccess();
        return result.Value;
    }

    public static CatDescription CreateRandomDescription(Faker faker)
    {
        Result<CatDescription> result = CatDescription.Create(faker.Lorem.Sentence(10));
        result.EnsureSuccess();
        return result.Value;
    }

    public static CatAge CreateRandomAge(Faker faker)
    {
        Result<CatAge> result = CatAge.Create(faker.Random.Int(CatAge.MinimumAllowedValue, CatAge.MaximumAllowedValue));
        result.EnsureSuccess();
        return result.Value;
    }

    public static CatWeight CreateRandomWeight(Faker faker)
    {
        Result<CatWeight> result = CatWeight.Create(faker.Random.Decimal(CatWeight.MinWeightKg, CatWeight.MaxWeightKg));
        result.EnsureSuccess();
        return result.Value;
    }

    public static ListingSource CreateRandomListingSource(Faker faker)
    {
        Result<ListingSource> result = faker.PickRandomParam(
            ListingSource.Shelter(faker.Company.CompanyName()),
            ListingSource.PrivatePerson(faker.Name.FullName()),
            ListingSource.Foundation(faker.Company.CompanyName()));
        result.EnsureSuccess();
        return result.Value;
    }

    private static InfectiousDiseaseStatus CreateFixedNormalInfectiousDiseaseStatus()
    {
        DateOnly currentDate = new(2025, 6, 1);
        DateOnly lastTestedAt = new(2025, 5, 1);
        Result<InfectiousDiseaseStatus> result = InfectiousDiseaseStatus.Create(
            FivStatus.Negative,
            FelvStatus.Negative,
            lastTestedAt,
            currentDate);
        result.EnsureSuccess();
        return result.Value;
    }
    
    public static InfectiousDiseaseStatus CreateRandomInfectiousDiseaseStatus(Faker faker)
    {
        DateOnly currentDate = new(2025, 6, 1);
        DateOnly lastTestedAt = new(2025, 5, 1);
        Result<InfectiousDiseaseStatus> result = InfectiousDiseaseStatus.Create(
            faker.PickRandomParam(FivStatus.Negative, FivStatus.Positive, FivStatus.NotTested),
            faker.PickRandomParam(FelvStatus.Negative, FelvStatus.Positive, FelvStatus.NotTested),
            lastTestedAt,
            currentDate);
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
