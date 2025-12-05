using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.CatAggregate.ValueObjects;

public sealed class CatGalleryItemDisplayOrderTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidValueProvided()
    {
        //Arrange & Act
        Result<CatGalleryItemDisplayOrder> result = CatGalleryItemDisplayOrder.Create(5, Cat.MaximumGalleryItemsCount);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(5);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValueBelowMinimum()
    {
        //Arrange & Act
        Result<CatGalleryItemDisplayOrder> result = CatGalleryItemDisplayOrder.Create(-1, Cat.MaximumGalleryItemsCount);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatGalleryItemEntity.DisplayOrderProperty.BelowMinimum(-1, CatGalleryItemDisplayOrder.MinValue));
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValueAboveOrEqualMaximum()
    {
        //Arrange & Act
        Result<CatGalleryItemDisplayOrder> result = CatGalleryItemDisplayOrder.Create(20, Cat.MaximumGalleryItemsCount);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatGalleryItemEntity.DisplayOrderProperty.AboveOrEqualMaximum(20, Cat.MaximumGalleryItemsCount));
    }
}
