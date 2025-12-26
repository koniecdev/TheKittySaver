using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.EntityFramework.Consts;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;

namespace TheKittySaver.AdoptionSystem.Domain.EntityFramework.Aggregates.AdoptionAnnouncementAggregate;

public sealed class AdoptionAnnouncementConfiguration : IEntityTypeConfiguration<AdoptionAnnouncement>
{
    public void Configure(EntityTypeBuilder<AdoptionAnnouncement> builder)
    {
        builder.ToTable("AdoptionAnnouncements");
        
        builder.Property(x => x.Id)
            .ValueGeneratedNever();
        
        EntityConfiguration.ConfigureCreatedAt(builder);
        
        builder.ComplexProperty(x => x.ClaimedAt, complexBuilder =>
        {
            complexBuilder.IsRequired(false);
            complexBuilder.Property(x => x.Value)
                .HasColumnName(nameof(AdoptionAnnouncement.ClaimedAt));
        });

        builder.ComplexProperty(x => x.Description, complexBuilder =>
        {
            complexBuilder.IsRequired(false);
            complexBuilder.Property(x => x.Value)
                .HasColumnName(nameof(AdoptionAnnouncement.Description))
                .HasMaxLength(AdoptionAnnouncementDescription.MaxLength);
        });

        builder.ComplexProperty(x => x.Address, complexBuilder =>
        {
            const string prefix = nameof(AdoptionAnnouncement.Address);

            complexBuilder.IsRequired();

            complexBuilder.Property(x => x.CountryCode)
                .HasColumnName($"{prefix}{nameof(AdoptionAnnouncementAddress.CountryCode)}");

            complexBuilder.ComplexProperty(x => x.PostalCode, nestedBuilder =>
            {
                nestedBuilder.IsRequired();
                nestedBuilder.Property(x => x.Value)
                    .HasColumnName($"{prefix}{nameof(AdoptionAnnouncementAddress.PostalCode)}")
                    .HasMaxLength(AddressPostalCode.MaxLength);
            });

            complexBuilder.ComplexProperty(x => x.Region, nestedBuilder =>
            {
                nestedBuilder.IsRequired();
                nestedBuilder.Property(x => x.Value)
                    .HasColumnName($"{prefix}{nameof(AdoptionAnnouncementAddress.Region)}")
                    .HasMaxLength(AddressRegion.MaxLength);
            });

            complexBuilder.ComplexProperty(x => x.City, nestedBuilder =>
            {
                nestedBuilder.IsRequired();
                nestedBuilder.Property(x => x.Value)
                    .HasColumnName($"{prefix}{nameof(AdoptionAnnouncementAddress.City)}")
                    .HasMaxLength(AddressCity.MaxLength);
            });

            complexBuilder.ComplexProperty(x => x.Line, nestedBuilder =>
            {
                nestedBuilder.IsRequired(false);
                nestedBuilder.Property(x => x.Value)
                    .HasColumnName($"{prefix}{nameof(AdoptionAnnouncementAddress.Line)}")
                    .HasMaxLength(AddressLine.MaxLength);
            });
        });

        builder.ComplexProperty(x => x.Email, complexBuilder =>
        {
            complexBuilder.IsRequired();
            complexBuilder.Property(x => x.Value)
                .HasColumnName(nameof(AdoptionAnnouncement.Email))
                .HasMaxLength(Email.MaxLength);
        });

        builder.ComplexProperty(x => x.PhoneNumber, complexBuilder =>
        {
            complexBuilder.IsRequired();
            complexBuilder.Property(x => x.Value)
                .HasColumnName(nameof(AdoptionAnnouncement.PhoneNumber))
                .HasMaxLength(PhoneNumber.MaxLength);
        });

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(EnumConsts.MaxLength);
        
        builder.Ignore(x=>x.MergeLogs);
        builder.ComplexCollection<List<AdoptionAnnouncementMergeLog>, AdoptionAnnouncementMergeLog>("_mergeLogs", complexBuilder =>
        {
            complexBuilder.ToJson();
            complexBuilder.Property(x => x.MergedAdoptionAnnouncementId);
            complexBuilder.ComplexProperty(x => x.MergedAt, mergedAtBuilder =>
            {
                mergedAtBuilder.IsRequired();
                mergedAtBuilder.Property(x => x.Value)
                    .HasColumnName(nameof(AdoptionAnnouncementMergeLog.MergedAt));
            });
        });
        
        builder.HasMany<Cat>()
            .WithOne()
            .HasForeignKey(x=>x.AdoptionAnnouncementId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
