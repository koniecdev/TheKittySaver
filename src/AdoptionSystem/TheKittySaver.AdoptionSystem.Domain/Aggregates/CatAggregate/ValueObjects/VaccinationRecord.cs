using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class VaccinationRecord : ValueObject
{
    private readonly List<Vaccination> _vaccinations;
    public IReadOnlyList<Vaccination> Vaccinations => _vaccinations.AsReadOnly();
    
    public static VaccinationRecord Empty() => new([]);
    
    public static VaccinationRecord Create(List<Vaccination> vaccinations)
    {
        ArgumentNullException.ThrowIfNull(vaccinations);
        return new VaccinationRecord(vaccinations);
    }
    
    public bool HasVaccination(VaccinationType type) 
        => _vaccinations.Any(v => v.Type == type);
    
    public Vaccination? GetLatestVaccination(VaccinationType type) 
        => _vaccinations
            .Where(v => v.Type == type)
            .MaxBy(v => v.VaccinationDate);
    
    public bool HasCoreVaccinations()
    {
        return HasVaccination(VaccinationType.FVRCP_Panleukopenia) &&
               HasVaccination(VaccinationType.FVRCP_Rhinotracheitis) &&
               HasVaccination(VaccinationType.FVRCP_Calicivirus);
    }

    private VaccinationRecord(List<Vaccination> vaccinations)
    {
        _vaccinations = vaccinations;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        foreach (Vaccination vaccination in _vaccinations.OrderBy(v => v.VaccinationDate))
        {
            yield return vaccination;
        }
    }
}