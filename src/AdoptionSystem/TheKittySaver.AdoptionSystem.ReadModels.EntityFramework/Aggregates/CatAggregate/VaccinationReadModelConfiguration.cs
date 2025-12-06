using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.ReadModels.EntityFramework.Aggregates.CatAggregate;

public sealed class VaccinationReadModelConfiguration : IEntityTypeConfiguration<VaccinationReadModel>
{
    public void Configure(EntityTypeBuilder<VaccinationReadModel> builder)
    {
        builder.ToTable("Vaccinations");

        builder.Property(vaccinationReadModel => vaccinationReadModel.Id)
            .ValueGeneratedNever();
    }
}
