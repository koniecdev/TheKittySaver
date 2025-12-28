using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.ReadModels.EntityFramework.Aggregates.CatAggregate;

public sealed class CatThumbnailReadModelConfiguration : IEntityTypeConfiguration<CatThumbnailReadModel>
{
    public void Configure(EntityTypeBuilder<CatThumbnailReadModel> builder)
    {
        builder.ToTable("CatThumbnails");

        builder.Property(catThumbnailReadModel => catThumbnailReadModel.Id)
            .ValueGeneratedNever();

        builder.HasQueryFilter(catThumbnailReadModel => catThumbnailReadModel.Cat.ArchivedAt == null);
    }
}
