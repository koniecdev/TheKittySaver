using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Extensions;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Factories;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.CatAggregate;

public sealed class CatGalleryManagementTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void AddGalleryItem_ShouldAddItem_WhenGalleryIsNotFull()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);

        //Act
        Result<CatGalleryItemId> result = cat.AddGalleryItem();

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.GetGalleryItems().Count.ShouldBe(1);
        cat.GetGalleryItems()[0].DisplayOrder.Value.ShouldBe(0);
    }

    [Fact]
    public void AddGalleryItem_ShouldAddMultipleItems_WithCorrectDisplayOrder()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);

        //Act
        cat.AddGalleryItem();
        cat.AddGalleryItem();
        cat.AddGalleryItem();

        //Assert
        cat.GetGalleryItems().Count.ShouldBe(3);
        cat.GetGalleryItems()[0].DisplayOrder.Value.ShouldBe(0);
        cat.GetGalleryItems()[1].DisplayOrder.Value.ShouldBe(1);
        cat.GetGalleryItems()[2].DisplayOrder.Value.ShouldBe(2);
    }

    [Fact]
    public void AddGalleryItem_ShouldAddItem_WhenGalleryHasExactlyMaximumCount()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        for (int i = 0; i < Cat.MaximumGalleryItemsCount - 1; i++)
        {
            cat.AddGalleryItem();
        }

        //Act
        Result<CatGalleryItemId> result = cat.AddGalleryItem();

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.GetGalleryItems().Count.ShouldBe(Cat.MaximumGalleryItemsCount);
    }

    [Fact]
    public void AddGalleryItem_ShouldReturnFailure_WhenGalleryIsFull()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        for (int i = 0; i < Cat.MaximumGalleryItemsCount; i++)
        {
            cat.AddGalleryItem();
        }

        //Act
        Result<CatGalleryItemId> result = cat.AddGalleryItem();

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.GalleryIsFull);
    }

    [Fact]
    public void RemoveGalleryItem_ShouldRemoveItem_WhenItemExists()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        Result<CatGalleryItemId> addResult = cat.AddGalleryItem();
        CatGalleryItemId galleryItemId = addResult.Value;

        //Act
        Result result = cat.RemoveGalleryItem(galleryItemId);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.GetGalleryItems().Count.ShouldBe(0);
    }

    [Fact]
    public void RemoveGalleryItem_ShouldReorderRemainingItems_WhenMiddleItemIsRemoved()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        cat.AddGalleryItem();
        Result<CatGalleryItemId> secondItemResult = cat.AddGalleryItem();
        cat.AddGalleryItem();
        CatGalleryItemId secondItemId = secondItemResult.Value;

        //Act
        Result result = cat.RemoveGalleryItem(secondItemId);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.GetGalleryItems().Count.ShouldBe(2);
        cat.GetGalleryItems()[0].DisplayOrder.Value.ShouldBe(0);
        cat.GetGalleryItems()[1].DisplayOrder.Value.ShouldBe(1);
    }

    [Fact]
    public void RemoveGalleryItem_ShouldReorderRemainingItems_WhenFirstItemIsRemoved()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        Result<CatGalleryItemId> firstItemResult = cat.AddGalleryItem();
        cat.AddGalleryItem();
        cat.AddGalleryItem();
        CatGalleryItemId firstItemId = firstItemResult.Value;

        //Act
        Result result = cat.RemoveGalleryItem(firstItemId);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.GetGalleryItems().Count.ShouldBe(2);
        cat.GetGalleryItems()[0].DisplayOrder.Value.ShouldBe(0);
        cat.GetGalleryItems()[1].DisplayOrder.Value.ShouldBe(1);
    }

    [Fact]
    public void RemoveGalleryItem_ShouldReorderRemainingItems_WhenLastItemIsRemoved()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        cat.AddGalleryItem();
        cat.AddGalleryItem();
        Result<CatGalleryItemId> lastItemResult = cat.AddGalleryItem();
        CatGalleryItemId lastItemId = lastItemResult.Value;

        //Act
        Result result = cat.RemoveGalleryItem(lastItemId);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.GetGalleryItems().Count.ShouldBe(2);
        cat.GetGalleryItems()[0].DisplayOrder.Value.ShouldBe(0);
        cat.GetGalleryItems()[1].DisplayOrder.Value.ShouldBe(1);
    }

    [Fact]
    public void RemoveGalleryItem_ShouldReturnFailure_WhenItemNotFound()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        CatGalleryItemId nonExistentItemId = CatGalleryItemId.New();

        //Act
        Result result = cat.RemoveGalleryItem(nonExistentItemId);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatGalleryItemEntity.NotFound(nonExistentItemId));
    }

    [Fact]
    public void RemoveGalleryItem_ShouldThrow_WhenEmptyIdIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);

        //Act
        Action removeItem = () => cat.RemoveGalleryItem(CatGalleryItemId.Empty);

        //Assert
        removeItem.ShouldThrow<ArgumentException>()
            .ParamName?.ToLower().ShouldContain("galleryitemid".ToLower());
    }

    [Fact]
    public void ReorderGalleryItems_ShouldReorder_WhenValidOrdersAreProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        Result<CatGalleryItemId> firstItemResult = cat.AddGalleryItem();
        Result<CatGalleryItemId> secondItemResult = cat.AddGalleryItem();
        CatGalleryItemId firstItemId = firstItemResult.Value;
        CatGalleryItemId secondItemId = secondItemResult.Value;

        Result<CatGalleryItemDisplayOrder> order0Result = CatGalleryItemDisplayOrder.Create(0, Cat.MaximumGalleryItemsCount);
        Result<CatGalleryItemDisplayOrder> order1Result = CatGalleryItemDisplayOrder.Create(1, Cat.MaximumGalleryItemsCount);
        order0Result.EnsureSuccess();
        order1Result.EnsureSuccess();

        Dictionary<CatGalleryItemId, CatGalleryItemDisplayOrder> newOrders = new()
        {
            { firstItemId, order1Result.Value },
            { secondItemId, order0Result.Value }
        };

        //Act
        Result result = cat.ReorderGalleryItems(newOrders);

        //Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ReorderGalleryItems_ShouldReturnFailure_WhenCountMismatch()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        cat.AddGalleryItem();
        cat.AddGalleryItem();

        Dictionary<CatGalleryItemId, CatGalleryItemDisplayOrder> newOrders = new();

        //Act
        Result result = cat.ReorderGalleryItems(newOrders);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.InvalidReorderOperation);
    }

    [Fact]
    public void ReorderGalleryItems_ShouldReturnFailure_WhenDuplicateDisplayOrders()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        Result<CatGalleryItemId> firstItemResult = cat.AddGalleryItem();
        Result<CatGalleryItemId> secondItemResult = cat.AddGalleryItem();
        CatGalleryItemId firstItemId = firstItemResult.Value;
        CatGalleryItemId secondItemId = secondItemResult.Value;

        Result<CatGalleryItemDisplayOrder> order0Result = CatGalleryItemDisplayOrder.Create(0, Cat.MaximumGalleryItemsCount);
        order0Result.EnsureSuccess();

        Dictionary<CatGalleryItemId, CatGalleryItemDisplayOrder> newOrders = new()
        {
            { firstItemId, order0Result.Value },
            { secondItemId, order0Result.Value }
        };

        //Act
        Result result = cat.ReorderGalleryItems(newOrders);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.DuplicateDisplayOrders);
    }

    [Fact]
    public void ReorderGalleryItems_ShouldReturnFailure_WhenDisplayOrdersNotContiguous()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        Result<CatGalleryItemId> firstItemResult = cat.AddGalleryItem();
        Result<CatGalleryItemId> secondItemResult = cat.AddGalleryItem();
        CatGalleryItemId firstItemId = firstItemResult.Value;
        CatGalleryItemId secondItemId = secondItemResult.Value;

        Result<CatGalleryItemDisplayOrder> order0Result = CatGalleryItemDisplayOrder.Create(0, Cat.MaximumGalleryItemsCount);
        Result<CatGalleryItemDisplayOrder> order5Result = CatGalleryItemDisplayOrder.Create(5, Cat.MaximumGalleryItemsCount);
        order0Result.EnsureSuccess();
        order5Result.EnsureSuccess();

        Dictionary<CatGalleryItemId, CatGalleryItemDisplayOrder> newOrders = new()
        {
            { firstItemId, order0Result.Value },
            { secondItemId, order5Result.Value }
        };

        //Act
        Result result = cat.ReorderGalleryItems(newOrders);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.DisplayOrderMustBeContiguous);
    }

    [Fact]
    public void ReorderGalleryItems_ShouldReturnFailure_WhenGalleryItemNotFound()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        cat.AddGalleryItem();
        CatGalleryItemId nonExistentItemId = CatGalleryItemId.New();

        Result<CatGalleryItemDisplayOrder> order0Result = CatGalleryItemDisplayOrder.Create(0, Cat.MaximumGalleryItemsCount);
        order0Result.EnsureSuccess();

        Dictionary<CatGalleryItemId, CatGalleryItemDisplayOrder> newOrders = new()
        {
            { nonExistentItemId, order0Result.Value }
        };

        //Act
        Result result = cat.ReorderGalleryItems(newOrders);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatGalleryItemEntity.NotFound(nonExistentItemId));
    }

    [Fact]
    public void ReorderGalleryItems_ShouldThrow_WhenNullOrdersAreProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);

        //Act
        Action reorder = () => cat.ReorderGalleryItems(null!);

        //Assert
        reorder.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldContain("neworders".ToLower());
    }
}
