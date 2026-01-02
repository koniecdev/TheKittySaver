using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.Services.CatAdoptionAnnouncementServices;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Extensions;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Factories;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Services;

public sealed class CatAdoptionAnnouncementAssignmentServiceTests
{
    private static readonly Faker Faker = new();
    private static readonly DateTimeOffset OperationDate =
        new(2025, 6, 1, 12, 0, 0, TimeSpan.Zero);
    private readonly CatAdoptionAnnouncementAssignmentService _service = new();

    [Fact]
    public void AssignCatToAdoptionAnnouncement_ShouldSucceed_WhenAllConditionsAreMet()
    {
        //Arrange
        PersonId personId = PersonId.Create();
        Cat cat = CreateDraftCatWithThumbnail(personId);
        AdoptionAnnouncement announcement = CreateActiveAnnouncement(personId);
        IReadOnlyCollection<Cat> emptyCatList = [];

        //Act
        Result result = _service.AssignCatToAdoptionAnnouncement(
            cat,
            announcement,
            emptyCatList,
            OperationDate);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.Status.ShouldBe(CatStatusType.Published);
        cat.AdoptionAnnouncementId.ShouldBe(announcement.Id);
        cat.PublishedAt.ShouldNotBeNull();
    }

    [Fact]
    public void AssignCatToAdoptionAnnouncement_ShouldSucceed_WhenCatHasCompatibleDiseaseStatus()
    {
        //Arrange
        PersonId personId = PersonId.Create();
        InfectiousDiseaseStatus fivPositive = CreateDiseaseStatus(FivStatus.Positive, FelvStatus.Negative);

        Cat existingCat = CreateDraftCatWithThumbnail(personId, fivPositive);
        existingCat.AssignToAdoptionAnnouncement(AdoptionAnnouncementId.New(), OperationDate.AddDays(-1))
            .EnsureSuccess();

        Cat newCat = CreateDraftCatWithThumbnail(personId, fivPositive);
        AdoptionAnnouncement announcement = CreateActiveAnnouncement(personId);

        //Act
        Result result = _service.AssignCatToAdoptionAnnouncement(
            newCat,
            announcement,
            [existingCat],
            OperationDate);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        newCat.Status.ShouldBe(CatStatusType.Published);
    }

    [Fact]
    public void AssignCatToAdoptionAnnouncement_ShouldSucceed_WhenNotTestedCatMixesWithPositiveCat()
    {
        //Arrange - NotTested should be compatible with any status
        PersonId personId = PersonId.Create();
        InfectiousDiseaseStatus fivPositive = CreateDiseaseStatus(FivStatus.Positive, FelvStatus.Negative);
        InfectiousDiseaseStatus notTested = CreateDiseaseStatus(FivStatus.NotTested, FelvStatus.NotTested);

        Cat existingCat = CreateDraftCatWithThumbnail(personId, fivPositive);
        existingCat.AssignToAdoptionAnnouncement(AdoptionAnnouncementId.New(), OperationDate.AddDays(-1))
            .EnsureSuccess();

        Cat newCat = CreateDraftCatWithThumbnail(personId, notTested);
        AdoptionAnnouncement announcement = CreateActiveAnnouncement(personId);

        //Act
        Result result = _service.AssignCatToAdoptionAnnouncement(
            newCat,
            announcement,
            [existingCat],
            OperationDate);

        //Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void AssignCatToAdoptionAnnouncement_ShouldFail_WhenPersonIdMismatch()
    {
        //Arrange
        PersonId catPersonId = PersonId.Create();
        PersonId announcementPersonId = PersonId.Create();

        Cat cat = CreateDraftCatWithThumbnail(catPersonId);
        AdoptionAnnouncement announcement = CreateActiveAnnouncement(announcementPersonId);

        //Act
        Result result = _service.AssignCatToAdoptionAnnouncement(
            cat,
            announcement,
            [],
            OperationDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatAdoptionAnnouncementService.PersonIdMismatch(
            cat.Id,
            catPersonId,
            announcement.Id,
            announcementPersonId));
    }

    [Fact]
    public void AssignCatToAdoptionAnnouncement_ShouldFail_WhenCatIsAlreadyPublished()
    {
        //Arrange
        PersonId personId = PersonId.Create();
        Cat cat = CreateDraftCatWithThumbnail(personId);
        cat.AssignToAdoptionAnnouncement(AdoptionAnnouncementId.New(), OperationDate.AddDays(-1))
            .EnsureSuccess(); // Cat is now Published

        AdoptionAnnouncement announcement = CreateActiveAnnouncement(personId);

        //Act
        Result result = _service.AssignCatToAdoptionAnnouncement(
            cat,
            announcement,
            [],
            OperationDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.StatusProperty.MustBeDraftForAssignment(cat.Id));
    }

    [Fact]
    public void AssignCatToAdoptionAnnouncement_ShouldFail_WhenCatIsClaimed()
    {
        //Arrange
        PersonId personId = PersonId.Create();
        Cat cat = CreateDraftCatWithThumbnail(personId);
        cat.AssignToAdoptionAnnouncement(AdoptionAnnouncementId.New(), OperationDate.AddDays(-2))
            .EnsureSuccess();
        cat.Claim(AdoptionAnnouncementFactory.CreateDefaultClaimedAt()).EnsureSuccess(); // Cat is now Claimed

        AdoptionAnnouncement announcement = CreateActiveAnnouncement(personId);

        //Act
        Result result = _service.AssignCatToAdoptionAnnouncement(
            cat,
            announcement,
            [],
            OperationDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.StatusProperty.MustBeDraftForAssignment(cat.Id));
    }

    [Fact]
    public void AssignCatToAdoptionAnnouncement_ShouldFail_WhenAnnouncementIsClaimed()
    {
        //Arrange
        PersonId personId = PersonId.Create();
        Cat cat = CreateDraftCatWithThumbnail(personId);
        AdoptionAnnouncement announcement = CreateActiveAnnouncement(personId);
        announcement.Claim(AdoptionAnnouncementFactory.CreateDefaultClaimedAt()).EnsureSuccess();

        //Act
        Result result = _service.AssignCatToAdoptionAnnouncement(
            cat,
            announcement,
            [],
            OperationDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.AdoptionAnnouncementEntity.StatusProperty.UnavailableForAssigning);
    }

    [Fact]
    public void AssignCatToAdoptionAnnouncement_ShouldFail_WhenCatIsAlreadyInTheList()
    {
        //Arrange
        PersonId personId = PersonId.Create();
        Cat cat = CreateDraftCatWithThumbnail(personId);

        // Create another cat that's already published to simulate cats in announcement
        Cat otherCat = CreateDraftCatWithThumbnail(personId);
        otherCat.AssignToAdoptionAnnouncement(AdoptionAnnouncementId.New(), OperationDate.AddDays(-1))
            .EnsureSuccess();

        AdoptionAnnouncement announcement = CreateActiveAnnouncement(personId);

        // Incorrectly include the cat we're trying to assign in the already-assigned list
        IReadOnlyCollection<Cat> catsAlreadyAssigned = [cat, otherCat];

        //Act
        Result result = _service.AssignCatToAdoptionAnnouncement(
            cat,
            announcement,
            catsAlreadyAssigned,
            OperationDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.Assignment.AlreadyAssignedToAnnouncement(cat.Id));
    }

    [Fact]
    public void AssignCatToAdoptionAnnouncement_ShouldFail_WhenFivPositiveMixesWithFivNegative()
    {
        //Arrange - FIV+ cat cannot mix with FIV- cat
        PersonId personId = PersonId.Create();
        InfectiousDiseaseStatus fivPositive = CreateDiseaseStatus(FivStatus.Positive, FelvStatus.Negative);
        InfectiousDiseaseStatus fivNegative = CreateDiseaseStatus(FivStatus.Negative, FelvStatus.Negative);

        Cat existingCat = CreateDraftCatWithThumbnail(personId, fivPositive);
        existingCat.AssignToAdoptionAnnouncement(AdoptionAnnouncementId.New(), OperationDate.AddDays(-1))
            .EnsureSuccess();

        Cat newCat = CreateDraftCatWithThumbnail(personId, fivNegative);
        AdoptionAnnouncement announcement = CreateActiveAnnouncement(personId);

        //Act
        Result result = _service.AssignCatToAdoptionAnnouncement(
            newCat,
            announcement,
            [existingCat],
            OperationDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatAdoptionAnnouncementService
            .InfectiousDiseaseConflict(newCat.Id, announcement.Id));
    }

    [Fact]
    public void AssignCatToAdoptionAnnouncement_ShouldFail_WhenFelvPositiveMixesWithFelvNegative()
    {
        //Arrange - FeLV+ cat cannot mix with FeLV- cat
        PersonId personId = PersonId.Create();
        InfectiousDiseaseStatus felvPositive = CreateDiseaseStatus(FivStatus.Negative, FelvStatus.Positive);
        InfectiousDiseaseStatus felvNegative = CreateDiseaseStatus(FivStatus.Negative, FelvStatus.Negative);

        Cat existingCat = CreateDraftCatWithThumbnail(personId, felvPositive);
        existingCat.AssignToAdoptionAnnouncement(AdoptionAnnouncementId.New(), OperationDate.AddDays(-1))
            .EnsureSuccess();

        Cat newCat = CreateDraftCatWithThumbnail(personId, felvNegative);
        AdoptionAnnouncement announcement = CreateActiveAnnouncement(personId);

        //Act
        Result result = _service.AssignCatToAdoptionAnnouncement(
            newCat,
            announcement,
            [existingCat],
            OperationDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatAdoptionAnnouncementService
            .InfectiousDiseaseConflict(newCat.Id, announcement.Id));
    }

    [Fact]
    public void AssignCatToAdoptionAnnouncement_ShouldFail_WhenBothDiseasesAreIncompatible()
    {
        //Arrange - FIV+/FeLV+ cat cannot mix with FIV-/FeLV- cat
        PersonId personId = PersonId.Create();
        InfectiousDiseaseStatus bothPositive = CreateDiseaseStatus(FivStatus.Positive, FelvStatus.Positive);
        InfectiousDiseaseStatus bothNegative = CreateDiseaseStatus(FivStatus.Negative, FelvStatus.Negative);

        Cat existingCat = CreateDraftCatWithThumbnail(personId, bothPositive);
        existingCat.AssignToAdoptionAnnouncement(AdoptionAnnouncementId.New(), OperationDate.AddDays(-1))
            .EnsureSuccess();

        Cat newCat = CreateDraftCatWithThumbnail(personId, bothNegative);
        AdoptionAnnouncement announcement = CreateActiveAnnouncement(personId);

        //Act
        Result result = _service.AssignCatToAdoptionAnnouncement(
            newCat,
            announcement,
            [existingCat],
            OperationDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void AssignCatToAdoptionAnnouncement_ShouldFail_WhenOneOfTwoDiseasesIsIncompatible()
    {
        //Arrange - FIV compatible but FeLV incompatible
        PersonId personId = PersonId.Create();
        InfectiousDiseaseStatus status1 = CreateDiseaseStatus(FivStatus.Positive, FelvStatus.Positive);
        InfectiousDiseaseStatus status2 = CreateDiseaseStatus(FivStatus.Positive, FelvStatus.Negative);

        Cat existingCat = CreateDraftCatWithThumbnail(personId, status1);
        existingCat.AssignToAdoptionAnnouncement(AdoptionAnnouncementId.New(), OperationDate.AddDays(-1))
            .EnsureSuccess();

        Cat newCat = CreateDraftCatWithThumbnail(personId, status2);
        AdoptionAnnouncement announcement = CreateActiveAnnouncement(personId);

        //Act
        Result result = _service.AssignCatToAdoptionAnnouncement(
            newCat,
            announcement,
            [existingCat],
            OperationDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void AssignCatToAdoptionAnnouncement_ShouldSucceed_WhenAllCatsHaveSameDiseaseStatus()
    {
        //Arrange
        PersonId personId = PersonId.Create();
        InfectiousDiseaseStatus sameStatus = CreateDiseaseStatus(FivStatus.Positive, FelvStatus.Negative);

        Cat existingCat1 = CreateDraftCatWithThumbnail(personId, sameStatus);
        existingCat1.AssignToAdoptionAnnouncement(AdoptionAnnouncementId.New(), OperationDate.AddDays(-2))
            .EnsureSuccess();

        Cat existingCat2 = CreateDraftCatWithThumbnail(personId, sameStatus);
        existingCat2.AssignToAdoptionAnnouncement(AdoptionAnnouncementId.New(), OperationDate.AddDays(-1))
            .EnsureSuccess();

        Cat newCat = CreateDraftCatWithThumbnail(personId, sameStatus);
        AdoptionAnnouncement announcement = CreateActiveAnnouncement(personId);

        //Act
        Result result = _service.AssignCatToAdoptionAnnouncement(
            newCat,
            announcement,
            [existingCat1, existingCat2],
            OperationDate);

        //Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void AssignCatToAdoptionAnnouncement_ShouldFail_WhenNewCatIncompatibleWithMultipleCats()
    {
        //Arrange - Testing with multiple existing cats
        PersonId personId = PersonId.Create();
        InfectiousDiseaseStatus fivPositive = CreateDiseaseStatus(FivStatus.Positive, FelvStatus.Negative);
        InfectiousDiseaseStatus fivNegative = CreateDiseaseStatus(FivStatus.Negative, FelvStatus.Negative);

        Cat existingCat1 = CreateDraftCatWithThumbnail(personId, fivPositive);
        existingCat1.AssignToAdoptionAnnouncement(AdoptionAnnouncementId.New(), OperationDate.AddDays(-2))
            .EnsureSuccess();

        Cat existingCat2 = CreateDraftCatWithThumbnail(personId, fivPositive);
        existingCat2.AssignToAdoptionAnnouncement(AdoptionAnnouncementId.New(), OperationDate.AddDays(-1))
            .EnsureSuccess();

        Cat newCat = CreateDraftCatWithThumbnail(personId, fivNegative);
        AdoptionAnnouncement announcement = CreateActiveAnnouncement(personId);

        //Act
        Result result = _service.AssignCatToAdoptionAnnouncement(
            newCat,
            announcement,
            [existingCat1, existingCat2],
            OperationDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatAdoptionAnnouncementService
            .InfectiousDiseaseConflict(newCat.Id, announcement.Id));
    }

    [Fact]
    public void AssignCatToAdoptionAnnouncement_ShouldSucceed_WhenNotTestedMixesWithNegativeCats()
    {
        //Arrange - NotTested is compatible with Negative
        PersonId personId = PersonId.Create();
        InfectiousDiseaseStatus negative = CreateDiseaseStatus(FivStatus.Negative, FelvStatus.Negative);
        InfectiousDiseaseStatus notTested = CreateDiseaseStatus(FivStatus.NotTested, FelvStatus.NotTested);

        Cat existingCat = CreateDraftCatWithThumbnail(personId, negative);
        existingCat.AssignToAdoptionAnnouncement(AdoptionAnnouncementId.New(), OperationDate.AddDays(-1))
            .EnsureSuccess();

        Cat newCat = CreateDraftCatWithThumbnail(personId, notTested);
        AdoptionAnnouncement announcement = CreateActiveAnnouncement(personId);

        //Act
        Result result = _service.AssignCatToAdoptionAnnouncement(
            newCat,
            announcement,
            [existingCat],
            OperationDate);

        //Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void AssignCatToAdoptionAnnouncement_ShouldSucceed_WhenAllCatsAreNotTested()
    {
        //Arrange - All NotTested cats are compatible
        PersonId personId = PersonId.Create();
        InfectiousDiseaseStatus notTested = CreateDiseaseStatus(FivStatus.NotTested, FelvStatus.NotTested);

        Cat existingCat1 = CreateDraftCatWithThumbnail(personId, notTested);
        existingCat1.AssignToAdoptionAnnouncement(AdoptionAnnouncementId.New(), OperationDate.AddDays(-2))
            .EnsureSuccess();

        Cat existingCat2 = CreateDraftCatWithThumbnail(personId, notTested);
        existingCat2.AssignToAdoptionAnnouncement(AdoptionAnnouncementId.New(), OperationDate.AddDays(-1))
            .EnsureSuccess();

        Cat newCat = CreateDraftCatWithThumbnail(personId, notTested);
        AdoptionAnnouncement announcement = CreateActiveAnnouncement(personId);

        //Act
        Result result = _service.AssignCatToAdoptionAnnouncement(
            newCat,
            announcement,
            [existingCat1, existingCat2],
            OperationDate);

        //Assert
        result.IsSuccess.ShouldBeTrue();
    }

    private static Cat CreateDraftCatWithThumbnail(
        PersonId personId,
        InfectiousDiseaseStatus? diseaseStatus = null)
    {
        if (diseaseStatus is null)
        {
            return CatFactory.CreateWithThumbnail(Faker, personId: personId);
        }

        Cat cat = CatFactory.CreateRandom(
            Faker,
            personId: personId);
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
        DateOnly? testDate = fivStatus is FivStatus.NotTested && felvStatus is FelvStatus.NotTested
        ? null
        : new DateOnly(2025, 5, 1);

        Result<InfectiousDiseaseStatus> result = InfectiousDiseaseStatus.Create(
            fivStatus,
            felvStatus,
            testDate,
            currentDate);

        result.EnsureSuccess();
        return result.Value;
    }


}
