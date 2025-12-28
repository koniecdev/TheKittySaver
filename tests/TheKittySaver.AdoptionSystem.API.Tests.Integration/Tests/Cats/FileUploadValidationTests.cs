// using System.Net;
// using System.Net.Http.Headers;
// using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
// using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
// using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
//
// namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Cats;
//
// public sealed class FileUploadValidationTests : CatEndpointsTestBase
// {
//     public FileUploadValidationTests(TheKittySaverApiFactory appFactory) : base(appFactory)
//     {
//     }
//
//     [Fact]
//     public async Task UploadGalleryItem_WithValidPng_ShouldSucceed()
//     {
//         CatId catId = await CatApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, TestPersonId);
//         byte[] imageBytes = ImageContentFactory.CreateMinimalPng();
//
//         using MultipartFormDataContent content = new();
//         using ByteArrayContent fileContent = new(imageBytes);
//         fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
//         content.Add(fileContent, "file", "test-image.png");
//
//         HttpResponseMessage response = await ApiClient.Http.PostAsync(
//             new Uri($"api/v1/cats/{catId}/gallery", UriKind.Relative),
//             content);
//
//         Assert.Equal(HttpStatusCode.Created, response.StatusCode);
//     }
//
//     [Fact]
//     public async Task UploadGalleryItem_WithValidJpeg_ShouldSucceed()
//     {
//         CatId catId = await CatApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, TestPersonId);
//         byte[] imageBytes = ImageContentFactory.CreateMinimalJpeg();
//
//         using MultipartFormDataContent content = new();
//         using ByteArrayContent fileContent = new(imageBytes);
//         fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
//         content.Add(fileContent, "file", "test-image.jpg");
//
//         HttpResponseMessage response = await ApiClient.Http.PostAsync(
//             new Uri($"api/v1/cats/{catId}/gallery", UriKind.Relative),
//             content);
//
//         Assert.Equal(HttpStatusCode.Created, response.StatusCode);
//     }
//
//     [Fact]
//     public async Task UploadGalleryItem_WithValidGif_ShouldSucceed()
//     {
//         CatId catId = await CatApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, TestPersonId);
//         byte[] imageBytes = ImageContentFactory.CreateMinimalGif();
//
//         using MultipartFormDataContent content = new();
//         using ByteArrayContent fileContent = new(imageBytes);
//         fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/gif");
//         content.Add(fileContent, "file", "test-image.gif");
//
//         HttpResponseMessage response = await ApiClient.Http.PostAsync(
//             new Uri($"api/v1/cats/{catId}/gallery", UriKind.Relative),
//             content);
//
//         Assert.Equal(HttpStatusCode.Created, response.StatusCode);
//     }
//
//     [Fact]
//     public async Task UploadGalleryItem_WithEmptyFile_ShouldReturnBadRequest()
//     {
//         CatId catId = await CatApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, TestPersonId);
//         byte[] imageBytes = ImageContentFactory.CreateEmptyFile();
//
//         using MultipartFormDataContent content = new();
//         using ByteArrayContent fileContent = new(imageBytes);
//         fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
//         content.Add(fileContent, "file", "test-image.png");
//
//         HttpResponseMessage response = await ApiClient.Http.PostAsync(
//             new Uri($"api/v1/cats/{catId}/gallery", UriKind.Relative),
//             content);
//
//         Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
//         string responseContent = await response.Content.ReadAsStringAsync();
//         Assert.Contains("FileUpload.EmptyFile", responseContent);
//     }
//
//     [Fact]
//     public async Task UploadGalleryItem_WithInvalidContentType_ShouldReturnBadRequest()
//     {
//         CatId catId = await CatApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, TestPersonId);
//         byte[] imageBytes = ImageContentFactory.CreateMinimalPng();
//
//         using MultipartFormDataContent content = new();
//         using ByteArrayContent fileContent = new(imageBytes);
//         fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
//         content.Add(fileContent, "file", "test-document.pdf");
//
//         HttpResponseMessage response = await ApiClient.Http.PostAsync(
//             new Uri($"api/v1/cats/{catId}/gallery", UriKind.Relative),
//             content);
//
//         Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
//         string responseContent = await response.Content.ReadAsStringAsync();
//         Assert.Contains("FileUpload.InvalidContentType", responseContent);
//     }
//
//     [Fact]
//     public async Task UploadGalleryItem_WithInvalidExtension_ShouldReturnBadRequest()
//     {
//         CatId catId = await CatApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, TestPersonId);
//         byte[] imageBytes = ImageContentFactory.CreateMinimalPng();
//
//         using MultipartFormDataContent content = new();
//         using ByteArrayContent fileContent = new(imageBytes);
//         fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
//         content.Add(fileContent, "file", "test-image.exe");
//
//         HttpResponseMessage response = await ApiClient.Http.PostAsync(
//             new Uri($"api/v1/cats/{catId}/gallery", UriKind.Relative),
//             content);
//
//         Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
//         string responseContent = await response.Content.ReadAsStringAsync();
//         Assert.Contains("FileUpload.InvalidFileExtension", responseContent);
//     }
//
//     [Fact]
//     public async Task UploadGalleryItem_WithMismatchedContentTypeAndMagicBytes_ShouldReturnBadRequest()
//     {
//         CatId catId = await CatApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, TestPersonId);
//         byte[] imageBytes = ImageContentFactory.CreateMinimalPng();
//
//         using MultipartFormDataContent content = new();
//         using ByteArrayContent fileContent = new(imageBytes);
//         fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
//         content.Add(fileContent, "file", "test-image.jpg");
//
//         HttpResponseMessage response = await ApiClient.Http.PostAsync(
//             new Uri($"api/v1/cats/{catId}/gallery", UriKind.Relative),
//             content);
//
//         Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
//         string responseContent = await response.Content.ReadAsStringAsync();
//         Assert.Contains("FileUpload", responseContent);
//     }
//
//     [Fact]
//     public async Task UploadGalleryItem_WithFakeImageFile_ShouldReturnBadRequest()
//     {
//         CatId catId = await CatApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, TestPersonId);
//         byte[] fakeImageBytes = ImageContentFactory.CreateInvalidFileWithFakeExtension();
//
//         using MultipartFormDataContent content = new();
//         using ByteArrayContent fileContent = new(fakeImageBytes);
//         fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
//         content.Add(fileContent, "file", "fake-image.png");
//
//         HttpResponseMessage response = await ApiClient.Http.PostAsync(
//             new Uri($"api/v1/cats/{catId}/gallery", UriKind.Relative),
//             content);
//
//         Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
//         string responseContent = await response.Content.ReadAsStringAsync();
//         Assert.Contains("FileUpload", responseContent);
//     }
//
//     [Fact]
//     public async Task UploadGalleryItem_WithPathTraversalInFileName_ShouldReturnBadRequest()
//     {
//         CatId catId = await CatApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, TestPersonId);
//         byte[] imageBytes = ImageContentFactory.CreateMinimalPng();
//
//         using MultipartFormDataContent content = new();
//         using ByteArrayContent fileContent = new(imageBytes);
//         fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
//         content.Add(fileContent, "file", "../../../etc/passwd.png");
//
//         HttpResponseMessage response = await ApiClient.Http.PostAsync(
//             new Uri($"api/v1/cats/{catId}/gallery", UriKind.Relative),
//             content);
//
//         Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
//         string responseContent = await response.Content.ReadAsStringAsync();
//         Assert.Contains("FileUpload.PathTraversalDetected", responseContent);
//     }
//
//     [Fact]
//     public async Task UploadGalleryItem_WithDoubleExtension_ShouldReturnBadRequest()
//     {
//         CatId catId = await CatApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, TestPersonId);
//         byte[] imageBytes = ImageContentFactory.CreateMinimalPng();
//
//         using MultipartFormDataContent content = new();
//         using ByteArrayContent fileContent = new(imageBytes);
//         fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
//         content.Add(fileContent, "file", "image.php.png");
//
//         HttpResponseMessage response = await ApiClient.Http.PostAsync(
//             new Uri($"api/v1/cats/{catId}/gallery", UriKind.Relative),
//             content);
//
//         Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
//         string responseContent = await response.Content.ReadAsStringAsync();
//         Assert.Contains("FileUpload.DoubleExtension", responseContent);
//     }
//
//     [Fact]
//     public async Task UploadGalleryItem_WithExtensionContentTypeMismatch_ShouldReturnBadRequest()
//     {
//         CatId catId = await CatApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, TestPersonId);
//         byte[] imageBytes = ImageContentFactory.CreateMinimalPng();
//
//         using MultipartFormDataContent content = new();
//         using ByteArrayContent fileContent = new(imageBytes);
//         fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
//         content.Add(fileContent, "file", "test-image.jpg");
//
//         HttpResponseMessage response = await ApiClient.Http.PostAsync(
//             new Uri($"api/v1/cats/{catId}/gallery", UriKind.Relative),
//             content);
//
//         Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
//         string responseContent = await response.Content.ReadAsStringAsync();
//         Assert.Contains("FileUpload.ExtensionContentTypeMismatch", responseContent);
//     }
//
//     [Fact]
//     public async Task UploadThumbnail_WithImageTooSmall_ShouldReturnBadRequest()
//     {
//         CatId catId = await CatApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, TestPersonId);
//         byte[] imageBytes = ImageContentFactory.CreateMinimalPng();
//
//         using MultipartFormDataContent content = new();
//         using ByteArrayContent fileContent = new(imageBytes);
//         fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
//         content.Add(fileContent, "file", "tiny-thumbnail.png");
//
//         HttpResponseMessage response = await ApiClient.Http.PutAsync(
//             new Uri($"api/v1/cats/{catId}/thumbnail", UriKind.Relative),
//             content);
//
//         Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
//         string responseContent = await response.Content.ReadAsStringAsync();
//         Assert.Contains("FileUpload.ImageTooSmall", responseContent);
//     }
//
//     [Fact]
//     public async Task UploadThumbnail_WithValidSizedImage_ShouldSucceed()
//     {
//         CatId catId = await CatApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, TestPersonId);
//         byte[] imageBytes = ImageContentFactory.CreatePngWithDimensions(200, 200);
//
//         using MultipartFormDataContent content = new();
//         using ByteArrayContent fileContent = new(imageBytes);
//         fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
//         content.Add(fileContent, "file", "valid-thumbnail.png");
//
//         HttpResponseMessage response = await ApiClient.Http.PutAsync(
//             new Uri($"api/v1/cats/{catId}/thumbnail", UriKind.Relative),
//             content);
//
//         Assert.Equal(HttpStatusCode.OK, response.StatusCode);
//     }
// }
