using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;

namespace TheKittySaver.AdoptionSystem.Domain.EntityFramework.Aggregates;

public static class EntityConfiguration
{
    public const string CreatedAt = nameof(CreatedAt);

    public static void ConfigureCreatedAt<T>(EntityTypeBuilder<T> builder) where T : class, IEntity
    {
        builder.Property<DateTimeOffset>(CreatedAt);
    }
}
