# Git Sync

PhoeNix can synchronize configurations and module templates with a git remote. Sync is **one-way** — you choose either Push-Only (DB is source of truth) or Pull-Only (git is source of truth). Only one mode is active at a time.

## Overview

| Mode | Direction | Source of truth | Use case |
|------|-----------|----------------|----------|
| Push-Only | DB → git | PhoeNix | Version history, CI validation, collaboration |
| Pull-Only | git → DB | Git repo | GitOps workflow, multi-instance sync |

## Repository Layout

PhoeNix creates (push) or reads (pull) this structure:

```
templates/
  timezone-sync.json
  nix-flake-settings.json
configurations/
  my-server.json
  my-desktop.json
flakes/                              # optional, push-only
  my-server/
    flake.nix
    Modules/
      timezone-sync/
        module.nix
        values.nix
    Systems/
      my-system/
        x86_64-linux.nix
        Modules/
          user-setup/
            module.nix
            values.nix
```

- `templates/*.json` — module templates (ID, name, content, tests, entry definitions, architectures)
- `configurations/*.json` — configurations (ID, title, modules, systems, inputs)
- `flakes/` — materialized Nix flakes with human-readable names (optional, push-only)

File names are slug versions of the template name or configuration title (lowercase, hyphens, no special characters).

## Configuration

All Git Sync settings live in **Settings → Git Sync** on the settings page.

### Sync Mode

- **None** (default): git sync is disabled
- **Push Only**: every change in PhoeNix is automatically pushed to git
- **Pull Only**: git is the source of truth; PhoeNix imports from git

### Common Settings (both modes)

| Setting | Description |
|---------|-------------|
| Remote URL | Git remote — SSH (`git@github.com:user/repo.git`) or HTTPS (`https://github.com/user/repo.git`) |
| Branch | Target branch (default: `main`) |
| Auth Method | None, SSH Key, or Token |
| Auth Secret | Path to SSH private key, or token value |

### Push-Only Settings

| Setting | Description |
|---------|-------------|
| Include Nix Files | Materialize `flakes/` directory with human-readable, runnable Nix flakes |
| Validation Tier | None, Syntax, Module, or Configuration — validation that must pass before push |

### Pull-Only Settings

| Setting | Description |
|---------|-------------|
| Polling Interval (minutes) | How often to check for remote changes. Empty or 0 = manual only |
| Delete Orphans | Remove DB entries that no longer exist in git (off by default) |

## Push-Only Mode

### How It Works

1. You create or edit configurations/templates in the PhoeNix UI
2. PhoeNix exports all data as JSON to the local git repo
3. If a validation tier is configured, validation runs before pushing
4. On success (or if validation is set to None), PhoeNix commits and pushes

### Automatic Push

Every configuration or template change triggers an automatic push in the background.

### Manual Push

Click **Sync Now** on the Settings page, or call the API:

```
POST /api/git-sync/push
```

### Validation Tiers

Set Validation Tier to gate pushes on passing checks:

| Tier | What it does |
|------|--------------|
| None | No local validation — push immediately (use when CI handles checks) |
| Syntax | Runs `nix flake check --no-build` |
| Module | Runs module-level Nix tests |
| Configuration | Full VM test via nixos-anywhere (slow but thorough) |

If validation fails, the push is blocked. Check PhoeNix logs for the Nix error output.

### CI Integration

When **Include Nix Files** is enabled, the `flakes/` directory contains self-contained, runnable Nix flakes. You can configure CI to validate them without needing a PhoeNix instance:

```bash
# Syntax check
nix flake check flakes/<config-name> --no-build

# Module tests
nix build flakes/<config-name>#checks.<arch>.<check-attr>

# Full VM test
nixos-anywhere -- --flake flakes/<config-name>#<system-name> --vm-test
```

For a CI-only workflow, set Validation Tier to **None** and let your CI pipeline run the checks against the pushed flakes.

## Pull-Only Mode

### How It Works

1. You maintain configurations and templates as JSON files in a git repo
2. PhoeNix periodically (or on manual trigger) pulls from the remote
3. All templates and configurations are upserted into the DB (matched by ID)
4. If **Delete Orphans** is on, DB entries not present in git are removed

### Read-Only UI

When Pull-Only is active, editing configurations and templates in the PhoeNix UI is disabled. All changes must go through git.

### Polling

- Set a polling interval > 0 for automatic periodic pulls
- PhoeNix checks for remote changes before pulling (no-op if nothing changed)
- Set to 0 or leave empty for manual-only sync

### Manual Pull

Click **Sync Now** on the Settings page, or call the API:

```
POST /api/git-sync/pull
```

### Creating JSON Files for Pull

The JSON format matches what Push-Only exports. The easiest way to learn the format:

1. Set up Push-Only mode temporarily
2. Create a configuration and template in PhoeNix
3. Inspect the exported JSON files in your git repo
4. Switch to Pull-Only mode

Each JSON file **must include the entity ID**. This is how PhoeNix matches existing DB records for upsert. If you create a new entity in git, generate a fresh GUID for its ID.

Templates are imported before configurations (since configurations reference templates by ID).

## Authentication

### SSH Key

1. Set **Auth Method** to "SSH Key"
2. Set **Auth Secret** to the path of the private key (e.g., `/var/lib/phoenix/keys/id_ed25519`)
3. Use SSH remote URL format: `git@github.com:user/repo.git`

Requirements:
- The key must be readable by the PhoeNix process
- The key must have no passphrase
- The remote host must be in `known_hosts`

### Token (HTTPS)

1. Set **Auth Method** to "Token"
2. Set **Auth Secret** to the token value (e.g., a GitHub Personal Access Token)
3. Use HTTPS remote URL format: `https://github.com/user/repo.git`

PhoeNix embeds the token in the remote URL for authentication.

## API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/git-sync/push` | Trigger manual push (push-only mode) |
| POST | `/api/git-sync/pull` | Trigger manual pull (pull-only mode) |

Both endpoints require authentication and return an error if the wrong mode is active.

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Push fails with auth error | Verify SSH key path/permissions or token validity |
| Push fails with "remote not reachable" | Ensure git remote is accessible from the PhoeNix server |
| Push blocked by validation | Check PhoeNix logs for the Nix error output; fix the configuration or lower the validation tier |
| Pull imports nothing | Verify JSON files are in `templates/` and `configurations/` directories with valid format |
| Pull doesn't detect changes | Polling only checks the configured branch; ensure you're pushing to the correct branch |
| SSH auth fails | Ensure the key has no passphrase and the remote host is in `known_hosts` for the PhoeNix process user |
