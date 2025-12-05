using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.EntityFramework.Aggregates.CatAggregate;

public sealed class CatGalleryItemConfiguration : IEntityTypeConfiguration<CatGalleryItem>
{
    public void Configure(EntityTypeBuilder<CatGalleryItem> builder)
    {
        builder.ToTable("CatGalleryItems");
    }
}
