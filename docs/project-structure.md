# Project Structure

This document describes the layout of the PhoeNix repository. Use it as a map when navigating the codebase or deciding where new code should live.

## Repository Root

```
PhoeNix/
├── sources/          # .NET solution (all C# code)
├── nix/              # Nix packaging, modules, dev environment
├── docs/             # Documentation
├── README.md         # Project introduction
└── flake.nix         # Nix flake entry point
```

## .NET Solution (`sources/`)

```
sources/
├── src/              # Production projects
├── tests/            # Test projects
└── PhoeNix.sln       # Solution file
```

### Source Projects (`sources/src/`)

| Project | Path | Role |
|---------|------|------|
| `PhoeNix.Domain` | `src/PhoeNix.Domain/` | Aggregate roots, entities, value objects, strongly-typed IDs, domain events. No external dependencies. |
| `PhoeNix.Application` | `src/PhoeNix.Application/` | CQRS commands and queries (MediatR), service interfaces, FluentValidation validators, pipeline behaviors. |
| `PhoeNix.Persistence` | `src/PhoeNix.Persistence/` | EF Core DbContext, repository implementations, migrations, outbox interceptor. PostgreSQL in production. |
| `PhoeNix.Infrastructure` | `src/PhoeNix.Infrastructure/` | SSH, Nix operations, hardware probing, bootstrap image building, background services for provisioning. |
| `PhoeNix.WebAPI` | `src/PhoeNix.WebAPI/` | REST API using Carter (minimal APIs). Swagger, auth middleware, CORS. Port 5001 / 7031. |
| `PhoeNix.McpServer` | `src/PhoeNix.McpServer/` | MCP server for AI agent access (HTTP streamable transport). Port 5003. |
| `PhoeNix.WebAPP` | `src/PhoeNix.WebAPP/` | Blazor WebAssembly frontend with MudBlazor components. |
| `PhoeNix.WebAPP.ApiClient` | `src/PhoeNix.WebAPP.ApiClient/` | Typed HTTP clients for WebAPP → WebAPI communication. |
| `Phoenix.Presentation` | `src/Phoenix.Presentation/` | Shared DTOs and request/response contracts used by both WebAPI and WebAPP. |
| `PhoeNix.Common` | `src/PhoeNix.Common/` | Shared utilities, enums, and extension methods used across projects. |
| `PhoeNix.Contracts` | `src/PhoeNix.Contracts/` | Public API contracts (e.g., for machine callback endpoints). |

### Dependency Direction

```
WebAPI / WebAPP / McpServer   (Presentation)
         ↓
Infrastructure / Persistence  (Infrastructure)
         ↓
Application                   (Use Cases)
         ↓
Domain                        (Core — no dependencies)
         ↑
Common / Contracts             (Shared utilities — depended on by all)
```

### Test Projects (`sources/tests/`)

| Project | What it tests |
|---------|--------------|
| `PhoeNix.Domain.UnitTests` | Domain aggregate logic, value objects, domain events |
| `PhoeNix.Application.UnitTests` | Command/query handlers, validators, pipeline behaviors |
| `PhoeNix.Common.Tests` | Shared utilities |
| `PhoeNix.Persistence.Tests` | Repository implementations against an in-memory database |
| `PhoeNix.Infrastructure.Tests` | Infrastructure services (SSH, Nix generation, etc.) |
| `PhoeNix.WebAPI.Tests` | API integration tests |
| `PhoeNix.WebAPP.Tests` | Blazor UI tests (Playwright) |

Run all tests:
```bash
dotnet test sources/PhoeNix.sln
```

## Nix Packaging (`nix/`)

```
nix/
├── apps/               # Runnable apps (nix run .#<name>)
│   ├── updateDeps.nix      # Regenerates nix/deps.json and nix/webapp-deps.json
│   ├── createPxeImage.nix  # Builds a PXE bootstrap image
│   ├── createIngest.nix    # QMD knowledge index ingestion
│   └── ...
├── packages/           # Built artifacts (nix build .#<name>)
│   ├── solution/           # Full .NET solution build
│   ├── webapi/             # WebAPI package
│   ├── webapp/             # Blazor WebAssembly frontend
│   ├── mcpserver/          # MCP server package
│   ├── qmd/                # QMD knowledge index tool
│   ├── imageBuilder/       # Bootstrap image builder
│   └── tests/              # Test packages
├── modules/            # NixOS service modules (used in nixosConfigurations)
│   ├── phoenix/            # Top-level PhoeNix NixOS module
│   ├── api.nix             # WebAPI service
│   ├── mcpserver.nix       # MCP server service
│   ├── database.nix        # PostgreSQL setup
│   ├── nginx.nix           # Nginx reverse proxy
│   ├── monitoring.nix      # Prometheus integration
│   └── options.nix         # Module option declarations
├── configurations/     # Concrete NixOS system configurations
│   └── phoenix-server/     # x86 and ARM server configs, disko disk layout
├── shells/
│   └── default/            # Development shell (dotnet, bun, postgres, etc.)
├── process-compose/        # Local dev orchestration (nix run .#up)
├── lib/                    # Shared Nix library functions
└── deps.json           # NuGet dependency lockfile (backend)
    webapp-deps.json    # NuGet dependency lockfile (Blazor WASM)
```

> **Important:** `nix/deps.json` and `nix/webapp-deps.json` are lockfiles generated from the NuGet package graph. They must be kept in sync with `.csproj` files. See [development.md](./development.md#6-dependency-management) for instructions.

## Documentation (`docs/`)

| File | Contents |
|------|---------|
| `development.md` | Local dev setup, process-compose, Rider, database, dependency management |
| `deployment.md` | Production deployment: NixOS module, VM, LXC, nixos-anywhere |
| `project-structure.md` | This file |
| `mcp-server.md` | MCP server setup and available tools |
| `miscs/CLAUDE.md` | AI agent instructions for working in this codebase |
