using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.CatAggregate.ValueObjects;

public sealed class VaccinationDateTests
{
    private static readonly DateOnly ReferenceDate = new(2025, 6, 1);

    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidDateProvided()
    {
        //Arrange
        DateOnly validDate = new(2025, 5, 1);

        //Act
        Result<VaccinationDate> result = VaccinationDate.Create(validDate, ReferenceDate);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(validDate);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDateIsInFuture()
    {
        //Arrange
        DateOnly futureDate = ReferenceDate.AddDays(10);

        //Act
        Result<VaccinationDate> result = VaccinationDate.Create(futureDate, ReferenceDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.VaccinationEntity.DateProperty.VaccinationDateInFuture(futureDate, ReferenceDate));
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDateIsTooOld()
    {
        //Arrange
        DateOnly tooOldDate = ReferenceDate.AddYears(-50);

        //Act
        Result<VaccinationDate> result = VaccinationDate.Create(tooOldDate, ReferenceDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.VaccinationEntity.DateProperty.VaccinationDateTooOld(tooOldDate, ReferenceDate));
    }
}
