using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.SharedValueObjects.PhoneNumbers;

public sealed class PhoneNumberTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void CreateUnsafe_ShouldCreatePhoneNumber_WhenValidValueIsProvided()
    {
        //Arrange
        string validPhoneNumber = Faker.Phone.PhoneNumber("+48#########");

        //Act
        PhoneNumber phoneNumber = PhoneNumber.CreateUnsafe(validPhoneNumber);

        //Assert
        phoneNumber.ShouldNotBeNull();
        phoneNumber.Value.ShouldBe(validPhoneNumber);
    }

    [Fact]
    public void CreateUnsafe_ShouldTrimValue_WhenValueHasWhitespaceAround()
    {
        //Arrange
        string phone = Faker.Phone.PhoneNumber("+48#########");
        string phoneNumberWithWhitespace = $"  {phone}  ";

        //Act
        PhoneNumber phoneNumber = PhoneNumber.CreateUnsafe(phoneNumberWithWhitespace);

        //Assert
        phoneNumber.Value.ShouldBe(phone);
    }

    [Fact]
    public void CreateUnsafe_ShouldThrow_WhenNullValueIsProvided()
    {
        //Arrange & Act
        Action createAction = () => PhoneNumber.CreateUnsafe(null!);

        //Assert
        createAction.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldBe(nameof(PhoneNumber.Value).ToLower());
    }

    [Fact]
    public void ToString_ShouldReturnValue_WhenValidPhoneNumberIsProvided()
    {
        //Arrange
        string validPhoneNumber = Faker.Phone.PhoneNumber("+48#########");
        PhoneNumber phoneNumber = PhoneNumber.CreateUnsafe(validPhoneNumber);

        //Act
        string toStringResult = phoneNumber.ToString();

        //Assert
        toStringResult.ShouldBe(validPhoneNumber);
    }

    [Fact]
    public void Equality_ShouldReturnTrue_WhenValuesAreEqual()
    {
        //Arrange
        string phone = Faker.Phone.PhoneNumber("+48#########");
        PhoneNumber phoneNumber1 = PhoneNumber.CreateUnsafe(phone);
        PhoneNumber phoneNumber2 = PhoneNumber.CreateUnsafe(phone);

        //Act & Assert
        phoneNumber1.ShouldBe(phoneNumber2);
    }

    [Fact]
    public void Equality_ShouldReturnFalse_WhenValuesAreDifferent()
    {
        //Arrange
        PhoneNumber phoneNumber1 = PhoneNumber.CreateUnsafe(Faker.Phone.PhoneNumber("+48#########"));
        PhoneNumber phoneNumber2 = PhoneNumber.CreateUnsafe(Faker.Phone.PhoneNumber("+49#########"));

        //Act & Assert
        phoneNumber1.ShouldNotBe(phoneNumber2);
    }
}
