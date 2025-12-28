using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.EntityFramework;
using TheKittySaver.AdoptionSystem.Persistence.Converters;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;

namespace TheKittySaver.AdoptionSystem.Persistence.DbContexts.WriteDbContexts;

internal sealed class ApplicationWriteDbContext : DbContext, IUnitOfWork
{
    public ApplicationWriteDbContext(DbContextOptions<ApplicationWriteDbContext> options) : base(options)
    {
    }

    public DbSet<Person> Persons => Set<Person>();
    public DbSet<Cat> Cats => Set<Cat>();
    public DbSet<AdoptionAnnouncement> AdoptionAnnouncements => Set<AdoptionAnnouncement>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.RegisterAllStronglyTypedIdConverters();
        base.ConfigureConventions(configurationBuilder);
    }

    /// <inheritdoc />
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        => Database.BeginTransactionAsync(cancellationToken);

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IDomainEntityFrameworkAssemblyReference).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
