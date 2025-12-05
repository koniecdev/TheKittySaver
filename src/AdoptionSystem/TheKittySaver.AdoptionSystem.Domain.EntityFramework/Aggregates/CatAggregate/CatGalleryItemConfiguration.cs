using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

namespace TheKittySaver.AdoptionSystem.Domain.EntityFramework.Aggregates.CatAggregate;

public sealed class CatGalleryItemConfiguration : IEntityTypeConfiguration<CatGalleryItem>
{
    public void Configure(EntityTypeBuilder<CatGalleryItem> builder)
    {
        builder.ToTable("CatGalleryItems");
        
        builder.Property(x => x.Id)
            .ValueGeneratedNever();
        
        builder.ComplexProperty(x => x.DisplayOrder, complexBuilder =>
        {
            complexBuilder.IsRequired();
            complexBuilder.Property(x => x.Value)
                .HasColumnName(nameof(CatGalleryItem.DisplayOrder));
        });
    }
}
