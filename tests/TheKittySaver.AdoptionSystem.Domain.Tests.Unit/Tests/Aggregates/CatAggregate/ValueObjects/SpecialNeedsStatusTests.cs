using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.CatAggregate.ValueObjects;

public sealed class SpecialNeedsStatusTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidDataProvided()
    {
        //Arrange & Act
        Result<SpecialNeedsStatus> result = SpecialNeedsStatus.Create("Requires medication", SpecialNeedsSeverityType.Moderate);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Description.ShouldBe("Requires medication");
        result.Value.SeverityType.ShouldBe(SpecialNeedsSeverityType.Moderate);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDescriptionIsEmpty()
    {
        //Arrange & Act
        Result<SpecialNeedsStatus> result = SpecialNeedsStatus.Create("   ", SpecialNeedsSeverityType.Moderate);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.SpecialNeedsProperty.DescriptionNullOrEmpty);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenSeverityTypeIsUnset()
    {
        //Arrange & Act
        Result<SpecialNeedsStatus> result = SpecialNeedsStatus.Create("Description", SpecialNeedsSeverityType.Unset);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.SpecialNeedsProperty.SeverityIsUnset);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDescriptionIsTooLong()
    {
        //Arrange
        string longDescription = new('a', SpecialNeedsStatus.MaxDescriptionLength + 1);

        //Act
        Result<SpecialNeedsStatus> result = SpecialNeedsStatus.Create(longDescription, SpecialNeedsSeverityType.Moderate);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.SpecialNeedsProperty.DescriptionLongerThanAllowed);
    }
}
