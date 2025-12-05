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

        builder.Property(x => x.Type);

        builder.ComplexProperty(x => x.Dates, complexBuilder =>
        {
            complexBuilder.IsRequired();
            complexBuilder.Property(x => x.VaccinationDate)
                .HasColumnName("Dates_VaccinationDate");
            complexBuilder.Property(x => x.NextDueDate)
                .HasColumnName("Dates_NextDueDate");
        });

        builder.ComplexProperty(x => x.VeterinarianNote, complexBuilder =>
        {
            complexBuilder.IsRequired(false);
            complexBuilder.Property(x => x.Value)
                .HasColumnName(nameof(Vaccination.VeterinarianNote))
                .HasMaxLength(VaccinationNote.MaxLength);
        });
    }
}
