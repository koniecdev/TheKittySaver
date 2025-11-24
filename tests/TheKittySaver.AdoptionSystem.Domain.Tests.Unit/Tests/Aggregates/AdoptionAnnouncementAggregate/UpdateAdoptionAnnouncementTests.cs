using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Factories;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.AdoptionAnnouncementAggregate;

public sealed class UpdateAdoptionAnnouncementTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void UpdateDescription_ShouldUpdateDescription_WhenValidDescriptionIsProvided()
    {
        //Arrange
        AdoptionAnnouncement announcement = AdoptionAnnouncementFactory.CreateRandom(Faker);
        AdoptionAnnouncementDescription newDescription = AdoptionAnnouncementFactory.CreateRandomDescription(Faker);
        Maybe<AdoptionAnnouncementDescription> maybeDescription = Maybe<AdoptionAnnouncementDescription>.From(newDescription);

        //Act
        Result result = announcement.UpdateDescription(maybeDescription);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        announcement.Description.ShouldBe(newDescription);
    }

    [Fact]
    public void UpdateDescription_ShouldSetDescriptionToNull_WhenNoneIsProvided()
    {
        //Arrange
        AdoptionAnnouncement announcement = AdoptionAnnouncementFactory.CreateRandom(Faker);
        Maybe<AdoptionAnnouncementDescription> maybeDescription = Maybe<AdoptionAnnouncementDescription>.None;

        //Act
        Result result = announcement.UpdateDescription(maybeDescription);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        announcement.Description.ShouldBeNull();
    }

    [Fact]
    public void UpdateDescription_ShouldThrow_WhenNullMaybeIsProvided()
    {
        //Arrange
        AdoptionAnnouncement announcement = AdoptionAnnouncementFactory.CreateRandom(Faker);

        //Act
        Action updateDescription = () => announcement.UpdateDescription(null!);

        //Assert
        updateDescription.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldContain("description");
    }

    [Fact]
    public void UpdateAddress_ShouldUpdateAddress_WhenValidAddressIsProvided()
    {
        //Arrange
        AdoptionAnnouncement announcement = AdoptionAnnouncementFactory.CreateRandom(Faker);
        AdoptionAnnouncementAddress newAddress = AdoptionAnnouncementFactory.CreateRandomAddress(Faker);

        //Act
        Result result = announcement.UpdateAddress(newAddress);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        announcement.Address.ShouldBe(newAddress);
    }

    [Fact]
    public void UpdateAddress_ShouldThrow_WhenNullAddressIsProvided()
    {
        //Arrange
        AdoptionAnnouncement announcement = AdoptionAnnouncementFactory.CreateRandom(Faker);

        //Act
        Action updateAddress = () => announcement.UpdateAddress(null!);

        //Assert
        updateAddress.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldContain("address");
    }

    [Fact]
    public void UpdateAddress_ShouldReturnFailure_WhenAnnouncementIsClaimed()
    {
        //Arrange
        AdoptionAnnouncement announcement = AdoptionAnnouncementFactory.CreateRandom(Faker);
        ClaimedAt claimedAt = AdoptionAnnouncementFactory.CreateDefaultClaimedAt();
        announcement.Claim(claimedAt);
        AdoptionAnnouncementAddress newAddress = AdoptionAnnouncementFactory.CreateRandomAddress(Faker);

        //Act
        Result result = announcement.UpdateAddress(newAddress);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.AdoptionAnnouncementErrors.StatusProperty.CanOnlyUpdateWhenActive);
    }

    [Fact]
    public void UpdateEmail_ShouldUpdateEmail_WhenValidEmailIsProvided()
    {
        //Arrange
        AdoptionAnnouncement announcement = AdoptionAnnouncementFactory.CreateRandom(Faker);
        Email newEmail = AdoptionAnnouncementFactory.CreateRandomEmail(Faker);

        //Act
        Result result = announcement.UpdateEmail(newEmail);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        announcement.Email.ShouldBe(newEmail);
    }

    [Fact]
    public void UpdateEmail_ShouldThrow_WhenNullEmailIsProvided()
    {
        //Arrange
        AdoptionAnnouncement announcement = AdoptionAnnouncementFactory.CreateRandom(Faker);

        //Act
        Action updateEmail = () => announcement.UpdateEmail(null!);

        //Assert
        updateEmail.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldContain("email");
    }

    [Fact]
    public void UpdateEmail_ShouldReturnFailure_WhenAnnouncementIsClaimed()
    {
        //Arrange
        AdoptionAnnouncement announcement = AdoptionAnnouncementFactory.CreateRandom(Faker);
        ClaimedAt claimedAt = AdoptionAnnouncementFactory.CreateDefaultClaimedAt();
        announcement.Claim(claimedAt);
        Email newEmail = AdoptionAnnouncementFactory.CreateRandomEmail(Faker);

        //Act
        Result result = announcement.UpdateEmail(newEmail);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.AdoptionAnnouncementErrors.StatusProperty.CanOnlyUpdateWhenActive);
    }

    [Fact]
    public void UpdatePhoneNumber_ShouldUpdatePhoneNumber_WhenValidPhoneNumberIsProvided()
    {
        //Arrange
        AdoptionAnnouncement announcement = AdoptionAnnouncementFactory.CreateRandom(Faker);
        PhoneNumber newPhoneNumber = AdoptionAnnouncementFactory.CreateRandomPhoneNumber(Faker);

        //Act
        Result result = announcement.UpdatePhoneNumber(newPhoneNumber);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        announcement.PhoneNumber.ShouldBe(newPhoneNumber);
    }

    [Fact]
    public void UpdatePhoneNumber_ShouldThrow_WhenNullPhoneNumberIsProvided()
    {
        //Arrange
        AdoptionAnnouncement announcement = AdoptionAnnouncementFactory.CreateRandom(Faker);

        //Act
        Action updatePhoneNumber = () => announcement.UpdatePhoneNumber(null!);

        //Assert
        updatePhoneNumber.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldContain("phonenumber");
    }

    [Fact]
    public void UpdatePhoneNumber_ShouldReturnFailure_WhenAnnouncementIsClaimed()
    {
        //Arrange
        AdoptionAnnouncement announcement = AdoptionAnnouncementFactory.CreateRandom(Faker);
        ClaimedAt claimedAt = AdoptionAnnouncementFactory.CreateDefaultClaimedAt();
        announcement.Claim(claimedAt);
        PhoneNumber newPhoneNumber = AdoptionAnnouncementFactory.CreateRandomPhoneNumber(Faker);

        //Act
        Result result = announcement.UpdatePhoneNumber(newPhoneNumber);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.AdoptionAnnouncementErrors.StatusProperty.CanOnlyUpdateWhenActive);
    }
}
