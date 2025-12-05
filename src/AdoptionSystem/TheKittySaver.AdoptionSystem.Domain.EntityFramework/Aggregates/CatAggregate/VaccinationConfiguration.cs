using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.EntityFramework.Consts;

namespace TheKittySaver.AdoptionSystem.Domain.EntityFramework.Aggregates.CatAggregate;

public sealed class VaccinationConfiguration : IEntityTypeConfiguration<Vaccination>
{
    public void Configure(EntityTypeBuilder<Vaccination> builder)
    {
        builder.ToTable("Vaccinations");
        
        builder.Property(x => x.Id)
            .ValueGeneratedNever();
        
        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(EnumConsts.MaxLength);
        
        //todo: does vaccination .Dates even have sense?
        builder.ComplexProperty(x => x.Dates, complexBuilder =>
        {
            const string prefix = nameof(Vaccination.Dates);

            complexBuilder.IsRequired();

            complexBuilder.Property(x => x.VaccinationDate)
                .HasColumnName($"{prefix}{nameof(VaccinationDates.VaccinationDate)}");

            complexBuilder.Property(x => x.NextDueDate)
                .HasColumnName($"{prefix}{nameof(VaccinationDates.NextDueDate)}");
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
