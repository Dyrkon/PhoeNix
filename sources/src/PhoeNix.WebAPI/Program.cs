using Carter;
using PhoeNix.Persistence.Seeding;
using PhoeNix.WebAPI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebApiHost(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await app.Services.SeedApplicationDataAsync();

app.UseWhen(
    context =>
    {
        var path = context.Request.Path;
        return !(path.StartsWithSegments("/v1/boot") || !path.StartsWithSegments("/provisioning/files/"));
    },
    appBuilder => { appBuilder.UseHttpsRedirection(); });

app.MapHealthChecks("/health");
app.UseCors("WebAppClient");
app.UseUserAuthentication();
app.MapCarter();

app.Run();