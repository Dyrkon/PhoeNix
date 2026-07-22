# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Test Commands

The .NET solution lives under `sources/`. Most commands run from the repo root with a `sources/` prefix.

```bash
# Build
dotnet build sources/PhoeNix.sln

# Run all tests
dotnet test sources/PhoeNix.sln

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Run a single test project
dotnet test sources/tests/PhoeNix.Domain.UnitTests/PhoeNix.Domain.UnitTests.csproj

# Run a specific test
dotnet test --filter "FullyQualifiedName~TestMethodName"

# Run WebAPI (https://localhost:7031, Swagger at /swagger)
dotnet run --project sources/src/PhoeNix.WebAPI/PhoeNix.WebAPI.csproj --profile https

# Run Blazor WebAPP
dotnet run --project sources/src/PhoeNix.WebAPP/PhoeNix.WebAPP.csproj --profile https

# Run MCP Server (http://localhost:5003/mcp)
dotnet run --project sources/src/PhoeNix.McpServer/PhoeNix.McpServer.csproj

# EF Core migrations (PostgreSQL must be running — see below)
dotnet ef migrations add MigrationName --project sources/src/PhoeNix.Persistence --startup-project sources/src/PhoeNix.WebAPI
dotnet ef database update --project sources/src/PhoeNix.Persistence --startup-project sources/src/PhoeNix.WebAPI
```

### Local stack

Local development is orchestrated with [process-compose](https://github.com/F1bonacc1/process-compose) via Nix. `nix run .#up` starts the full stack (PostgreSQL, WebAPI, WebAPP, MCP server, Prometheus, and an nginx reverse proxy); `nix run .#down` stops it. Running EF Core or the API directly requires PostgreSQL to be up. See `docs/development.md` for the complete guide.

## Architecture Overview

PhoeNix is a NixOS machine provisioning and configuration management system built with Clean Architecture and DDD principles.

### Project Structure

| Project | Purpose |
|---------|---------|
| PhoeNix.Domain | Aggregate roots, entities, strongly-typed IDs, domain events |
| PhoeNix.Application | CQRS commands/queries via MediatR, service abstractions, validators |
| PhoeNix.Persistence | EF Core DbContext, repositories, migrations, outbox pattern (PostgreSQL via Npgsql) |
| PhoeNix.Infrastructure | External services: SSH, Nix operations, file system, hardware probing |
| PhoeNix.WebAPI | REST API using Carter (minimal API modules) |
| PhoeNix.McpServer | MCP server for AI agent access (HTTP transport, port 5003) |
| PhoeNix.WebAPP | Blazor WebAssembly frontend with MudBlazor components |
| PhoeNix.WebAPP.ApiClient | HTTP client library for WebAPP→WebAPI communication |
| PhoeNix.Contracts | Request/response contract records shared by API and clients |
| Phoenix.Presentation | Shared DTOs and contracts between API and frontend |
| PhoeNix.Common | Shared utilities and enums |

### Key Patterns

**Strongly-Typed IDs**: All entities use `StronglyTypedId<T>` base class (e.g., `ConfigurationId`, `MachineId`) to prevent ID mix-ups at compile time.

**CQRS with MediatR**: Commands and queries are organized by aggregate under `Application/{AggregateName}/`. Handlers follow the pattern `{Command/Query}Handler.cs`.

**Repository Pattern**: Read/write separation - `IConfigurationRepository` for writes, `IConfigurationReadRepository` for queries.

**Result Pattern**: Operations return `Result<T>` or `Result` with `.Tap()`, `.Map()` for functional composition. Errors have Code and Message.

**Outbox Pattern**: Domain events are captured via `InsertOutboxMessagesInterceptor` and processed by `OutboxProcessorBackgroundService`.

**MediatR Behaviors**: `UnitOfWorkBehavior` wraps handlers in transactions; `RequestLoggingBehavior` logs execution.

### Domain Aggregates

- **Configuration** - NixOS configuration composed of module values and entry values
- **Machine** - Physical/virtual machine with hardware inventory and deployment status
- **SetupSession** - Provisioning workflow state machine
- **ModuleTemplate/SystemTemplate** - Reusable configuration templates with inputs

### Authentication

Dual authentication: Cookie-based for web sessions, JWT for provisioning callbacks. User sessions managed via `IUserSessionService`.

### Database

PostgreSQL 18 with the `pgvector` extension, accessed through the Npgsql EF Core provider (`options.UseNpgsql(...)` in `PhoeNix.Persistence/DependencyInjection.cs`). The connection string is named `PhoeNix` and is supplied via the `ConnectionStrings__DefaultConnection` environment variable, e.g. `Host=localhost;Port=5432;Username=phoenix;Database=phoenix;`.

For local development the database runs via process-compose (`nix run .#up`) at `localhost:5432`, with data stored in `.dev-data/db` (wipe this folder to reset). Tests use the EF Core in-memory provider (`AddInMemoryPersistence`), so they need no running database.

## Tech Stack

- .NET 10.0, Blazor WebAssembly, ASP.NET Core
- Carter (minimal API), MudBlazor (UI), MediatR (CQRS)
- Entity Framework Core (PostgreSQL / Npgsql, pgvector), FluentValidation
- XUnit, FluentAssertions, Coverlet (testing)
- Nix + process-compose for local orchestration (PostgreSQL, nginx reverse proxy, Prometheus)

## MCP Server

The MCP server exposes PhoeNix functionality to AI agents via the [Model Context Protocol](https://modelcontextprotocol.io/). It runs on `http://localhost:5003/mcp`.

To register it with Claude Code:
```bash
claude mcp add --transport http phoenix-mcp http://localhost:5003/mcp
```

The MCP server must be running first — either via `dotnet run` (standalone) or `nix run .#up` (full stack). See `docs/mcp-server.md` for available tools.

## Rule: use MCP tools to interact with PhoeNix data

When working with live PhoeNix data (configurations, machines, module templates, setup sessions), always use the `phoenix-mcp` MCP tools instead of querying the database directly or writing ad-hoc scripts.

Available tool groups:

- **Configurations** — `ListConfigurations`, `GetConfiguration`, `CreateConfiguration`, `UpdateConfiguration`, `DeleteConfiguration`, `AddConfigurationModule`, `UpdateConfigurationModule`, `AddConfigurationSystem`, `PreviewConfigurationNix`

- **Module Templates** — `ListModuleTemplates`, `GetModuleTemplate`, `GetModuleScaffolding`, `GetModuleScaffoldingPreview`, `CreateModuleTemplate`, `UpdateModuleTemplate`

- **Machines** — `ListMachines`, `GetMachine`, `CreateMachine`, `GetMachineMetrics`

- **Setup Sessions** — `StartSetupSession`, `ListSetupSessions`, `GetSetupSession`, `GetSetupSessionStatus`, `StartMachineProvisioning`, `GetMachineSetupStatus`, `CancelSetupSession`

The MCP server must be running (`dotnet run --project src/PhoeNix.McpServer/PhoeNix.McpServer.csproj` or `nix run .#up`) and registered (`claude mcp add --transport http phoenix-mcp http://localhost:5003/mcp`).

Use MCP tools for reads and writes. Fall back to direct DB access only when the required operation has no corresponding tool.

## Rule: always use qmd before reading files

Before reading files or exploring directories, always use qmd to search for information in local projects.

Available tools:

- `qmd search “query”` — fast keyword search (BM25)

- `qmd query “query”` — hybrid search with reranking (best quality)

- `qmd vsearch “query”` — semantic vector search

- `qmd get <file>` — retrieve a specific document

Use qmd search for quick lookups and qmd query for complex questions.

Use Read/Glob only if qmd doesn’t return enough results.

Once this is in place, Claude will always search the index first. It will only fall back to reading full files when it genuinely can’t find what it needs through the

index.