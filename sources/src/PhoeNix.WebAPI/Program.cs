using Carter;
using Microsoft.EntityFrameworkCore;
using PhoeNix.Persistence;
using PhoeNix.WebAPI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebApiHost(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (app.Environment.IsDevelopment())
        db.Database.EnsureCreated();
    else
        db.Database.Migrate();
}

app.UseWhen(
    context =>
    {
        var path = context.Request.Path;
        return !(path.StartsWithSegments("/v1/boot") || !path.StartsWithSegments("/provisioning/files/"));
    },
    appBuilder => { appBuilder.UseHttpsRedirection(); });

app.MapHealthChecks("/health");

app.UseAuthentication();
app.UseAuthorization();
app.MapCarter();

app.Run();