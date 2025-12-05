using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.EntityFramework.Aggregates.CatAggregate;

public sealed class VaccinationConfiguration : IEntityTypeConfiguration<Vaccination>
{
    public void Configure(EntityTypeBuilder<Vaccination> builder)
    {
        builder.ToTable("Vaccinations");
    }
}
