using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.AdoptionAnnouncementAggregate;

namespace TheKittySaver.AdoptionSystem.ReadModels.EntityFramework.Aggregates.AdoptionAnnouncementAggregate;

public sealed class AdoptionAnnouncementReadModelConfiguration : IEntityTypeConfiguration<AdoptionAnnouncementReadModel>
{
    public void Configure(EntityTypeBuilder<AdoptionAnnouncementReadModel> builder)
    {
        builder.ToTable("AdoptionAnnouncements");

        builder.Property(adoptionAnnouncementReadModel => adoptionAnnouncementReadModel.Id)
            .ValueGeneratedNever();

        builder.OwnsMany(adoptionAnnouncementReadModel => adoptionAnnouncementReadModel.MergeLogs, mergeLogBuilder =>
        {
            mergeLogBuilder.ToJson();

            mergeLogBuilder.Property(adoptionAnnouncementMergeLogReadModel => adoptionAnnouncementMergeLogReadModel.MergedAdoptionAnnouncementId);
        });
    }
}
