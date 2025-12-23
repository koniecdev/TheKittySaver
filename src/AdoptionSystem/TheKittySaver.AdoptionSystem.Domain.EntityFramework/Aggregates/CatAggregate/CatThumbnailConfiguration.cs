using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;

namespace TheKittySaver.AdoptionSystem.Domain.EntityFramework.Aggregates.CatAggregate;

public sealed class CatThumbnailConfiguration : IEntityTypeConfiguration<CatThumbnail>
{
    public void Configure(EntityTypeBuilder<CatThumbnail> builder)
    {
        builder.ToTable("CatThumbnails");
        
        builder.Property(x => x.Id)
            .ValueGeneratedNever();
        
        EntityConfiguration.ConfigureCreatedAt(builder);
    }
}
