using TheKittySaver.AdoptionSystem.API;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.Register();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
await app.RunAsync();
