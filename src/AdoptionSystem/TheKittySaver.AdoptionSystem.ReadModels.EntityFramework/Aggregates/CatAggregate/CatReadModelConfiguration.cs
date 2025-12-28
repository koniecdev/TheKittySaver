using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.ReadModels.EntityFramework.Aggregates.CatAggregate;

public sealed class CatReadModelConfiguration : IEntityTypeConfiguration<CatReadModel>
{
    public void Configure(EntityTypeBuilder<CatReadModel> builder)
    {
        builder.ToTable("Cats");

        builder.Property(catReadModel => catReadModel.Id)
            .ValueGeneratedNever();

        builder.Property(catReadModel => catReadModel.Gender)
            .HasConversion<string>();

        builder.Property(catReadModel => catReadModel.Color)
            .HasConversion<string>();

        builder.Property(catReadModel => catReadModel.HealthStatus)
            .HasConversion<string>();

        builder.Property(catReadModel => catReadModel.SpecialNeedsStatusSeverityType)
            .HasConversion<string>();

        builder.Property(catReadModel => catReadModel.Temperament)
            .HasConversion<string>();

        builder.Property(catReadModel => catReadModel.ListingSourceType)
            .HasConversion<string>();

        builder.Property(catReadModel => catReadModel.InfectiousDiseaseStatusFivStatus)
            .HasConversion<string>();

        builder.Property(catReadModel => catReadModel.InfectiousDiseaseStatusFelvStatus)
            .HasConversion<string>();

        builder.Property(catReadModel => catReadModel.Status)
            .HasConversion<string>();

        builder.Property(x => x.WeightValueInKilograms)
            .HasPrecision(5, 2);

        builder.HasQueryFilter(catReadModel => catReadModel.ArchivedAt == null);

        builder.HasOne(catReadModel => catReadModel.AdoptionAnnouncement)
            .WithMany(adoptionAnnouncementReadModel => adoptionAnnouncementReadModel.Cats)
            .HasForeignKey(catReadModel => catReadModel.AdoptionAnnouncementId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(catReadModel => catReadModel.Thumbnail)
            .WithOne(catThumbnailReadModel => catThumbnailReadModel.Cat)
            .HasForeignKey<CatThumbnailReadModel>(catThumbnailReadModel => catThumbnailReadModel.CatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(catReadModel => catReadModel.GalleryItems)
            .WithOne(catGalleryItemReadModel => catGalleryItemReadModel.Cat)
            .HasForeignKey(catGalleryItemReadModel => catGalleryItemReadModel.CatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(catReadModel => catReadModel.Vaccinations)
            .WithOne(vaccinationReadModel => vaccinationReadModel.Cat)
            .HasForeignKey(vaccinationReadModel => vaccinationReadModel.CatId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
