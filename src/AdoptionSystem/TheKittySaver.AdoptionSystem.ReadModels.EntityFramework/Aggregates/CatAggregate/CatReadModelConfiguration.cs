using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.ReadModels.EntityFramework.Aggregates.CatAggregate;

public sealed class CatReadModelConfiguration : IEntityTypeConfiguration<CatReadModel>
{
    public void Configure(EntityTypeBuilder<CatReadModel> builder)
    {
        builder.ToTable("Cats");

        builder.Property(catReadModel => catReadModel.Id)
            .ValueGeneratedNever();

        builder.HasOne(catReadModel => catReadModel.AdoptionAnnouncement)
            .WithMany(adoptionAnnouncementReadModel => adoptionAnnouncementReadModel.Cats)
            .HasForeignKey(catReadModel => catReadModel.AdoptionAnnouncementId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(catReadModel => catReadModel.Thumbnail)
            .WithOne()
            .HasForeignKey<CatThumbnailReadModel>(catThumbnailReadModel => catThumbnailReadModel.CatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(catReadModel => catReadModel.GalleryItems)
            .WithOne()
            .HasForeignKey(catGalleryItemReadModel => catGalleryItemReadModel.CatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(catReadModel => catReadModel.Vaccinations)
            .WithOne()
            .HasForeignKey(vaccinationReadModel => vaccinationReadModel.CatId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
