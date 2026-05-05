# PhoeNix Development Guide

This guide covers setting up your local environment to work on the PhoeNix API and WebApp.

## 1. Prerequisites
You only need **Nix** with Flakes enabled. The environment provides the .NET 10 SDK, Bun, and PostgreSQL.

## 2. Running the Full Stack

You can enter development environment with all dependencies prepared by running:

```bash
nix develop
```

PhoeNix uses [process-compose](https://github.com/F1bonacc1/process-compose) to orchestrate all local services.

```bash
# Start everything
nix run .#up

# Stop everything
nix run .#down
```
> [!IMPORTANT]
> You have to change Netboot (PXE) public API Base URL in settings before you start using the app to http://{your hostname or IP of the orchestrator machine}:8888/api

### Services started by process-compose

| Process | Address | Description |
|---------|---------|-------------|
| `postgres` | `localhost:5432` | PostgreSQL 18 with pgvector. Data stored in `.dev-data/db`. |
| `webapi` | `http://localhost:5001` | REST API (also accessible via nginx at `https://localhost:8443/api/`) |
| `webapp` | `https://localhost:7052` | Blazor WebAssembly frontend (also via nginx at `https://localhost:8443/`) |
| `mcpserver` | `http://localhost:5003` | MCP server for AI agent access |
| `node-exporter` | `localhost:9100` | Prometheus node exporter |
| `prometheus` | `localhost:9090` | Prometheus (also via nginx at `https://localhost:8443/prometheus/`) |
| `nginx` | `http://localhost:8888` → `https://localhost:8443` | Reverse proxy (HTTP redirects to HTTPS except provisioning callbacks) |

### What happens under the hood?
1. **Build:** All projects are compiled in Release mode before any service starts (`build-all` process)
2. **PostgreSQL:** Initialized in `.dev-data/db` on first run. Wipe this folder to reset your database.
3. **Networking:** Nginx acts as a reverse proxy. HTTP on port 8888 redirects to HTTPS on 8443 (except provisioning callback endpoints which machines call over plain HTTP).
4. **TLS:** A self-signed certificate is generated at build time for `localhost`. You may need to accept it in your browser.

## 3. Working with JetBrains Rider
If you want to debug the WebAPI inside Rider while keeping the database and proxy running:

1. Run `nix run .#up`.
2. In the process-compose TUI, select `webapi` and press `k` to stop it.
3. In Rider, set the following **Environment Variables** in your Run Configuration:
   ```text
   ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Username=phoenix;Database=phoenix;
   PHOENIX_STATE_DIR=./.dev-data/phoenix
   ```
4. Click **Debug**.

## 4. Manual Database Access
To connect to the development database manually (e.g., for running migrations or checking tables):

```bash
# Using psql (specified database and user are required)
psql -h localhost -U phoenix -d phoenix
```

## 5. Monitoring & Tokens
The API automatically generates a Prometheus discovery token in `./.dev-data/phoenix/prometheus-token`.
* The **Local Bypass** is active for `127.0.0.1` requests.
* If testing the full HTTP Service Discovery flow, ensure the API is returning the target JSON at `/api/monitoring/targets`.

## 6. Running Tests

Most test projects run without any services:

```bash
dotnet test sources/PhoeNix.sln
```

**Exception:** `PhoeNix.WebAPP.Tests` (Playwright UI tests) require the full stack to be running. Start it first, then run the tests:

```bash
nix run .#up   # keep this running in another terminal
dotnet test sources/tests/PhoeNix.WebAPP.Tests/PhoeNix.WebAPP.Tests.csproj
```

## 7. Dependency Management

PhoeNix uses Nix to build the .NET solution reproducibly. This requires two lockfiles that pin the exact NuGet packages fetched during the build:

- `nix/deps.json` — backend (WebAPI, Application, Domain, etc.)
- `nix/webapp-deps.json` — Blazor WebAssembly frontend

**Before contributing code that adds or updates a NuGet package**, regenerate these lockfiles:

```bash
nix run .#updateDeps
```

This command fetches all NuGet dependencies and writes the updated lockfiles. Commit `nix/deps.json` and `nix/webapp-deps.json` alongside your `.csproj` changes — the Nix build will fail without them.
