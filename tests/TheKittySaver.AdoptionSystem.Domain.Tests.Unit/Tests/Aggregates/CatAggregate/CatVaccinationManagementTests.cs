using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Extensions;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Factories;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.CatAggregate;

public sealed class CatVaccinationManagementTests
{
    private static readonly Faker Faker = new();
    private static readonly DateOnly TheVaccinationDate = new(2024, 12, 1);
    private static readonly DateOnly TestCurrentDate = new(2025, 6, 1);
    private static readonly DateOnly TestNewVaccinationDate = new(2025, 1, 1);
    private static readonly DateTimeOffset FixedDateOfOperation = 
        new(2025, 1, 2, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void AddVaccination_ShouldAddVaccination_WhenValidDataAreProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);

        //Act
        Result<Vaccination> result = cat.AddVaccination(
            VaccinationType.Rabies,
            TheVaccinationDate,
            FixedDateOfOperation);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.Vaccinations.Count.ShouldBe(1);
        cat.Vaccinations[0].Type.ShouldBe(VaccinationType.Rabies);
    }

    [Fact]
    public void RemoveVaccination_ShouldRemoveVaccination_WhenVaccinationExists()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        Result<Vaccination> addResult = 
            cat.AddVaccination(VaccinationType.Rabies, TheVaccinationDate, FixedDateOfOperation);
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
            .ParamName?.ToLowerInvariant().ShouldContain("vaccinationid".ToLowerInvariant());
    }

    [Fact]
    public void UpdateVaccinationType_ShouldUpdateType_WhenVaccinationExists()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        Result<Vaccination> addResult = 
            cat.AddVaccination(VaccinationType.Rabies, TheVaccinationDate, FixedDateOfOperation);
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
        Result<Vaccination> addResult = 
            cat.AddVaccination(VaccinationType.Rabies, TheVaccinationDate, FixedDateOfOperation);
        VaccinationId vaccinationId = addResult.Value.Id;

        //Act
        Result<VaccinationDate> newDateResult = VaccinationDate.Create(TestNewVaccinationDate, TestCurrentDate);
        newDateResult.EnsureSuccess();
        Result result = cat.UpdateVaccinationDate(vaccinationId, newDateResult.Value);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.Vaccinations[0].Date.Value.ShouldBe(TestNewVaccinationDate);
    }

    [Fact]
    public void UpdateVaccinationDate_ShouldReturnFailure_WhenVaccinationNotFound()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        VaccinationId nonExistentVaccinationId = VaccinationId.New();

        //Act
        Result<VaccinationDate> dateResult = VaccinationDate.Create(TheVaccinationDate, TestCurrentDate);
        dateResult.EnsureSuccess();
        Result result = cat.UpdateVaccinationDate(nonExistentVaccinationId, dateResult.Value);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.VaccinationEntity.NotFound(nonExistentVaccinationId));
    }

    [Fact]
    public void AddVaccination_ShouldReturnFailure_WhenVaccinationDateInFuture()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        DateOnly futureDate = new(2050, 1, 1);

        //Act
        Result<Vaccination> result = cat.AddVaccination(VaccinationType.Rabies, futureDate, FixedDateOfOperation);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.VaccinationEntity.DateProperty.VaccinationDateInFuture(
            futureDate,
            DateOnly.FromDateTime(FixedDateOfOperation.DateTime)));
    }

    [Fact]
    public void AddVaccination_ShouldReturnFailure_WhenVaccinationDateTooOld()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        DateOnly tooOldDate = new(1980, 1, 1);

        //Act
        Result<Vaccination> result = cat.AddVaccination(VaccinationType.Rabies, tooOldDate, FixedDateOfOperation);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.VaccinationEntity.DateProperty.VaccinationDateTooOld(
            tooOldDate,
            DateOnly.FromDateTime(FixedDateOfOperation.DateTime)));
    }

    [Fact]
    public void UpdateVaccinationVeterinarianNote_ShouldUpdateNote_WhenVaccinationExists()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        Result<Vaccination> addResult = 
            cat.AddVaccination(VaccinationType.Rabies, TheVaccinationDate, FixedDateOfOperation);
        VaccinationId vaccinationId = addResult.Value.Id;
        string newNote = Faker.Lorem.Sentence();

        //Act
        Result<VaccinationNote> newNoteResult = VaccinationNote.Create(newNote);
        newNoteResult.EnsureSuccess();
        Result result = cat.UpdateVaccinationVeterinarianNote(vaccinationId, newNoteResult.Value);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.Vaccinations[0].VeterinarianNote.ShouldBe(newNoteResult.Value);
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
