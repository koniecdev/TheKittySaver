using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds;

namespace TheKittySaver.AdoptionSystem.Domain.EntityFramework.Aggregates.PersonAggregate;

public sealed class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses");

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        EntityConfiguration.ConfigureCreatedAt(builder);

        builder.Property(x => x.CountryCode);

        builder.ComplexProperty(x => x.Name, complexBuilder =>
        {
            complexBuilder.IsRequired();
            complexBuilder.Property(x => x.Value)
                .HasColumnName(nameof(Address.Name))
                .HasMaxLength(AddressName.MaxLength);
        });

        builder.ComplexProperty(x => x.PostalCode, complexBuilder =>
        {
            complexBuilder.IsRequired();
            complexBuilder.Property(x => x.Value)
                .HasColumnName(nameof(Address.PostalCode))
                .HasMaxLength(AddressPostalCode.MaxLength);
        });

        builder.ComplexProperty(x => x.Region, complexBuilder =>
        {
            complexBuilder.IsRequired();
            complexBuilder.Property(x => x.Value)
                .HasColumnName(nameof(Address.Region))
                .HasMaxLength(AddressRegion.MaxLength);
        });

        builder.ComplexProperty(x => x.City, complexBuilder =>
        {
            complexBuilder.IsRequired();
            complexBuilder.Property(x => x.Value)
                .HasColumnName(nameof(Address.City))
                .HasMaxLength(AddressCity.MaxLength);
        });

        builder.ComplexProperty(x => x.Line, complexBuilder =>
        {
            complexBuilder.IsRequired(false);
            complexBuilder.Property(x => x.Value)
                .HasColumnName(nameof(Address.Line))
                .HasMaxLength(AddressLine.MaxLength);
        });
    }
}
