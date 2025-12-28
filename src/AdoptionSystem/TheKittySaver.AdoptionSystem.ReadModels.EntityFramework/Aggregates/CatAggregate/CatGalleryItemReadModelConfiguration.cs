using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.ReadModels.EntityFramework.Aggregates.CatAggregate;

public sealed class CatGalleryItemReadModelConfiguration : IEntityTypeConfiguration<CatGalleryItemReadModel>
{
    public void Configure(EntityTypeBuilder<CatGalleryItemReadModel> builder)
    {
        builder.ToTable("CatGalleryItems");

        builder.Property(catGalleryItemReadModel => catGalleryItemReadModel.Id)
            .ValueGeneratedNever();

        builder.HasQueryFilter(catGalleryItemReadModel => catGalleryItemReadModel.Cat.ArchivedAt == null);
    }
}
