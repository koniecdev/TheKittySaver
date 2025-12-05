using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.EntityFramework.Aggregates.AdoptionAnnouncementAggregate;

public sealed class AdoptionAnnouncementConfiguration : IEntityTypeConfiguration<AdoptionAnnouncement>
{
    public void Configure(EntityTypeBuilder<AdoptionAnnouncement> builder)
    {
        builder.ToTable("AdoptionAnnouncements");

        builder.Property(x => x.PersonId);
        builder.Property(x => x.Status);
        builder.Property(x => x.MergeLogs);

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
            complexBuilder.IsRequired();

            complexBuilder.Property(x => x.CountryCode)
                .HasColumnName("Address_CountryCode");

            complexBuilder.ComplexProperty(x => x.PostalCode, nestedBuilder =>
            {
                nestedBuilder.IsRequired();
                nestedBuilder.Property(x => x.Value)
                    .HasColumnName("Address_PostalCode")
                    .HasMaxLength(AddressPostalCode.MaxLength);
            });

            complexBuilder.ComplexProperty(x => x.Region, nestedBuilder =>
            {
                nestedBuilder.IsRequired();
                nestedBuilder.Property(x => x.Value)
                    .HasColumnName("Address_Region")
                    .HasMaxLength(AddressRegion.MaxLength);
            });

            complexBuilder.ComplexProperty(x => x.City, nestedBuilder =>
            {
                nestedBuilder.IsRequired();
                nestedBuilder.Property(x => x.Value)
                    .HasColumnName("Address_City")
                    .HasMaxLength(AddressCity.MaxLength);
            });

            complexBuilder.ComplexProperty(x => x.Line, nestedBuilder =>
            {
                nestedBuilder.IsRequired(false);
                nestedBuilder.Property(x => x.Value)
                    .HasColumnName("Address_Line")
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
    }
}
