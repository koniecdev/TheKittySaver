using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.SharedValueObjects;

public sealed class CreatedAtTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidDateIsProvided()
    {
        //Arrange
        DateTimeOffset validDate = Faker.Date.FutureOffset(1, CreatedAt.MinimumAllowedValue);

        //Act
        Result<CreatedAt> result = CreatedAt.Create(validDate);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(validDate);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenMinimumAllowedDateIsProvided()
    {
        //Arrange
        DateTimeOffset minimumDate = CreatedAt.MinimumAllowedValue;

        //Act
        Result<CreatedAt> result = CreatedAt.Create(minimumDate);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(minimumDate);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenCurrentDateIsProvided()
    {
        //Arrange
        DateTimeOffset currentDate = DateTimeOffset.UtcNow;

        //Act
        Result<CreatedAt> result = CreatedAt.Create(currentDate);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(currentDate);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDateIsBeforeMinimumAllowed()
    {
        //Arrange
        DateTimeOffset invalidDate = CreatedAt.MinimumAllowedValue.AddSeconds(-1);

        //Act
        Result<CreatedAt> result = CreatedAt.Create(invalidDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenVeryOldDateIsProvided()
    {
        //Arrange
        DateTimeOffset oldDate = Faker.Date.PastOffset(10, CreatedAt.MinimumAllowedValue.AddYears(-1));

        //Act
        Result<CreatedAt> result = CreatedAt.Create(oldDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void ToString_ShouldReturnIso8601Format()
    {
        //Arrange
        DateTimeOffset validDate = Faker.Date.FutureOffset(1, CreatedAt.MinimumAllowedValue);
        Result<CreatedAt> result = CreatedAt.Create(validDate);

        //Act
        string toStringResult = result.Value.ToString();

        //Assert
        toStringResult.ShouldBe(validDate.ToString("O"));
    }

    [Fact]
    public void Equality_ShouldReturnTrue_WhenValuesAreEqual()
    {
        //Arrange
        DateTimeOffset date = Faker.Date.FutureOffset(1, CreatedAt.MinimumAllowedValue);
        Result<CreatedAt> result1 = CreatedAt.Create(date);
        Result<CreatedAt> result2 = CreatedAt.Create(date);

        //Act & Assert
        result1.Value.ShouldBe(result2.Value);
    }

    [Fact]
    public void Equality_ShouldReturnFalse_WhenValuesAreDifferent()
    {
        //Arrange
        DateTimeOffset date1 = Faker.Date.FutureOffset(1, CreatedAt.MinimumAllowedValue);
        DateTimeOffset date2 = date1.AddDays(1);
        Result<CreatedAt> result1 = CreatedAt.Create(date1);
        Result<CreatedAt> result2 = CreatedAt.Create(date2);

        //Act & Assert
        result1.Value.ShouldNotBe(result2.Value);
    }
}
