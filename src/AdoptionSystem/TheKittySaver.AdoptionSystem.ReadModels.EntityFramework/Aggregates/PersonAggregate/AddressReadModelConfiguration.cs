using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.ReadModels.EntityFramework.Aggregates.PersonAggregate;

public sealed class AddressReadModelConfiguration : IEntityTypeConfiguration<AddressReadModel>
{
    public void Configure(EntityTypeBuilder<AddressReadModel> builder)
    {
        builder.ToTable("Addresses");

        builder.Property(addressReadModel => addressReadModel.Id)
            .ValueGeneratedNever();
    }
}
