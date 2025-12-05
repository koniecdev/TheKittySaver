using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.CatAggregate.ValueObjects;

public sealed class AdoptionHistoryTests
{
    private static readonly DateTimeOffset CurrentDate = new(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ValidLastReturn = new(2025, 5, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CatHasBeenReturned_ShouldReturnSuccess_WhenValidDataProvided()
    {
        //Arrange & Act
        Result<AdoptionHistory> result = AdoptionHistory.CatHasBeenReturned(
            counterHowManyTimesWasTheCatReturned: 1,
            currentDate: CurrentDate,
            lastReturn: ValidLastReturn,
            reason: "Test reason");

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ReturnCount.ShouldBe(1);
    }

    [Fact]
    public void CatHasBeenReturned_ShouldReturnFailure_WhenCountIsNegative()
    {
        //Arrange & Act
        Result<AdoptionHistory> result = AdoptionHistory.CatHasBeenReturned(
            counterHowManyTimesWasTheCatReturned: -1,
            currentDate: CurrentDate,
            lastReturn: ValidLastReturn,
            reason: "Test reason");

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.AdoptionHistoryProperty.CountTooLow);
    }

    [Fact]
    public void CatHasBeenReturned_ShouldReturnFailure_WhenLastReturnDateIsTooOld()
    {
        //Arrange
        DateTimeOffset tooOldDate = CurrentDate.AddYears(-50);

        //Act
        Result<AdoptionHistory> result = AdoptionHistory.CatHasBeenReturned(
            counterHowManyTimesWasTheCatReturned: 1,
            currentDate: CurrentDate,
            lastReturn: tooOldDate,
            reason: "Test reason");

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.AdoptionHistoryProperty.LastReturnTooFarInPast(tooOldDate, CurrentDate));
    }

    [Fact]
    public void CatHasBeenReturned_ShouldReturnFailure_WhenReasonIsEmpty()
    {
        //Arrange & Act
        Result<AdoptionHistory> result = AdoptionHistory.CatHasBeenReturned(
            counterHowManyTimesWasTheCatReturned: 1,
            currentDate: CurrentDate,
            lastReturn: ValidLastReturn,
            reason: "   ");

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.AdoptionHistoryProperty.LastReturnReasonIsEmpty);
    }

    [Fact]
    public void CatHasBeenReturned_ShouldReturnFailure_WhenReasonIsTooLong()
    {
        //Arrange
        string longReason = new('a', AdoptionHistory.LastReturnReasonMaxLength + 1);

        //Act
        Result<AdoptionHistory> result = AdoptionHistory.CatHasBeenReturned(
            counterHowManyTimesWasTheCatReturned: 1,
            currentDate: CurrentDate,
            lastReturn: ValidLastReturn,
            reason: longReason);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.AdoptionHistoryProperty.LongerThanAllowed);
    }
}
