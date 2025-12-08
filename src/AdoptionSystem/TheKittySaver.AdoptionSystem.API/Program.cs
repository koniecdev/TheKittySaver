using System.Reflection;
using TheKittySaver.AdoptionSystem.API;
using TheKittySaver.AdoptionSystem.API.Extensions;
using Asp.Versioning;
using Asp.Versioning.Builder;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.Register(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();


builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

ApiVersionSet apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .ReportApiVersions()
    .Build();
RouteGroupBuilder versionedGroup = app
    .MapGroup("api/v{apiVersion:apiVersion}")
    .WithApiVersionSet(apiVersionSet);

app.MapEndpoints(versionedGroup);

await app.RunAsync();
