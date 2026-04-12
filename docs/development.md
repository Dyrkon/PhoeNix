# PhoeNix Development Guide

This guide covers setting up your local environment to work on the PhoeNix API and WebApp.

## 1. Prerequisites
You only need **Nix** with Flakes enabled. The environment provides the Dotnet 10 SDK, Bun, and PostgreSQL.

## 2. Running the Full Stack
The easiest way to run the entire environment (API + App + DB + Nginx) is via `process-compose`:

```bash
# Start everything
nix run .#up

# Stop everything
nix run .#down
```

### What happens under the hood?
1. **PostgreSQL:** A local instance starts on port `5432`. It initializes a database named `phoenix` with the `pgvector` extension.
2. **Persistence:** Data is stored in `./.dev-data/db`. You can wipe this folder to reset your database.
3. **Networking:** Nginx acts as a reverse proxy on `http://localhost:8888`.
    * `/` points to the WebApp.
    * `/api/` points to the WebAPI.

## 3. Working with JetBrains Rider
If you want to debug the WebAPI inside Rider while keeping the database and proxy running:

1. Run `nix run .#up`.
2. In the `process-compose` TUI, select `webapi` and press `k` to stop it (or just let Rider override the port).
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