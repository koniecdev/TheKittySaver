using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Factories;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.CatAggregate;

public sealed class UpdateCatTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void UpdateName_ShouldUpdateName_WhenValidNameIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        CatName newName = CatFactory.CreateRandomName(Faker);

        //Act
        Result result = cat.UpdateName(newName);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.Name.ShouldBe(newName);
    }

    [Fact]
    public void UpdateName_ShouldThrow_WhenNullNameIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);

        //Act
        Action updateName = () => cat.UpdateName(null!);

        //Assert
        updateName.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldContain("name");
    }

    [Fact]
    public void UpdateDescription_ShouldUpdateDescription_WhenValidDescriptionIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        CatDescription newDescription = CatFactory.CreateRandomDescription(Faker);

        //Act
        Result result = cat.UpdateDescription(newDescription);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.Description.ShouldBe(newDescription);
    }

    [Fact]
    public void UpdateDescription_ShouldThrow_WhenNullDescriptionIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);

        //Act
        Action updateDescription = () => cat.UpdateDescription(null!);

        //Assert
        updateDescription.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldContain("description");
    }

    [Fact]
    public void UpdateAge_ShouldUpdateAge_WhenValidAgeIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        CatAge newAge = CatFactory.CreateRandomAge(Faker);

        //Act
        Result result = cat.UpdateAge(newAge);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.Age.ShouldBe(newAge);
    }

    [Fact]
    public void UpdateAge_ShouldThrow_WhenNullAgeIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);

        //Act
        Action updateAge = () => cat.UpdateAge(null!);

        //Assert
        updateAge.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldContain("age");
    }

    [Fact]
    public void UpdateGender_ShouldUpdateGender_WhenValidGenderIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        CatGender newGender = CatGender.Female();

        //Act
        Result result = cat.UpdateGender(newGender);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.Gender.ShouldBe(newGender);
    }

    [Fact]
    public void UpdateGender_ShouldThrow_WhenNullGenderIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);

        //Act
        Action updateGender = () => cat.UpdateGender(null!);

        //Assert
        updateGender.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldContain("gender");
    }

    [Fact]
    public void UpdateColor_ShouldUpdateColor_WhenValidColorIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        CatColor newColor = CatColor.Black();

        //Act
        Result result = cat.UpdateColor(newColor);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.Color.ShouldBe(newColor);
    }

    [Fact]
    public void UpdateColor_ShouldThrow_WhenNullColorIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);

        //Act
        Action updateColor = () => cat.UpdateColor(null!);

        //Assert
        updateColor.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldContain("color");
    }

    [Fact]
    public void UpdateWeight_ShouldUpdateWeight_WhenValidWeightIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        CatWeight newWeight = CatFactory.CreateRandomWeight(Faker);

        //Act
        Result result = cat.UpdateWeight(newWeight);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.Weight.ShouldBe(newWeight);
    }

    [Fact]
    public void UpdateWeight_ShouldThrow_WhenNullWeightIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);

        //Act
        Action updateWeight = () => cat.UpdateWeight(null!);

        //Assert
        updateWeight.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldContain("weight");
    }

    [Fact]
    public void UpdateHealthStatus_ShouldUpdateHealthStatus_WhenValidStatusIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        HealthStatus newStatus = HealthStatus.MinorIssues();

        //Act
        Result result = cat.UpdateHealthStatus(newStatus);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.HealthStatus.ShouldBe(newStatus);
    }

    [Fact]
    public void UpdateHealthStatus_ShouldThrow_WhenNullStatusIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);

        //Act
        Action updateHealthStatus = () => cat.UpdateHealthStatus(null!);

        //Assert
        updateHealthStatus.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldContain("healthstatus");
    }

    [Fact]
    public void UpdateSpecialNeeds_ShouldUpdateSpecialNeeds_WhenValidStatusIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        SpecialNeedsStatus newStatus = SpecialNeedsStatus.None();

        //Act
        Result result = cat.UpdateSpecialNeeds(newStatus);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.SpecialNeeds.ShouldBe(newStatus);
    }

    [Fact]
    public void UpdateSpecialNeeds_ShouldThrow_WhenNullStatusIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);

        //Act
        Action updateSpecialNeeds = () => cat.UpdateSpecialNeeds(null!);

        //Assert
        updateSpecialNeeds.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldContain("specialneeds");
    }

    [Fact]
    public void UpdateTemperament_ShouldUpdateTemperament_WhenValidTemperamentIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        Temperament newTemperament = Temperament.Independent();

        //Act
        Result result = cat.UpdateTemperament(newTemperament);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.Temperament.ShouldBe(newTemperament);
    }

    [Fact]
    public void UpdateTemperament_ShouldThrow_WhenNullTemperamentIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);

        //Act
        Action updateTemperament = () => cat.UpdateTemperament(null!);

        //Assert
        updateTemperament.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldContain("temperament");
    }

    [Fact]
    public void UpdateAdoptionHistory_ShouldUpdateAdoptionHistory_WhenValidHistoryIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        AdoptionHistory newHistory = AdoptionHistory.CatHasNeverBeenAdopted;

        //Act
        Result result = cat.UpdateAdoptionHistory(newHistory);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.AdoptionHistory.ShouldBe(newHistory);
    }

    [Fact]
    public void UpdateAdoptionHistory_ShouldThrow_WhenNullHistoryIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);

        //Act
        Action updateAdoptionHistory = () => cat.UpdateAdoptionHistory(null!);

        //Assert
        updateAdoptionHistory.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldContain("adoptionhistory");
    }

    [Fact]
    public void UpdateListingSource_ShouldUpdateListingSource_WhenValidSourceIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        ListingSource newSource = CatFactory.CreateRandomListingSource(Faker);

        //Act
        Result result = cat.UpdateListingSource(newSource);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.ListingSource.ShouldBe(newSource);
    }

    [Fact]
    public void UpdateListingSource_ShouldThrow_WhenNullSourceIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);

        //Act
        Action updateListingSource = () => cat.UpdateListingSource(null!);

        //Assert
        updateListingSource.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldContain("listingsource");
    }

    [Fact]
    public void UpdateNeuteringStatus_ShouldUpdateNeuteringStatus_WhenValidStatusIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        Result<NeuteringStatus> neuteredResult = NeuteringStatus.Neutered();
        NeuteringStatus newStatus = neuteredResult.Value;

        //Act
        Result result = cat.UpdateNeuteringStatus(newStatus);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.NeuteringStatus.ShouldBe(newStatus);
    }

    [Fact]
    public void UpdateNeuteringStatus_ShouldThrow_WhenNullStatusIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);

        //Act
        Action updateNeuteringStatus = () => cat.UpdateNeuteringStatus(null!);

        //Assert
        updateNeuteringStatus.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldContain("neuteringstatus");
    }

    [Fact]
    public void UpdateInfectiousDiseaseStatus_ShouldUpdateStatus_WhenValidStatusIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        InfectiousDiseaseStatus newStatus = CatFactory.CreateRandomInfectiousDiseaseStatus(Faker);

        //Act
        Result result = cat.UpdateInfectiousDiseaseStatus(newStatus);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.InfectiousDiseaseStatus.ShouldBe(newStatus);
    }

    [Fact]
    public void UpdateInfectiousDiseaseStatus_ShouldThrow_WhenNullStatusIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);

        //Act
        Action updateStatus = () => cat.UpdateInfectiousDiseaseStatus(null!);

        //Assert
        updateStatus.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldContain("infectiousdiseasestatus");
    }

    [Fact]
    public void UpdateThumbnail_ShouldUpdateThumbnail_WhenValidThumbnailIdIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        CatThumbnailId newThumbnailId = CatThumbnailId.New();

        //Act
        Result result = cat.UpdateThumbnail(newThumbnailId);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.ThumbnailId.ShouldBe(newThumbnailId);
    }

    [Fact]
    public void UpdateThumbnail_ShouldThrow_WhenEmptyThumbnailIdIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);

        //Act
        Action updateThumbnail = () => cat.UpdateThumbnail(CatThumbnailId.Empty);

        //Assert
        updateThumbnail.ShouldThrow<ArgumentException>()
            .ParamName?.ToLower().ShouldContain("thumbnailid");
    }
}
