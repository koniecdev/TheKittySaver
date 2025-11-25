using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Factories;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.CatAggregate;

public sealed class CatVaccinationManagementTests
{
    private static readonly Faker Faker = new();
    private static readonly DateTimeOffset VaccinationDate = new(2024, 12, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset TestCurrentDate = new(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset TestNewVaccinationDate = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset TestNextDueDate = new(2026, 5, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void AddVaccination_ShouldAddVaccination_WhenValidDataAreProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        CreatedAt createdAt = CatFactory.CreateDefaultCreatedAt();

        //Act
        Result<Vaccination> result = cat.AddVaccination(
            VaccinationType.Rabies,
            VaccinationDate,
            createdAt);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.Vaccinations.Count.ShouldBe(1);
        cat.Vaccinations[0].Type.ShouldBe(VaccinationType.Rabies);
    }

    [Fact]
    public void AddVaccination_ShouldThrow_WhenNullCreatedAtIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);

        //Act
        Action addVaccination = () => cat.AddVaccination(
            VaccinationType.Rabies,
            VaccinationDate,
            null!);

        //Assert
        addVaccination.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldContain("createdat");
    }

    [Fact]
    public void RemoveVaccination_ShouldRemoveVaccination_WhenVaccinationExists()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        CreatedAt createdAt = CatFactory.CreateDefaultCreatedAt();
        Result<Vaccination> addResult = cat.AddVaccination(VaccinationType.Rabies, VaccinationDate, createdAt);
        VaccinationId vaccinationId = addResult.Value.Id;

        //Act
        Result result = cat.RemoveVaccination(vaccinationId);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.Vaccinations.Count.ShouldBe(0);
    }

    [Fact]
    public void RemoveVaccination_ShouldReturnFailure_WhenVaccinationNotFound()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        VaccinationId nonExistentVaccinationId = VaccinationId.New();

        //Act
        Result result = cat.RemoveVaccination(nonExistentVaccinationId);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.VaccinationEntity.NotFound(nonExistentVaccinationId));
    }

    [Fact]
    public void RemoveVaccination_ShouldThrow_WhenEmptyVaccinationIdIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);

        //Act
        Action removeVaccination = () => cat.RemoveVaccination(VaccinationId.Empty);

        //Assert
        removeVaccination.ShouldThrow<ArgumentException>()
            .ParamName?.ToLower().ShouldContain("vaccinationid");
    }

    [Fact]
    public void UpdateVaccinationType_ShouldUpdateType_WhenVaccinationExists()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        CreatedAt createdAt = CatFactory.CreateDefaultCreatedAt();
        Result<Vaccination> addResult = cat.AddVaccination(VaccinationType.Rabies, VaccinationDate, createdAt);
        VaccinationId vaccinationId = addResult.Value.Id;

        //Act
        Result result = cat.UpdateVaccinationType(vaccinationId, VaccinationType.FvrcpPanleukopenia);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.Vaccinations[0].Type.ShouldBe(VaccinationType.FvrcpPanleukopenia);
    }

    [Fact]
    public void UpdateVaccinationType_ShouldReturnFailure_WhenVaccinationNotFound()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        VaccinationId nonExistentVaccinationId = VaccinationId.New();

        //Act
        Result result = cat.UpdateVaccinationType(nonExistentVaccinationId, VaccinationType.Rabies);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.VaccinationEntity.NotFound(nonExistentVaccinationId));
    }

    [Fact]
    public void UpdateVaccinationDate_ShouldUpdateDate_WhenVaccinationExists()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        CreatedAt createdAt = CatFactory.CreateDefaultCreatedAt();
        Result<Vaccination> addResult = cat.AddVaccination(VaccinationType.Rabies, VaccinationDate, createdAt);
        VaccinationId vaccinationId = addResult.Value.Id;

        //Act
        Result result = cat.UpdateVaccinationDate(vaccinationId, TestNewVaccinationDate, TestCurrentDate);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.Vaccinations[0].VaccinationDate.ShouldBe(TestNewVaccinationDate);
    }

    [Fact]
    public void UpdateVaccinationDate_ShouldReturnFailure_WhenVaccinationNotFound()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        VaccinationId nonExistentVaccinationId = VaccinationId.New();

        //Act
        Result result = cat.UpdateVaccinationDate(nonExistentVaccinationId, VaccinationDate, TestCurrentDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.VaccinationEntity.NotFound(nonExistentVaccinationId));
    }

    [Fact]
    public void UpdateVaccinationNextDueDate_ShouldUpdateNextDueDate_WhenVaccinationExists()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        CreatedAt createdAt = CatFactory.CreateDefaultCreatedAt();
        Result<Vaccination> addResult = cat.AddVaccination(VaccinationType.Rabies, VaccinationDate, createdAt);
        VaccinationId vaccinationId = addResult.Value.Id;

        //Act
        Result result = cat.UpdateVaccinationNextDueDate(vaccinationId, TestNextDueDate, TestCurrentDate);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.Vaccinations[0].NextDueDate.ShouldBe(TestNextDueDate);
    }

    [Fact]
    public void UpdateVaccinationNextDueDate_ShouldReturnFailure_WhenVaccinationNotFound()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        VaccinationId nonExistentVaccinationId = VaccinationId.New();

        //Act
        Result result = cat.UpdateVaccinationNextDueDate(nonExistentVaccinationId, TestNextDueDate, TestCurrentDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.VaccinationEntity.NotFound(nonExistentVaccinationId));
    }

    [Fact]
    public void UpdateVaccinationVeterinarianNote_ShouldUpdateNote_WhenVaccinationExists()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        CreatedAt createdAt = CatFactory.CreateDefaultCreatedAt();
        Result<Vaccination> addResult = cat.AddVaccination(VaccinationType.Rabies, VaccinationDate, createdAt);
        VaccinationId vaccinationId = addResult.Value.Id;
        string newNote = Faker.Lorem.Sentence();

        //Act
        Result result = cat.UpdateVaccinationVeterinarianNote(vaccinationId, newNote);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.Vaccinations[0].VeterinarianNote.ShouldBe(newNote);
    }

    [Fact]
    public void UpdateVaccinationVeterinarianNote_ShouldReturnFailure_WhenVaccinationNotFound()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        VaccinationId nonExistentVaccinationId = VaccinationId.New();

        //Act
        Result result = cat.UpdateVaccinationVeterinarianNote(nonExistentVaccinationId, null);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.VaccinationEntity.NotFound(nonExistentVaccinationId));
    }
}
