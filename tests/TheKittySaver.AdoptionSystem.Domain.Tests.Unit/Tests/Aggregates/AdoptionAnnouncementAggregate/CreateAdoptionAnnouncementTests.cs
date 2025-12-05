using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Factories;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.AdoptionAnnouncementAggregate;

public sealed class CreateAdoptionAnnouncementTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void Create_ShouldCreateAdoptionAnnouncement_WhenValidDataAreProvided()
    {
        //Arrange & Act
        AdoptionAnnouncement announcement = AdoptionAnnouncementFactory.CreateRandom(Faker);

        //Assert
        announcement.ShouldNotBeNull();
        announcement.Id.ShouldNotBe(AdoptionAnnouncementId.Empty);
        announcement.PersonId.ShouldNotBe(PersonId.Empty);
        announcement.Description.ShouldNotBeNull();
        announcement.Address.ShouldNotBeNull();
        announcement.Email.ShouldNotBeNull();
        announcement.PhoneNumber.ShouldNotBeNull();
        announcement.CreatedAt.ShouldNotBeNull();
        announcement.Status.ShouldBe(AnnouncementStatusType.Active);
        announcement.MergeLogs.Count.ShouldBe(0);
        announcement.ClaimedAt.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldThrow_WhenEmptyPersonIdIsProvided()
    {
        //Arrange & Act
        Func<AdoptionAnnouncement> announcementCreation = () => AdoptionAnnouncementFactory.CreateRandom(
            Faker, replacePersonIdWithEmpty: true);

        //Assert
        announcementCreation.ShouldThrow<ArgumentException>()
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(AdoptionAnnouncement.PersonId).ToLowerInvariant());
    }

    [Fact]
    public void Create_ShouldThrow_WhenNullDescriptionMaybeIsProvided()
    {
        //Arrange & Act
        Func<AdoptionAnnouncement> announcementCreation = () => AdoptionAnnouncementFactory.CreateRandom(
            Faker, replaceDescriptionWithNull: true);

        //Assert
        announcementCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(AdoptionAnnouncement.Description).ToLowerInvariant());
    }

    [Fact]
    public void Create_ShouldThrow_WhenNullAddressIsProvided()
    {
        //Arrange & Act
        Func<AdoptionAnnouncement> announcementCreation = () => AdoptionAnnouncementFactory.CreateRandom(
            Faker, replaceAddressWithNull: true);

        //Assert
        announcementCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(AdoptionAnnouncement.Address).ToLowerInvariant());
    }

    [Fact]
    public void Create_ShouldThrow_WhenNullEmailIsProvided()
    {
        //Arrange & Act
        Func<AdoptionAnnouncement> announcementCreation = () => AdoptionAnnouncementFactory.CreateRandom(
            Faker, replaceEmailWithNull: true);

        //Assert
        announcementCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(AdoptionAnnouncement.Email).ToLowerInvariant());
    }

    [Fact]
    public void Create_ShouldThrow_WhenNullPhoneNumberIsProvided()
    {
        //Arrange & Act
        Func<AdoptionAnnouncement> announcementCreation = () => AdoptionAnnouncementFactory.CreateRandom(
            Faker, replacePhoneNumberWithNull: true);

        //Assert
        announcementCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(AdoptionAnnouncement.PhoneNumber).ToLowerInvariant());
    }

    [Fact]
    public void Create_ShouldThrow_WhenNullCreatedAtIsProvided()
    {
        //Arrange & Act
        Func<AdoptionAnnouncement> announcementCreation = () => AdoptionAnnouncementFactory.CreateRandom(
            Faker, replaceCreatedAtWithNull: true);

        //Assert
        announcementCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(AdoptionAnnouncement.CreatedAt).ToLowerInvariant());
    }
}
