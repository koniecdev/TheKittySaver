using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.SharedValueObjects;

public sealed class ArchivedAtTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidValueProvided()
    {
        //Arrange
        DateTimeOffset validValue = new(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);

        //Act
        Result<ArchivedAt> result = ArchivedAt.Create(validValue);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(validValue);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValueIsBeforeMinimum()
    {
        //Arrange
        DateTimeOffset pastValue = ArchivedAt.MinimumAllowedValue.AddDays(-1);

        //Act
        Result<ArchivedAt> result = ArchivedAt.Create(pastValue);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.ArchivedAtValueObject.CannotBeInThePast);
    }
}
