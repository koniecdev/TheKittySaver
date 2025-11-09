using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class VaccinationNote : ValueObject
{
    public const int MaxLength = 250;
    public string Value { get; }

    public static Result<VaccinationNote> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<VaccinationNote>(DomainErrors.CatVaccinationEntity.VeterinarianNoteValueObject.NullOrEmpty);
        }

        if (value.Length > MaxLength)
        {
            return Result.Failure<VaccinationNote>(DomainErrors.CatVaccinationEntity.VeterinarianNoteValueObject.LongerThanAllowed);
        }

        VaccinationNote instance = new(value);
        return Result.Success(instance);
    }

    private VaccinationNote(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}