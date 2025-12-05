using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.CatAggregate.ValueObjects;

public sealed class CatAgeTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidValueProvided()
    {
        //Arrange & Act
        Result<CatAge> result = CatAge.Create(5);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(5);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValueBelowMinimum()
    {
        //Arrange & Act
        Result<CatAge> result = CatAge.Create(-1);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.AgeProperty.BelowMinimalAllowedValue(-1, CatAge.MinimumAllowedValue));
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValueAboveMaximum()
    {
        //Arrange & Act
        Result<CatAge> result = CatAge.Create(50);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.AgeProperty.AboveMaximumAllowedValue(50, CatAge.MaximumAllowedValue));
    }
}
