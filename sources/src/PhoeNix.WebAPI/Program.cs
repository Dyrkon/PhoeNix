using System.Net;
using Carter;
using Microsoft.AspNetCore.HttpOverrides;
using PhoeNix.Persistence.Seeding;
using PhoeNix.WebAPI;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddWebApiHost(builder.Configuration);
var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    KnownNetworks = { new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Any, 0) },
    KnownProxies = { IPAddress.Parse("127.0.0.1") }
});

app.UsePathBase("/api");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("v1/swagger.json", "PhoeNix API V1"); });
}

await app.Services.SeedApplicationDataAsync();
app.MapHealthChecks("/health");
app.UseCors("WebAppClient");
app.UseUserAuthentication();
app.MapCarter();
app.Run();