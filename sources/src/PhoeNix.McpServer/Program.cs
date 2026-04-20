using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;
using PhoeNix.Application;
using PhoeNix.Infrastructure;
using PhoeNix.McpServer;
using PhoeNix.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

builder.Services.AddMcpServerOptions(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure();
builder.Services.AddApplication();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.MapMcp("/mcp");

app.Run("http://localhost:5003");
