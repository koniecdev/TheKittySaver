using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.CatAggregate.ValueObjects;

public sealed class VaccinationNoteTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidValueProvided()
    {
        //Arrange & Act
        Result<VaccinationNote> result = VaccinationNote.Create("Cat reacted well");

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("Cat reacted well");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValueIsEmpty()
    {
        //Arrange & Act
        Result<VaccinationNote> result = VaccinationNote.Create("   ");

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.VaccinationEntity.VeterinarianNoteProperty.NullOrEmpty);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenValueIsTooLong()
    {
        //Arrange
        string longValue = new('a', VaccinationNote.MaxLength + 1);

        //Act
        Result<VaccinationNote> result = VaccinationNote.Create(longValue);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.VaccinationEntity.VeterinarianNoteProperty.LongerThanAllowed);
    }
}
