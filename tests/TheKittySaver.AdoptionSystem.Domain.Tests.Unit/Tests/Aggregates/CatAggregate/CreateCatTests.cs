using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Factories;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.CatAggregate;

public sealed class CreateCatTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void Create_ShouldCreateCat_WhenValidDataAreProvided()
    {
        //Arrange & Act
        Cat cat = CatFactory.CreateRandom(Faker);

        //Assert
        cat.ShouldNotBeNull();
        cat.Id.ShouldNotBe(CatId.Empty);
        cat.PersonId.ShouldNotBe(PersonId.Empty);
        cat.Name.ShouldNotBeNull();
        cat.Description.ShouldNotBeNull();
        cat.Age.ShouldNotBeNull();
        cat.Gender.ShouldNotBeNull();
        cat.Color.ShouldNotBeNull();
        cat.Weight.ShouldNotBeNull();
        cat.HealthStatus.ShouldNotBeNull();
        cat.SpecialNeeds.ShouldNotBeNull();
        cat.Temperament.ShouldNotBeNull();
        cat.AdoptionHistory.ShouldNotBeNull();
        cat.ListingSource.ShouldNotBeNull();
        cat.NeuteringStatus.ShouldNotBeNull();
        cat.InfectiousDiseaseStatus.ShouldNotBeNull();
        cat.CreatedAt.ShouldNotBeNull();
        cat.Status.ShouldBe(CatStatusType.Draft);
        cat.Vaccinations.Count.ShouldBe(0);
        cat.GetGalleryItems().Count.ShouldBe(0);
        cat.ThumbnailId.ShouldBeNull();
        cat.AdoptionAnnouncementId.ShouldBeNull();
        cat.ClaimedAt.ShouldBeNull();
        cat.PublishedAt.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldThrow_WhenEmptyPersonIdIsProvided()
    {
        //Arrange & Act
        Func<Cat> catCreation = () => CatFactory.CreateRandom(Faker, replacePersonIdWithEmpty: true);

        //Assert
        catCreation.ShouldThrow<ArgumentException>()
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(Cat.PersonId).ToLowerInvariant());
    }

    [Fact]
    public void Create_ShouldThrow_WhenNullNameIsProvided()
    {
        //Arrange & Act
        Func<Cat> catCreation = () => CatFactory.CreateRandom(Faker, replaceNameWithNull: true);

        //Assert
        catCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(Cat.Name).ToLowerInvariant());
    }

    [Fact]
    public void Create_ShouldThrow_WhenNullDescriptionIsProvided()
    {
        //Arrange & Act
        Func<Cat> catCreation = () => CatFactory.CreateRandom(Faker, replaceDescriptionWithNull: true);

        //Assert
        catCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(Cat.Description).ToLowerInvariant());
    }

    [Fact]
    public void Create_ShouldThrow_WhenNullAgeIsProvided()
    {
        //Arrange & Act
        Func<Cat> catCreation = () => CatFactory.CreateRandom(Faker, replaceAgeWithNull: true);

        //Assert
        catCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(Cat.Age).ToLowerInvariant());
    }

    [Fact]
    public void Create_ShouldThrow_WhenNullGenderIsProvided()
    {
        //Arrange & Act
        Func<Cat> catCreation = () => CatFactory.CreateRandom(Faker, replaceGenderWithNull: true);

        //Assert
        catCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(Cat.Gender).ToLowerInvariant());
    }

    [Fact]
    public void Create_ShouldThrow_WhenNullColorIsProvided()
    {
        //Arrange & Act
        Func<Cat> catCreation = () => CatFactory.CreateRandom(Faker, replaceColorWithNull: true);

        //Assert
        catCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(Cat.Color).ToLowerInvariant());
    }

    [Fact]
    public void Create_ShouldThrow_WhenNullWeightIsProvided()
    {
        //Arrange & Act
        Func<Cat> catCreation = () => CatFactory.CreateRandom(Faker, replaceWeightWithNull: true);

        //Assert
        catCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(Cat.Weight).ToLowerInvariant());
    }

    [Fact]
    public void Create_ShouldThrow_WhenNullHealthStatusIsProvided()
    {
        //Arrange & Act
        Func<Cat> catCreation = () => CatFactory.CreateRandom(Faker, replaceHealthStatusWithNull: true);

        //Assert
        catCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(Cat.HealthStatus).ToLowerInvariant());
    }

    [Fact]
    public void Create_ShouldThrow_WhenNullSpecialNeedsIsProvided()
    {
        //Arrange & Act
        Func<Cat> catCreation = () => CatFactory.CreateRandom(Faker, replaceSpecialNeedsWithNull: true);

        //Assert
        catCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(Cat.SpecialNeeds).ToLowerInvariant());
    }

    [Fact]
    public void Create_ShouldThrow_WhenNullTemperamentIsProvided()
    {
        //Arrange & Act
        Func<Cat> catCreation = () => CatFactory.CreateRandom(Faker, replaceTemperamentWithNull: true);

        //Assert
        catCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(Cat.Temperament).ToLowerInvariant());
    }

    [Fact]
    public void Create_ShouldThrow_WhenNullAdoptionHistoryIsProvided()
    {
        //Arrange & Act
        Func<Cat> catCreation = () => CatFactory.CreateRandom(Faker, replaceAdoptionHistoryWithNull: true);

        //Assert
        catCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(Cat.AdoptionHistory).ToLowerInvariant());
    }

    [Fact]
    public void Create_ShouldThrow_WhenNullListingSourceIsProvided()
    {
        //Arrange & Act
        Func<Cat> catCreation = () => CatFactory.CreateRandom(Faker, replaceListingSourceWithNull: true);

        //Assert
        catCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(Cat.ListingSource).ToLowerInvariant());
    }

    [Fact]
    public void Create_ShouldThrow_WhenNullNeuteringStatusIsProvided()
    {
        //Arrange & Act
        Func<Cat> catCreation = () => CatFactory.CreateRandom(Faker, replaceNeuteringStatusWithNull: true);

        //Assert
        catCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(Cat.NeuteringStatus).ToLowerInvariant());
    }

    [Fact]
    public void Create_ShouldThrow_WhenNullInfectiousDiseaseStatusIsProvided()
    {
        //Arrange & Act
        Func<Cat> catCreation = () => CatFactory.CreateRandom(Faker, replaceInfectiousDiseaseStatusWithNull: true);

        //Assert
        catCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(Cat.InfectiousDiseaseStatus).ToLowerInvariant());
    }

    [Fact]
    public void Create_ShouldThrow_WhenNullCreatedAtIsProvided()
    {
        //Arrange & Act
        Func<Cat> catCreation = () => CatFactory.CreateRandom(Faker, replaceCreatedAtWithNull: true);

        //Assert
        catCreation.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldBe(nameof(Cat.CreatedAt).ToLowerInvariant());
    }
}
