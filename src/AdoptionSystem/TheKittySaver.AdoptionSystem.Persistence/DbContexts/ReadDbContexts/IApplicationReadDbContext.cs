using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Persistence.DbContexts.ReadDbContexts;

public interface IApplicationReadDbContext
{
    DbSet<PersonReadModel> Persons { get; }
    DbSet<AddressReadModel> Addresses { get; }
    DbSet<CatReadModel> Cats { get; }
    DbSet<CatThumbnailReadModel> CatThumbnails { get; }
    DbSet<CatGalleryItemReadModel> CatGalleryItems { get; }
    DbSet<VaccinationReadModel> Vaccinations { get; }
    DbSet<AdoptionAnnouncementReadModel> AdoptionAnnouncements { get; }
}
