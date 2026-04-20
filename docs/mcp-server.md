# PhoeNix MCP Server

The MCP server exposes PhoeNix over the [Model Context Protocol](https://modelcontextprotocol.io/), allowing AI agents (Claude Code, Cursor, etc.) to manage NixOS configurations, provision machines, and monitor setup sessions.

- **Transport**: Streamable HTTP
- **Endpoint**: `http://localhost:5003/mcp` (dev) / `https://<host>/mcp/` (production via nginx)

## Running

**Standalone** (requires PostgreSQL running separately):
```bash
dotnet run --project sources/src/PhoeNix.McpServer/PhoeNix.McpServer.csproj
```

**Full stack** (MCP server starts automatically alongside the API, app, and database):
```bash
nix run .#up
```

## Connecting Claude Code

```bash
claude mcp add --transport http phoenix-mcp http://localhost:5003/mcp
```

Run `/mcp` in Claude Code to verify the server shows as connected.

## Available Tools

### Configurations
| Tool | Description |
|------|-------------|
| `ListConfigurations` | List all NixOS configurations (paginated) |
| `GetConfiguration` | Get a configuration with all its modules and entries |
| `CreateConfiguration` | Create a new configuration |
| `UpdateConfiguration` | Rename or update a configuration |
| `DeleteConfiguration` | Delete a configuration |
| `AddConfigurationModule` | Add a module template instance to a configuration |
| `UpdateConfigurationModule` | Update module entry values |
| `AddConfigurationSystem` | Add a system template to a configuration |
| `PreviewConfigurationNix` | Render the full Nix flake file tree for a configuration |

### Module Templates
| Tool | Description |
|------|-------------|
| `ListModuleTemplates` | List module templates (paginated, searchable) |
| `GetModuleTemplate` | Get a template with its input schema |
| `GetModuleScaffolding` | Show the Nix prefix/suffix wrapping an existing template |
| `GetModuleScaffoldingPreview` | Preview Nix scaffolding for a given module type + inputs |
| `CreateModuleTemplate` | Create a new module template |
| `UpdateModuleTemplate` | Update an existing module template |

### Machines
| Tool | Description |
|------|-------------|
| `ListMachines` | List all machines with their status |
| `GetMachine` | Get a machine with hardware inventory |
| `CreateMachine` | Register a new machine |
| `GetMachineMetrics` | Get Prometheus metrics for a machine |

### Setup Sessions
| Tool | Description |
|------|-------------|
| `StartSetupSession` | Start a provisioning session for a machine |
| `ListSetupSessions` | List active and recent setup sessions |
| `GetSetupSession` | Get full session details |
| `GetSetupSessionStatus` | Get the current status/step of a session |
| `StartMachineProvisioning` | Trigger NixOS installation on the target machine |
| `GetMachineSetupStatus` | Poll installation progress |
| `CancelSetupSession` | Cancel an in-progress session |
