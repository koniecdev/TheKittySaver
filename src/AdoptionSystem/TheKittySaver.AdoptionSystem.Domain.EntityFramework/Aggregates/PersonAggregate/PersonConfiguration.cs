using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;

namespace TheKittySaver.AdoptionSystem.Domain.EntityFramework.Aggregates.PersonAggregate;

public sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("Persons");

        builder.Property(x => x.Id)
            .ValueGeneratedNever();
        
        builder.Property(x => x.IdentityId)
            .ValueGeneratedNever();
        
        builder.ComplexProperty(x => x.Username, complexBuilder =>
        {
            complexBuilder.IsRequired();
            complexBuilder.Property(x => x.Value)
                .HasColumnName(nameof(Person.Username))
                .HasMaxLength(Username.MaxLength);
        });
        
        builder.ComplexProperty(x => x.Email, complexBuilder =>
        {
            complexBuilder.IsRequired();
            complexBuilder.Property(x => x.Value)
                .HasColumnName(nameof(Person.Email))
                .HasMaxLength(Email.MaxLength);
        });
        
        builder.ComplexProperty(x => x.PhoneNumber, complexBuilder =>
        {
            complexBuilder.IsRequired();
            complexBuilder.Property(x => x.Value)
                .HasColumnName(nameof(Person.PhoneNumber))
                .HasMaxLength(PhoneNumber.MaxLength);
        });
        
        builder.HasMany(x=>x.Addresses)
            .WithOne()
            .HasForeignKey(x=>x.PersonId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany<Cat>()
            .WithOne()
            .HasForeignKey(x=>x.PersonId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany<AdoptionAnnouncement>()
            .WithOne()
            .HasForeignKey(x=>x.PersonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
