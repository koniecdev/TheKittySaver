using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Persistence.Converters;

public static class StronglyTypedIdsConverters
{
    public static ModelConfigurationBuilder RegisterAllStronglyTypedIdConverters(
        this ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<PersonId>()
            .HaveConversion<PersonIdConverter>();
        
        configurationBuilder
            .Properties<CatId>()
            .HaveConversion<CatIdConverter>();
        
        configurationBuilder
            .Properties<IdentityId>()
            .HaveConversion<IdentityIdConverter>();
        
        configurationBuilder
            .Properties<AddressId>()
            .HaveConversion<AddressIdConverter>();
        
        configurationBuilder
            .Properties<CatGalleryItemId>()
            .HaveConversion<CatGalleryItemIdConverter>();
        
        configurationBuilder
            .Properties<CatThumbnailId>()
            .HaveConversion<CatThumbnailIdConverter>();
        
        configurationBuilder
            .Properties<VaccinationId>()
            .HaveConversion<VaccinationIdConverter>();
        
        configurationBuilder
            .Properties<AdoptionAnnouncementId>()
            .HaveConversion<AdoptionAnnouncementIdConverter>();

        return configurationBuilder;
    }

    private sealed class PersonIdConverter() : ValueConverter<PersonId, Guid>(
        id => id.Value,
        value => new PersonId(value));
    
    private sealed class IdentityIdConverter() : ValueConverter<IdentityId, Guid>(
        id => id.Value,
        value => new IdentityId(value));
    
    private sealed class AddressIdConverter() : ValueConverter<AddressId, Guid>(
        id => id.Value,
        value => new AddressId(value));

    private sealed class CatIdConverter() : ValueConverter<CatId, Guid>(
        id => id.Value,
        value => new CatId(value));

    private sealed class CatGalleryItemIdConverter() : ValueConverter<CatGalleryItemId, Guid>(
        id => id.Value,
        value => new CatGalleryItemId(value));
    
    private sealed class CatThumbnailIdConverter() : ValueConverter<CatThumbnailId, Guid>(
        id => id.Value,
        value => new CatThumbnailId(value));
    
    private sealed class VaccinationIdConverter() : ValueConverter<VaccinationId, Guid>(
        id => id.Value,
        value => new VaccinationId(value));
    
    private sealed class AdoptionAnnouncementIdConverter() : ValueConverter<AdoptionAnnouncementId, Guid>(
        id => id.Value,
        value => new AdoptionAnnouncementId(value));
}
