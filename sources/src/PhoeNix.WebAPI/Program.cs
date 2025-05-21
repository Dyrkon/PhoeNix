using Carter;
using Microsoft.AspNetCore.Http.HttpResults;
using PhoeNix.Application;
using PhoeNix.Domain.Service;
using PhoeNix.Infrastructure;
using PhoeNix.Persistence;
using PhoeNix.WebAPI.OptionSetsup;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddCarter();

builder.Services.ConfigureOptions<FileStorageOptionsSetup>();

builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure();
builder.Services.AddApplication();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapHealthChecks("/health");

app.MapCarter();

app.Run();