using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.EntityFramework.Consts;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;

namespace TheKittySaver.AdoptionSystem.Domain.EntityFramework.Aggregates.CatAggregate;

public sealed class VaccinationConfiguration : IEntityTypeConfiguration<Vaccination>
{
    public void Configure(EntityTypeBuilder<Vaccination> builder)
    {
        builder.ToTable("Vaccinations");

        builder.HasQueryFilter(x => x.ArchivedAt == null);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        EntityConfiguration.ConfigureCreatedAt(builder);

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(EnumConsts.MaxLength);

        builder.ComplexProperty(x => x.Date, complexBuilder =>
        {
            complexBuilder.IsRequired();

            complexBuilder.Property(x => x.Value)
                .HasColumnName(nameof(VaccinationDate));
        });

        builder.ComplexProperty(x => x.VeterinarianNote, complexBuilder =>
        {
            complexBuilder.IsRequired(false);
            complexBuilder.Property(x => x.Value)
                .HasColumnName(nameof(Vaccination.VeterinarianNote))
                .HasMaxLength(VaccinationNote.MaxLength);
        });

        builder.ComplexProperty(x => x.ArchivedAt, complexBuilder =>
        {
            complexBuilder.IsRequired(false);
            complexBuilder.Property(x => x.Value)
                .HasColumnName(nameof(Vaccination.ArchivedAt));
        });
    }
}
