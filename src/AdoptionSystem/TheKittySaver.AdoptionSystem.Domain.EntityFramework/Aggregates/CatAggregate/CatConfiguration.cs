using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.EntityFramework.Consts;

namespace TheKittySaver.AdoptionSystem.Domain.EntityFramework.Aggregates.CatAggregate;

public sealed class CatConfiguration : IEntityTypeConfiguration<Cat>
{
    public void Configure(EntityTypeBuilder<Cat> builder)
    {
        builder.ToTable("Cats");
        
        builder.Property(x => x.Id)
            .ValueGeneratedNever();
        
        builder.ComplexProperty(x => x.ClaimedAt, complexBuilder =>
        {
            complexBuilder.IsRequired(false);
            complexBuilder.Property(x => x.Value)
                .HasColumnName(nameof(Cat.ClaimedAt));
        });
        
        builder.ComplexProperty(x => x.PublishedAt, complexBuilder =>
        {
            complexBuilder.IsRequired(false);
            complexBuilder.Property(x => x.Value)
                .HasColumnName(nameof(Cat.PublishedAt));
        });
        
        builder.ComplexProperty(x => x.Name, complexBuilder =>
        {
            complexBuilder.IsRequired();
            complexBuilder.Property(x => x.Value)
                .HasColumnName(nameof(Cat.Name))
                .HasMaxLength(CatName.MaxLength);
        });
        
        builder.ComplexProperty(x => x.Description, complexBuilder =>
        {
            complexBuilder.IsRequired();
            complexBuilder.Property(x => x.Value)
                .HasColumnName(nameof(Cat.Description))
                .HasMaxLength(CatDescription.MaxLength);
        });
        
        builder.ComplexProperty(x => x.Age, complexBuilder =>
        {
            complexBuilder.IsRequired();
            complexBuilder.Property(x => x.Value)
                .HasColumnName(nameof(Cat.Age));
        });
        
        builder.ComplexProperty(x => x.Gender, complexBuilder =>
        {
            complexBuilder.IsRequired();
            complexBuilder.Property(x => x.Value)
                .HasColumnName(nameof(Cat.Gender))
                .HasConversion<string>()
                .HasMaxLength(EnumConsts.MaxLength);
        });
        
        builder.ComplexProperty(x => x.Color, complexBuilder =>
        {
            complexBuilder.IsRequired();
            complexBuilder.Property(x => x.Value)
                .HasColumnName(nameof(Cat.Color))
                .HasConversion<string>()
                .HasMaxLength(EnumConsts.MaxLength);
        });
        
        builder.ComplexProperty(x => x.Weight, complexBuilder =>
        {
            complexBuilder.IsRequired();
            complexBuilder.Property(x => x.ValueInKilograms)
                .HasColumnName($"{nameof(Cat.Weight)}{nameof(CatWeight.ValueInKilograms)}")
                .HasPrecision(5, 2);
        });
        
        builder.ComplexProperty(x => x.HealthStatus, complexBuilder =>
        {
            complexBuilder.IsRequired();
            complexBuilder.Property(x => x.Value)
                .HasColumnName(nameof(Cat.HealthStatus))
                .HasConversion<string>()
                .HasMaxLength(EnumConsts.MaxLength);
        });
        
        builder.ComplexProperty(x => x.SpecialNeeds, complexBuilder =>
        {
            complexBuilder.IsRequired();
            
            const string prefix = nameof(SpecialNeedsStatus);
            
            complexBuilder.Property(x => x.HasSpecialNeeds)
                .HasColumnName($"{prefix}{nameof(SpecialNeedsStatus.HasSpecialNeeds)}");
            
            complexBuilder.Property(x => x.Description)
                .HasColumnName($"{prefix}{nameof(SpecialNeedsStatus.Description)}")
                .HasMaxLength(SpecialNeedsStatus.MaxDescriptionLength);
            
            complexBuilder.Property(x => x.SeverityType)
                .HasColumnName($"{prefix}{nameof(SpecialNeedsStatus.SeverityType)}")
                .HasConversion<string>()
                .HasMaxLength(EnumConsts.MaxLength);
        });
 
        builder.ComplexProperty(x => x.Temperament, complexBuilder =>
        {
            complexBuilder.IsRequired();
            complexBuilder.Property(x => x.Value)
                .HasColumnName(nameof(Cat.Temperament))
                .HasConversion<string>()
                .HasMaxLength(EnumConsts.MaxLength);
        });
        
        builder.ComplexProperty(x => x.AdoptionHistory, complexBuilder =>
        {
            const string prefix = nameof(Cat.AdoptionHistory);

            complexBuilder.IsRequired();

            complexBuilder.Property(x => x.ReturnCount)
                .HasColumnName($"{prefix}{nameof(AdoptionHistory.ReturnCount)}");

            complexBuilder.Property(x => x.LastReturnDate)
                .HasColumnName($"{prefix}{nameof(AdoptionHistory.LastReturnDate)}");

            complexBuilder.Property(x => x.LastReturnReason)
                .HasColumnName($"{prefix}{nameof(AdoptionHistory.LastReturnReason)}")
                .HasMaxLength(AdoptionHistory.LastReturnReasonMaxLength);
        });
        
        builder.ComplexProperty(x => x.ListingSource, complexBuilder =>
        {
            const string prefix = nameof(Cat.ListingSource);

            complexBuilder.IsRequired();

            complexBuilder.Property(x => x.Type)
                .HasColumnName($"{prefix}{nameof(ListingSource.Type)}")
                .HasConversion<string>()
                .HasMaxLength(EnumConsts.MaxLength);

            complexBuilder.Property(x => x.SourceName)
                .HasColumnName($"{prefix}{nameof(ListingSource.SourceName)}")
                .HasMaxLength(ListingSource.MaxSourceNameLength);
        });

        builder.ComplexProperty(x => x.NeuteringStatus, complexBuilder =>
        {
            const string prefix = nameof(Cat.NeuteringStatus);

            complexBuilder.IsRequired();

            complexBuilder.Property(x => x.IsNeutered)
                .HasColumnName($"{prefix}{nameof(NeuteringStatus.IsNeutered)}");
        });
        
        builder.ComplexProperty(x => x.InfectiousDiseaseStatus, complexBuilder =>
        {
            const string prefix = nameof(Cat.InfectiousDiseaseStatus);

            complexBuilder.IsRequired();

            complexBuilder.Property(x => x.FivStatus)
                .HasColumnName($"{prefix}{nameof(InfectiousDiseaseStatus.FivStatus)}")
                .HasConversion<string>()
                .HasMaxLength(EnumConsts.MaxLength);

            complexBuilder.Property(x => x.FelvStatus)
                .HasColumnName($"{prefix}{nameof(InfectiousDiseaseStatus.FelvStatus)}")
                .HasConversion<string>()
                .HasMaxLength(EnumConsts.MaxLength);

            complexBuilder.Property(x => x.LastTestedAt)
                .HasColumnName($"{prefix}{nameof(InfectiousDiseaseStatus.LastTestedAt)}");
        });
        
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(EnumConsts.MaxLength);
        
        builder.HasOne(x=>x.Thumbnail)
            .WithOne()
            .HasForeignKey<CatThumbnail>(x => x.CatId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(x=>x.GalleryItems)
            .WithOne()
            .HasForeignKey(x => x.CatId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(x=>x.Vaccinations)
            .WithOne()
            .HasForeignKey(x => x.CatId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
