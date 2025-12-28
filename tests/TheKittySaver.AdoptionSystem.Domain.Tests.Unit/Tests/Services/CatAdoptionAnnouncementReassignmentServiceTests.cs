using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.Services.CatAdoptionAnnouncementReassignmentServices;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Extensions;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Factories;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Services;

public sealed class CatAdoptionAnnouncementReassignmentServiceTests
{
    private static readonly Faker Faker = new();
    private static readonly DateTimeOffset OperationDate = new(2025, 6, 1, 12, 0, 0, TimeSpan.Zero);
    private readonly CatAdoptionAnnouncementReassignmentService _service = new();

    [Fact]
    public void ReassignCatToAnotherAdoptionAnnouncement_ShouldSucceed_WhenAllConditionsAreMet()
    {
        //Arrange
        PersonId personId = PersonId.Create();
        AdoptionAnnouncement sourceAnnouncement = CreateActiveAnnouncement(personId);
        AdoptionAnnouncement destinationAnnouncement = CreateActiveAnnouncement(personId);

        Cat cat = CreatePublishedCat(personId, sourceAnnouncement.Id);
        IReadOnlyCollection<Cat> destinationCats = [];

        //Act
        Result result = _service.ReassignCatToAnotherAdoptionAnnouncement(
            cat,
            sourceAnnouncement,
            destinationAnnouncement,
            destinationCats,
            OperationDate);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.AdoptionAnnouncementId.ShouldBe(destinationAnnouncement.Id);
        cat.Status.ShouldBe(CatStatusType.Published); // Should remain Published
    }

    [Fact]
    public void ReassignCatToAnotherAdoptionAnnouncement_ShouldSucceed_WhenDiseaseStatusesAreCompatible()
    {
        //Arrange
        PersonId personId = PersonId.Create();
        InfectiousDiseaseStatus fivPositive = CreateDiseaseStatus(FivStatus.Positive, FelvStatus.Negative);

        AdoptionAnnouncement sourceAnnouncement = CreateActiveAnnouncement(personId);
        AdoptionAnnouncement destinationAnnouncement = CreateActiveAnnouncement(personId);

        Cat catToReassign = CreatePublishedCat(personId, sourceAnnouncement.Id, fivPositive);
        Cat existingCatInDestination = CreatePublishedCat(personId, destinationAnnouncement.Id, fivPositive);

        //Act
        Result result = _service.ReassignCatToAnotherAdoptionAnnouncement(
            catToReassign,
            sourceAnnouncement,
            destinationAnnouncement,
            [existingCatInDestination],
            OperationDate);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        catToReassign.AdoptionAnnouncementId.ShouldBe(destinationAnnouncement.Id);
    }

    [Fact]
    public void ReassignCatToAnotherAdoptionAnnouncement_ShouldSucceed_WhenDestinationIsEmpty()
    {
        //Arrange
        PersonId personId = PersonId.Create();
        AdoptionAnnouncement sourceAnnouncement = CreateActiveAnnouncement(personId);
        AdoptionAnnouncement destinationAnnouncement = CreateActiveAnnouncement(personId);

        Cat cat = CreatePublishedCat(personId, sourceAnnouncement.Id);

        //Act
        Result result = _service.ReassignCatToAnotherAdoptionAnnouncement(
            cat,
            sourceAnnouncement,
            destinationAnnouncement,
            [],
            OperationDate);

        //Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ReassignCatToAnotherAdoptionAnnouncement_ShouldFail_WhenSourceAnnouncementIsClaimed()
    {
        //Arrange
        PersonId personId = PersonId.Create();
        AdoptionAnnouncement sourceAnnouncement = CreateActiveAnnouncement(personId);
        sourceAnnouncement.Claim(AdoptionAnnouncementFactory.CreateDefaultClaimedAt()).EnsureSuccess();

        AdoptionAnnouncement destinationAnnouncement = CreateActiveAnnouncement(personId);
        Cat cat = CreatePublishedCat(personId, sourceAnnouncement.Id);

        //Act
        Result result = _service.ReassignCatToAnotherAdoptionAnnouncement(
            cat,
            sourceAnnouncement,
            destinationAnnouncement,
            [],
            OperationDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.AdoptionAnnouncementEntity.StatusProperty
            .CannotReassignCatFromInactiveAnnouncement);
    }

    [Fact]
    public void ReassignCatToAnotherAdoptionAnnouncement_ShouldFail_WhenDestinationAnnouncementIsClaimed()
    {
        //Arrange
        PersonId personId = PersonId.Create();
        AdoptionAnnouncement sourceAnnouncement = CreateActiveAnnouncement(personId);
        AdoptionAnnouncement destinationAnnouncement = CreateActiveAnnouncement(personId);
        destinationAnnouncement.Claim(AdoptionAnnouncementFactory.CreateDefaultClaimedAt()).EnsureSuccess();

        Cat cat = CreatePublishedCat(personId, sourceAnnouncement.Id);

        //Act
        Result result = _service.ReassignCatToAnotherAdoptionAnnouncement(
            cat,
            sourceAnnouncement,
            destinationAnnouncement,
            [],
            OperationDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.AdoptionAnnouncementEntity.StatusProperty
            .CannotReassignCatToInactiveAnnouncement);
    }

    [Fact]
    public void ReassignCatToAnotherAdoptionAnnouncement_ShouldFail_WhenBothAnnouncementsAreClaimed()
    {
        //Arrange
        PersonId personId = PersonId.Create();
        AdoptionAnnouncement sourceAnnouncement = CreateActiveAnnouncement(personId);
        sourceAnnouncement.Claim(AdoptionAnnouncementFactory.CreateDefaultClaimedAt()).EnsureSuccess();

        AdoptionAnnouncement destinationAnnouncement = CreateActiveAnnouncement(personId);
        destinationAnnouncement.Claim(AdoptionAnnouncementFactory.CreateDefaultClaimedAt()).EnsureSuccess();

        Cat cat = CreatePublishedCat(personId, sourceAnnouncement.Id);

        //Act
        Result result = _service.ReassignCatToAnotherAdoptionAnnouncement(
            cat,
            sourceAnnouncement,
            destinationAnnouncement,
            [],
            OperationDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void ReassignCatToAnotherAdoptionAnnouncement_ShouldFail_WhenCatAlreadyInDestination()
    {
        //Arrange
        PersonId personId = PersonId.Create();
        AdoptionAnnouncement sourceAnnouncement = CreateActiveAnnouncement(personId);
        AdoptionAnnouncement destinationAnnouncement = CreateActiveAnnouncement(personId);

        Cat cat = CreatePublishedCat(personId, sourceAnnouncement.Id);

        // Cat is already in the destination list
        IReadOnlyCollection<Cat> destinationCats = [cat];

        //Act
        Result result = _service.ReassignCatToAnotherAdoptionAnnouncement(
            cat,
            sourceAnnouncement,
            destinationAnnouncement,
            destinationCats,
            OperationDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.Assignment.CannotReassignToSameAnnouncement(cat.Id));
    }

    [Fact]
    public void ReassignCatToAnotherAdoptionAnnouncement_ShouldFail_WhenFivPositiveMixesWithFivNegative()
    {
        //Arrange
        PersonId personId = PersonId.Create();
        InfectiousDiseaseStatus fivPositive = CreateDiseaseStatus(FivStatus.Positive, FelvStatus.Negative);
        InfectiousDiseaseStatus fivNegative = CreateDiseaseStatus(FivStatus.Negative, FelvStatus.Negative);

        AdoptionAnnouncement sourceAnnouncement = CreateActiveAnnouncement(personId);
        AdoptionAnnouncement destinationAnnouncement = CreateActiveAnnouncement(personId);

        Cat catToReassign = CreatePublishedCat(personId, sourceAnnouncement.Id, fivPositive);
        Cat existingCatInDestination = CreatePublishedCat(personId, destinationAnnouncement.Id, fivNegative);

        //Act
        Result result = _service.ReassignCatToAnotherAdoptionAnnouncement(
            catToReassign,
            sourceAnnouncement,
            destinationAnnouncement,
            [existingCatInDestination],
            OperationDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.Assignment
            .IncompatibleInfectiousDiseaseStatus(catToReassign.Id));
    }

    [Fact]
    public void ReassignCatToAnotherAdoptionAnnouncement_ShouldFail_WhenFelvPositiveMixesWithFelvNegative()
    {
        //Arrange
        PersonId personId = PersonId.Create();
        InfectiousDiseaseStatus felvPositive = CreateDiseaseStatus(FivStatus.Negative, FelvStatus.Positive);
        InfectiousDiseaseStatus felvNegative = CreateDiseaseStatus(FivStatus.Negative, FelvStatus.Negative);

        AdoptionAnnouncement sourceAnnouncement = CreateActiveAnnouncement(personId);
        AdoptionAnnouncement destinationAnnouncement = CreateActiveAnnouncement(personId);

        Cat catToReassign = CreatePublishedCat(personId, sourceAnnouncement.Id, felvPositive);
        Cat existingCatInDestination = CreatePublishedCat(personId, destinationAnnouncement.Id, felvNegative);

        //Act
        Result result = _service.ReassignCatToAnotherAdoptionAnnouncement(
            catToReassign,
            sourceAnnouncement,
            destinationAnnouncement,
            [existingCatInDestination],
            OperationDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.Assignment
            .IncompatibleInfectiousDiseaseStatus(catToReassign.Id));
    }

    [Fact]
    public void ReassignCatToAnotherAdoptionAnnouncement_ShouldSucceed_WhenNotTestedMixesWithPositive()
    {
        //Arrange - NotTested should be compatible with any status
        PersonId personId = PersonId.Create();
        InfectiousDiseaseStatus notTested = CreateDiseaseStatus(FivStatus.NotTested, FelvStatus.NotTested);
        InfectiousDiseaseStatus fivPositive = CreateDiseaseStatus(FivStatus.Positive, FelvStatus.Negative);

        AdoptionAnnouncement sourceAnnouncement = CreateActiveAnnouncement(personId);
        AdoptionAnnouncement destinationAnnouncement = CreateActiveAnnouncement(personId);

        Cat catToReassign = CreatePublishedCat(personId, sourceAnnouncement.Id, notTested);
        Cat existingCatInDestination = CreatePublishedCat(personId, destinationAnnouncement.Id, fivPositive);

        //Act
        Result result = _service.ReassignCatToAnotherAdoptionAnnouncement(
            catToReassign,
            sourceAnnouncement,
            destinationAnnouncement,
            [existingCatInDestination],
            OperationDate);

        //Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ReassignCatToAnotherAdoptionAnnouncement_ShouldFail_WhenIncompatibleWithMultipleCatsInDestination()
    {
        //Arrange
        PersonId personId = PersonId.Create();
        InfectiousDiseaseStatus fivPositive = CreateDiseaseStatus(FivStatus.Positive, FelvStatus.Negative);
        InfectiousDiseaseStatus fivNegative = CreateDiseaseStatus(FivStatus.Negative, FelvStatus.Negative);

        AdoptionAnnouncement sourceAnnouncement = CreateActiveAnnouncement(personId);
        AdoptionAnnouncement destinationAnnouncement = CreateActiveAnnouncement(personId);

        Cat catToReassign = CreatePublishedCat(personId, sourceAnnouncement.Id, fivPositive);

        Cat existingCat1 = CreatePublishedCat(personId, destinationAnnouncement.Id, fivNegative);
        Cat existingCat2 = CreatePublishedCat(personId, destinationAnnouncement.Id, fivNegative);

        //Act
        Result result = _service.ReassignCatToAnotherAdoptionAnnouncement(
            catToReassign,
            sourceAnnouncement,
            destinationAnnouncement,
            [existingCat1, existingCat2],
            OperationDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void ReassignCatToAnotherAdoptionAnnouncement_ShouldSucceed_WhenCompatibleWithMultipleCatsInDestination()
    {
        //Arrange
        PersonId personId = PersonId.Create();
        InfectiousDiseaseStatus sameStatus = CreateDiseaseStatus(FivStatus.Positive, FelvStatus.Negative);

        AdoptionAnnouncement sourceAnnouncement = CreateActiveAnnouncement(personId);
        AdoptionAnnouncement destinationAnnouncement = CreateActiveAnnouncement(personId);

        Cat catToReassign = CreatePublishedCat(personId, sourceAnnouncement.Id, sameStatus);

        Cat existingCat1 = CreatePublishedCat(personId, destinationAnnouncement.Id, sameStatus);
        Cat existingCat2 = CreatePublishedCat(personId, destinationAnnouncement.Id, sameStatus);

        //Act
        Result result = _service.ReassignCatToAnotherAdoptionAnnouncement(
            catToReassign,
            sourceAnnouncement,
            destinationAnnouncement,
            [existingCat1, existingCat2],
            OperationDate);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        catToReassign.AdoptionAnnouncementId.ShouldBe(destinationAnnouncement.Id);
    }

    private static Cat CreatePublishedCat(
        PersonId personId,
        AdoptionAnnouncementId announcementId,
        InfectiousDiseaseStatus? diseaseStatus = null)
    {
        Cat cat = diseaseStatus == null
            ? CatFactory.CreateWithThumbnail(Faker, personId: personId)
            : CreateCatWithDiseaseStatus(personId, diseaseStatus);

        cat.AssignToAdoptionAnnouncement(announcementId, OperationDate.AddDays(-1)).EnsureSuccess();
        return cat;
    }

    private static Cat CreateCatWithDiseaseStatus(PersonId personId, InfectiousDiseaseStatus diseaseStatus)
    {
        Cat cat = CatFactory.CreateRandom(Faker, personId: personId);
        cat.UpsertThumbnail();
        cat.UpdateInfectiousDiseaseStatus(diseaseStatus).EnsureSuccess();
        return cat;
    }

    private static AdoptionAnnouncement CreateActiveAnnouncement(PersonId personId)
    {
        return AdoptionAnnouncementFactory.CreateRandom(Faker, personId: personId);
    }

    private static InfectiousDiseaseStatus CreateDiseaseStatus(FivStatus fivStatus, FelvStatus felvStatus)
    {
        DateOnly currentDate = new(2025, 6, 1);
        DateOnly testDate = new(2025, 5, 1);

        Result<InfectiousDiseaseStatus> result = InfectiousDiseaseStatus.Create(
            fivStatus,
            felvStatus,
            testDate,
            currentDate);

        result.EnsureSuccess();
        return result.Value;
    }


}
