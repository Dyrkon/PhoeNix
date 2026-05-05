# PhoeNix

PhoeNix is a NixOS machine provisioning and configuration management platform. It gives infrastructure administrators a web-based interface to build reproducible NixOS configurations from reusable module templates and provision bare-metal or virtual machines — without writing Nix code by hand.

## What PhoeNix can do

- **Compose configurations visually** — browse a library of parameterized module templates and assemble NixOS configurations through forms instead of editing `.nix` files
- **Provision machines over the network** — boot target machines via PXE, probe their hardware automatically, then install a fully configured NixOS system in one workflow
- **Manage your fleet** — track machine status, hardware inventory, deployment snapshots, and configuration history from a single dashboard
- **Update machines remotely** — push NixOS configuration changes to provisioned machines without manual SSH work
- **Monitor infrastructure** — built-in Prometheus integration with per-machine metrics and a scrape-target discovery endpoint
- **Control everything via AI agents** — a built-in MCP server exposes all functionality to Claude Code and other MCP-compatible agents
- **Share and reuse** — export and import configurations and module templates across instances

## How it works

1. A knowledgeable user writes a **module template** — a parameterized NixOS module with typed entry fields (text, integer, choice, list)
2. Users compose a **configuration** by picking templates from the library and filling in their values
3. A **setup session** is created for target machines: PhoeNix serves a bootstrap image, the machine PXE-boots, hardware is probed automatically
4. PhoeNix generates the full NixOS flake, builds the system closure, and installs it via `nixos-anywhere` + `disko`
5. The machine calls back on first boot and transitions to `Provisioned` state

## Installation

You will need [nix](https://nixos.org/download/) either as a package manager or an OS in form of NixOS.

You can either:
- Install the [PhoeNix package](TODO)
- Clone this repo and use `nix run` if you want to modify the project later

## Documentation

- [Project structure](./docs/project-structure.md)
- [Development guide](./docs/development.md)
- [Deployment guide](./docs/deployment.md)
- [MCP server](./docs/mcp-server.md)
- [Project technical overview](./docs/PROJECT.md) — architecture, domain model, tech stack (useful for AI agents)

## How to contribute?

Nix makes it simple:

1. Clone the repo: `git clone https://github.com/Dyrkon/PhoeNix.git`
2. Start development environment: `nix develop`
3. Add new awesome feature or fix a bug
4. Test the change
5. Create a pull request ([how to](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/creating-a-pull-request))

See [project-structure.md](./docs/project-structure.md) for a map of the codebase and [development.md](./docs/development.md) for the full local setup guide.

## Special thanks
