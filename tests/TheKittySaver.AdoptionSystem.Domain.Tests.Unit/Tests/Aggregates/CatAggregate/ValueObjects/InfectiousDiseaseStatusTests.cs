using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.CatAggregate.ValueObjects;

public sealed class InfectiousDiseaseStatusTests
{
    private static readonly DateOnly CurrentDate = new(2025, 6, 1);
    private static readonly DateOnly ValidTestDate = new(2024, 1, 15);

    [Theory]
    [InlineData(FivStatus.Positive, FivStatus.Positive, true)]
    [InlineData(FivStatus.Positive, FivStatus.Negative, false)]
    [InlineData(FivStatus.Positive, FivStatus.NotTested, true)]
    [InlineData(FivStatus.Negative, FivStatus.Positive, false)]
    [InlineData(FivStatus.Negative, FivStatus.Negative, true)]
    [InlineData(FivStatus.Negative, FivStatus.NotTested, true)]
    [InlineData(FivStatus.NotTested, FivStatus.Positive, true)]
    [InlineData(FivStatus.NotTested, FivStatus.Negative, true)]
    [InlineData(FivStatus.NotTested, FivStatus.NotTested, true)]
    public void IsCompatibleWith_ShouldReturnExpectedResult_ForFivStatusCombinations(
        FivStatus status1,
        FivStatus status2,
        bool expectedCompatibility)
    {
        //Arrange
        InfectiousDiseaseStatus diseaseStatus1 = CreateDiseaseStatus(fivStatus: status1);
        InfectiousDiseaseStatus diseaseStatus2 = CreateDiseaseStatus(fivStatus: status2);

        //Act
        bool isCompatible = diseaseStatus1.IsCompatibleWith(diseaseStatus2);

        //Assert
        isCompatible.ShouldBe(expectedCompatibility);
    }


    [Theory]
    [InlineData(FelvStatus.Positive, FelvStatus.Positive, true)]
    [InlineData(FelvStatus.Positive, FelvStatus.Negative, false)]
    [InlineData(FelvStatus.Positive, FelvStatus.NotTested, true)]
    [InlineData(FelvStatus.Negative, FelvStatus.Positive, false)]
    [InlineData(FelvStatus.Negative, FelvStatus.Negative, true)]
    [InlineData(FelvStatus.Negative, FelvStatus.NotTested, true)]
    [InlineData(FelvStatus.NotTested, FelvStatus.Positive, true)]
    [InlineData(FelvStatus.NotTested, FelvStatus.Negative, true)]
    [InlineData(FelvStatus.NotTested, FelvStatus.NotTested, true)]
    public void IsCompatibleWith_ShouldReturnExpectedResult_ForFelvStatusCombinations(
        FelvStatus status1,
        FelvStatus status2,
        bool expectedCompatibility)
    {
        //Arrange
        InfectiousDiseaseStatus diseaseStatus1 = CreateDiseaseStatus(felvStatus: status1);
        InfectiousDiseaseStatus diseaseStatus2 = CreateDiseaseStatus(felvStatus: status2);

        //Act
        bool isCompatible = diseaseStatus1.IsCompatibleWith(diseaseStatus2);

        //Assert
        isCompatible.ShouldBe(expectedCompatibility);
    }

    [Fact]
    public void IsCompatibleWith_ShouldReturnTrue_WhenBothCatsHaveSameDiseaseStatuses()
    {
        //Arrange
        InfectiousDiseaseStatus diseaseStatus1 = CreateDiseaseStatus(
            fivStatus: FivStatus.Positive,
            felvStatus: FelvStatus.Negative);

        InfectiousDiseaseStatus diseaseStatus2 = CreateDiseaseStatus(
            fivStatus: FivStatus.Positive,
            felvStatus: FelvStatus.Negative);

        //Act
        bool isCompatible = diseaseStatus1.IsCompatibleWith(diseaseStatus2);

        //Assert
        isCompatible.ShouldBeTrue();
    }

    [Fact]
    public void IsCompatibleWith_ShouldReturnFalse_WhenFivCompatibleButFelvIncompatible()
    {
        //Arrange - Same FIV (Positive), different FeLV (Positive vs Negative)
        InfectiousDiseaseStatus diseaseStatus1 = CreateDiseaseStatus(
            fivStatus: FivStatus.Positive,
            felvStatus: FelvStatus.Positive);

        InfectiousDiseaseStatus diseaseStatus2 = CreateDiseaseStatus(
            fivStatus: FivStatus.Positive,
            felvStatus: FelvStatus.Negative);

        //Act
        bool isCompatible = diseaseStatus1.IsCompatibleWith(diseaseStatus2);

        //Assert
        isCompatible.ShouldBeFalse();
    }

    [Fact]
    public void IsCompatibleWith_ShouldReturnFalse_WhenFelvCompatibleButFivIncompatible()
    {
        //Arrange - Same FeLV (Negative), different FIV (Positive vs Negative)
        InfectiousDiseaseStatus diseaseStatus1 = CreateDiseaseStatus(
            fivStatus: FivStatus.Positive,
            felvStatus: FelvStatus.Negative);

        InfectiousDiseaseStatus diseaseStatus2 = CreateDiseaseStatus(
            fivStatus: FivStatus.Negative,
            felvStatus: FelvStatus.Negative);

        //Act
        bool isCompatible = diseaseStatus1.IsCompatibleWith(diseaseStatus2);

        //Assert
        isCompatible.ShouldBeFalse();
    }

    [Fact]
    public void IsCompatibleWith_ShouldReturnFalse_WhenBothDiseasesIncompatible()
    {
        //Arrange - Both incompatible (FIV+/FeLV+ vs FIV-/FeLV-)
        InfectiousDiseaseStatus diseaseStatus1 = CreateDiseaseStatus(
            fivStatus: FivStatus.Positive,
            felvStatus: FelvStatus.Positive);

        InfectiousDiseaseStatus diseaseStatus2 = CreateDiseaseStatus(
            fivStatus: FivStatus.Negative,
            felvStatus: FelvStatus.Negative);

        //Act
        bool isCompatible = diseaseStatus1.IsCompatibleWith(diseaseStatus2);

        //Assert
        isCompatible.ShouldBeFalse();
    }

    [Fact]
    public void IsCompatibleWith_ShouldReturnTrue_WhenOneCatIsCompletelyNotTested()
    {
        //Arrange
        InfectiousDiseaseStatus diseaseStatus1 = CreateDiseaseStatus(
            fivStatus: FivStatus.Positive,
            felvStatus: FelvStatus.Negative);

        InfectiousDiseaseStatus diseaseStatus2 = CreateDiseaseStatus(
            fivStatus: FivStatus.NotTested,
            felvStatus: FelvStatus.NotTested);

        //Act
        bool isCompatible = diseaseStatus1.IsCompatibleWith(diseaseStatus2);

        //Assert
        isCompatible.ShouldBeTrue();
    }

    [Fact]
    public void IsCompatibleWith_ShouldBeSymmetric_WhenCheckingCompatibility()
    {
        //Arrange
        InfectiousDiseaseStatus diseaseStatus1 = CreateDiseaseStatus(
            fivStatus: FivStatus.Positive,
            felvStatus: FelvStatus.NotTested);

        InfectiousDiseaseStatus diseaseStatus2 = CreateDiseaseStatus(
            fivStatus: FivStatus.Negative,
            felvStatus: FelvStatus.NotTested);

        //Act
        bool compatibility1To2 = diseaseStatus1.IsCompatibleWith(diseaseStatus2);
        bool compatibility2To1 = diseaseStatus2.IsCompatibleWith(diseaseStatus1);

        //Assert
        compatibility1To2.ShouldBe(compatibility2To1);
    }

    [Theory]
    [InlineData(FivStatus.Positive, true)]
    [InlineData(FivStatus.Negative, false)]
    [InlineData(FivStatus.NotTested, false)]
    public void HasFiv_ShouldReturnExpectedValue_BasedOnFivStatus(FivStatus fivStatus, bool expectedHasFiv)
    {
        //Arrange
        InfectiousDiseaseStatus diseaseStatus = CreateDiseaseStatus(fivStatus: fivStatus);

        //Act & Assert
        diseaseStatus.HasFiv.ShouldBe(expectedHasFiv);
    }

    [Theory]
    [InlineData(FelvStatus.Positive, true)]
    [InlineData(FelvStatus.Negative, false)]
    [InlineData(FelvStatus.NotTested, false)]
    public void HasFelv_ShouldReturnExpectedValue_BasedOnFelvStatus(FelvStatus felvStatus, bool expectedHasFelv)
    {
        //Arrange
        InfectiousDiseaseStatus diseaseStatus = CreateDiseaseStatus(felvStatus: felvStatus);

        //Act & Assert
        diseaseStatus.HasFelv.ShouldBe(expectedHasFelv);
    }

    [Theory]
    [InlineData(FivStatus.Positive, FelvStatus.Negative, true)]
    [InlineData(FivStatus.Negative, FelvStatus.Positive, true)]
    [InlineData(FivStatus.Positive, FelvStatus.Positive, true)]
    [InlineData(FivStatus.Negative, FelvStatus.Negative, false)]
    [InlineData(FivStatus.NotTested, FelvStatus.NotTested, false)]
    public void HasAnyInfectiousDisease_ShouldReturnExpectedValue_BasedOnDiseaseStatuses(
        FivStatus fivStatus,
        FelvStatus felvStatus,
        bool expectedHasDisease)
    {
        //Arrange
        InfectiousDiseaseStatus diseaseStatus = CreateDiseaseStatus(fivStatus, felvStatus);

        //Act & Assert
        diseaseStatus.HasAnyInfectiousDisease.ShouldBe(expectedHasDisease);
    }

    [Theory]
    [InlineData(FivStatus.Negative, FelvStatus.Negative, true)]
    [InlineData(FivStatus.NotTested, FelvStatus.NotTested, true)]
    [InlineData(FivStatus.Positive, FelvStatus.Negative, false)]
    [InlineData(FivStatus.Negative, FelvStatus.Positive, false)]
    [InlineData(FivStatus.Positive, FelvStatus.Positive, false)]
    public void IsSafeToMixWithOtherCats_ShouldReturnExpectedValue_BasedOnDiseaseStatuses(
        FivStatus fivStatus,
        FelvStatus felvStatus,
        bool expectedSafeToMix)
    {
        //Arrange
        InfectiousDiseaseStatus diseaseStatus = CreateDiseaseStatus(fivStatus, felvStatus);

        //Act & Assert
        diseaseStatus.IsSafeToMixWithOtherCats.ShouldBe(expectedSafeToMix);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenValidDataIsProvided()
    {
        //Arrange
        FivStatus fivStatus = FivStatus.Negative;
        FelvStatus felvStatus = FelvStatus.Negative;
        DateOnly testDate = ValidTestDate;

        //Act
        Result<InfectiousDiseaseStatus> result = InfectiousDiseaseStatus.Create(
            fivStatus,
            felvStatus,
            testDate,
            CurrentDate);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.FivStatus.ShouldBe(fivStatus);
        result.Value.FelvStatus.ShouldBe(felvStatus);
        result.Value.LastTestedAt.ShouldBe(testDate);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenTestDateIsInFuture()
    {
        //Arrange
        FivStatus fivStatus = FivStatus.Positive;
        FelvStatus felvStatus = FelvStatus.Negative;
        DateOnly futureTestDate = CurrentDate.AddDays(30);

        //Act
        Result<InfectiousDiseaseStatus> result = InfectiousDiseaseStatus.Create(
            fivStatus,
            felvStatus,
            futureTestDate,
            CurrentDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.InfectiousDiseaseStatusProperty
            .TestDateInFuture(futureTestDate, CurrentDate));
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenTestDateIsTooOld()
    {
        //Arrange
        const FivStatus fivStatus = FivStatus.Negative;
        const FelvStatus felvStatus = FelvStatus.Negative;
        DateOnly veryOldTestDate = CurrentDate.AddYears(-50); // Way too old for a cat

        //Act
        Result<InfectiousDiseaseStatus> result = InfectiousDiseaseStatus.Create(
            fivStatus,
            felvStatus,
            veryOldTestDate,
            CurrentDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.InfectiousDiseaseStatusProperty
            .TestDateTooOld(veryOldTestDate, CurrentDate));
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenTestDateIsToday()
    {
        //Arrange
        const FivStatus fivStatus = FivStatus.NotTested;
        const FelvStatus felvStatus = FelvStatus.NotTested;

        //Act
        Result<InfectiousDiseaseStatus> result = InfectiousDiseaseStatus.Create(
            fivStatus,
            felvStatus,
            CurrentDate,
            CurrentDate);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.LastTestedAt.ShouldBe(CurrentDate);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenTestDateIsRecentButValid()
    {
        //Arrange
        const FivStatus fivStatus = FivStatus.Positive;
        const FelvStatus felvStatus = FelvStatus.Positive;
        DateOnly recentTestDate = CurrentDate.AddMonths(-6);

        //Act
        Result<InfectiousDiseaseStatus> result = InfectiousDiseaseStatus.Create(
            fivStatus,
            felvStatus,
            recentTestDate,
            CurrentDate);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.LastTestedAt.ShouldBe(recentTestDate);
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenAllPropertiesAreEqual()
    {
        //Arrange
        InfectiousDiseaseStatus diseaseStatus1 = CreateDiseaseStatus(
            FivStatus.Positive);

        InfectiousDiseaseStatus diseaseStatus2 = CreateDiseaseStatus(
            FivStatus.Positive);

        //Act & Assert
        diseaseStatus1.Equals(diseaseStatus2).ShouldBeTrue();
        (diseaseStatus1 == diseaseStatus2).ShouldBeTrue();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenFivStatusDiffers()
    {
        //Arrange
        InfectiousDiseaseStatus diseaseStatus1 = CreateDiseaseStatus(
            FivStatus.Positive);

        InfectiousDiseaseStatus diseaseStatus2 = CreateDiseaseStatus();

        //Act & Assert
        diseaseStatus1.Equals(diseaseStatus2).ShouldBeFalse();
        (diseaseStatus1 != diseaseStatus2).ShouldBeTrue();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenFelvStatusDiffers()
    {
        //Arrange
        InfectiousDiseaseStatus diseaseStatus1 = CreateDiseaseStatus(
            FivStatus.Negative,
            FelvStatus.Positive);

        InfectiousDiseaseStatus diseaseStatus2 = CreateDiseaseStatus();

        //Act & Assert
        diseaseStatus1.Equals(diseaseStatus2).ShouldBeFalse();
    }

    private static InfectiousDiseaseStatus CreateDiseaseStatus(
        FivStatus fivStatus = FivStatus.Negative,
        FelvStatus felvStatus = FelvStatus.Negative,
        DateOnly? testDate = null)
    {
        DateOnly lastTestedAt = testDate ?? ValidTestDate;

        Result<InfectiousDiseaseStatus> result = InfectiousDiseaseStatus.Create(
            fivStatus,
            felvStatus,
            lastTestedAt,
            CurrentDate);

        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                $"Failed to create InfectiousDiseaseStatus for test: {result.Error}");
        }

        return result.Value;
    }
}
