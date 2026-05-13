# PhoeNix Deployment Guide

This document covers how to deploy the PhoeNix orchestrator using NixOS. PhoeNix can be deployed as a NixOS module into an existing configuration, as a pre-built VM image, as an LXC container, or directly onto bare metal using `nixos-anywhere`.

## 1. Using the NixOS Module
If you have an existing NixOS configuration, you can consume the PhoeNix module directly from the flake.

```nix
{ inputs, ... }: {
  imports = [ inputs.phoenix.nixosModules.default ];

  services.phoenix = {
    enable = true;
    mcpServer = {
      enable = true;
    };
    monitoring = {
      enable = true;
      prometheusServer.enable = true;
      prometheusServer.ui.nginxProxy = true; # Access UI at /prometheus
    };
    nginx.enable = true;
  };
}
```

## 2. Pre-built Reference Images

Images are built using [nixos-generators](https://github.com/nix-community/nixos-generators), which is now part of nixpkgs. The `--image-variant` and `--format` flags correspond to nixos-generators format names. All formats listed in the [nixos-generators format reference](https://github.com/nix-community/nixos-generators?tab=readme-ov-file#supported-formats) are available.

General command pattern:
```bash
nixos-rebuild build-image \
  --flake github:Dyrkon/PhoeNix#phoenix-x86 \
  --image-variant <format> \
  --format <format>
```

### Virtual Machine (QCOW2)
Optimized for **Proxmox (UEFI)** and KVM. Includes VirtIO drivers, QEMU Guest Agent, and `systemd-boot`.

```bash
nixos-rebuild build-image \
  --flake github:Dyrkon/PhoeNix#phoenix-x86 \
  --image-variant qemu
```

### LXC Container (Proxmox)

```bash
nixos-rebuild build-image \
  --flake github:Dyrkon/PhoeNix#phoenix-x86 \
  --image-variant proxmox-lxc
```

Proxmox has **disabled mDNS** out of the box, you need to **enable** it by running following commands if you want to use hostnames for machine updates and metrics.

```bash
cat /sys/class/net/vmbr0/bridge/multicast_snooping # If this returns 1, mDNS will not work
echo 0 > /sys/class/net/vmbr0/bridge/multicast_snooping # Enable multicast snooping
```

> [!WARNING]
> Current version of Disko (1.12.0) is used, because version (1.13.0) breaks `--vm-test` until this [issue](https://github.com/nix-community/disko/issues/1203) is fixed, VMs in proxmox should be created with these parameters:

- Machine: q35
- BIOS: OVMF (UEFI)
  - Disable prerolled keys
  - Add EFI disk
- Bus/Device: SCSI
  - Set the index starting with 0

## 3. Zero-Touch Remote Deployment
You can deploy PhoeNix to a bare-metal server or fresh VM without cloning the repo using `nixos-anywhere`.

```bash
nix run github:nix-community/nixos-anywhere -- \
  --flake github:Dyrkon/PhoeNix#phoenix-x86 \
  root@<TARGET_IP>
```
*This command partitions the disk via **Disko**, installs NixOS, and sets up the Phoenix stack in one step.*

## 4. Post-Install Access

> [!IMPORTANT]
> You have to change *Netboot (PXE)* public API Base URL in settings before you start using the app to http://{your hostname or IP of the orchestrator machine}/api

> [!IMPORTANT]
> If you want to resolve machines for updates and metrics via DNS, change your local domain in settings section *Machine Resolution*

* **Default User:** `phoenix-admin`
* **Initial Password:** `phoenix-default-pass` (Change this immediately!)
* **SSH:** `ssh phoenix-admin@<IP>`
* **Web UI:** `https://<hostname>/` (proxied by nginx)
* **Prometheus:** `https://<hostname>/prometheus/`
* **MCP server:** `https://<hostname>/mcp/` (requires JWT auth)

> [!TIP]
> Simplest way to add the MCP server: `claude mcp add --transport http phoenix-mcp-lxc http://<hostname>/mcp/`

## 5. Monitoring

Prometheus is included and enabled via the NixOS module's `monitoring` option. When enabled:

* Prometheus scrapes the PhoeNix API's HTTP service discovery endpoint (`/api/monitoring/targets`) to discover provisioned machines automatically
* The scrape endpoint is token-protected; the API generates and rotates the token
* Node Exporter runs on the orchestrator host and is scraped by default
* The Prometheus UI is accessible at `/prometheus/` via nginx (when `prometheusServer.ui.nginxProxy = true`)

Machine metrics collected by PhoeNix are displayed in the Machine detail page of the web UI and are queryable by AI agents via the `GetMachineMetrics` MCP tool.
