using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.ReadModels.EntityFramework.Aggregates.PersonAggregate;

public sealed class PersonReadModelConfiguration : IEntityTypeConfiguration<PersonReadModel>
{
    public void Configure(EntityTypeBuilder<PersonReadModel> builder)
    {
        builder.ToTable("Persons");

        builder.Property(personReadModel => personReadModel.Id)
            .ValueGeneratedNever();

        builder.HasQueryFilter(x => x.ArchivedAt == null);

        builder.HasMany(personReadModel => personReadModel.Addresses)
            .WithOne(addressReadModel => addressReadModel.Person)
            .HasForeignKey(addressReadModel => addressReadModel.PersonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<CatReadModel>()
            .WithOne()
            .HasForeignKey(catReadModel => catReadModel.PersonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<AdoptionAnnouncementReadModel>()
            .WithOne(adoptionAnnouncementReadModel => adoptionAnnouncementReadModel.Person)
            .HasForeignKey(adoptionAnnouncementReadModel => adoptionAnnouncementReadModel.PersonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
