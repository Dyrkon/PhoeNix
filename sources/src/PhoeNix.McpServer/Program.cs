using PhoeNix.Application;
using PhoeNix.Infrastructure;
using PhoeNix.McpServer;
using PhoeNix.McpServer.Auth;
using PhoeNix.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

builder.Services.AddMcpServerOptions(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure();
builder.Services.AddApplication();
builder.Services.AddMcpHost(builder.Configuration);

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

McpOAuthEndpoints.Map(app);

app.MapMcp("/mcp").RequireAuthorization();

app.Run("http://localhost:5003");
