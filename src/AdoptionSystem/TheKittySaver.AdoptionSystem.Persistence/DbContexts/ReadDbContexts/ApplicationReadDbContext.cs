using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.ReadModels.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.ReadModels.EntityFramework;

namespace TheKittySaver.AdoptionSystem.Persistence.DbContexts.ReadDbContexts;

internal sealed class ApplicationReadDbContext : DbContext, IApplicationReadDbContext
{
    public ApplicationReadDbContext(DbContextOptions<ApplicationReadDbContext> options) : base(options)
    {
    }
    
    public DbSet<PersonReadModel> Persons => Set<PersonReadModel>();
    public DbSet<AddressReadModel> Addresses => Set<AddressReadModel>();
    public DbSet<CatReadModel> Cats => Set<CatReadModel>();
    public DbSet<CatThumbnailReadModel> CatThumbnails => Set<CatThumbnailReadModel>();
    public DbSet<CatGalleryItemReadModel> CatGalleryItems => Set<CatGalleryItemReadModel>();
    public DbSet<VaccinationReadModel> Vaccinations => Set<VaccinationReadModel>();
    public DbSet<AdoptionAnnouncementReadModel> AdoptionAnnouncements => Set<AdoptionAnnouncementReadModel>();
    public DbSet<AdoptionAnnouncementMergeLogReadModel> AdoptionAnnouncementMergeLogs => Set<AdoptionAnnouncementMergeLogReadModel>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IReadModelsEntityFrameworkAssemblyReference).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}
