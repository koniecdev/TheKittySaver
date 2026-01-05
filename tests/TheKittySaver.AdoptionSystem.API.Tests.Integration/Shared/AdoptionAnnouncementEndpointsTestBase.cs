// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Options;
// using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
// using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
// using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
// using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;
//
// namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
//
// [Collection("Api")]
// internal abstract class AdoptionAnnouncementEndpointsTestBase : AsyncLifetimeTestBase
// {
//     protected override TestApiClient ApiClient { get; }
//
//     protected AdoptionAnnouncementEndpointsTestBase(TheKittySaverApiFactory appFactory)
//     {
//         JsonSerializerOptions jsonSerializerOptions =
//             appFactory.Services.GetRequiredService<IOptionsSnapshot<JsonOptions>>().Value.SerializerOptions;
//
//         ApiClient = new TestApiClient(appFactory.CreateClient(), jsonSerializerOptions);
//     }
//
//     public override async Task InitializeAsync()
//     {
//         await base.InitializeAsync();
//         _ = await CatGalleryApiFactory.UpsertRandomThumbnailAsync(ApiClient);
//     }
// }
