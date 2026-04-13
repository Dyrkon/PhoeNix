# PhoeNix Deployment Guide

This document covers how to deploy the PhoeNix orchestrator using NixOS. 

## 1. Using the NixOS Module
If you have an existing NixOS configuration, you can consume the PhoeNix module directly from the flake.

```nix
{ inputs, ... }: {
  imports = [ inputs.phoenix.nixosModules.default ];

  services.phoenix = {
    enable = true;
    monitoring = {
      enable = true;
      prometheusServer.enable = true;
      prometheusServer.ui.nginxProxy = true; # Access UI at /prometheus
    };
    nginx.enable = true;
    nginx.hostName = "orchestrator.internal";
  };
}
```

## 2. Pre-built Reference Images
The flake provides "Gold Images" for common infrastructure.

### Virtual Machine (QCOW2)
Optimized for **Proxmox (UEFI)** and KVM.
* **Build:** `nix build github:Dyrkon/PhoeNix#packages.x86_64-linux.vm`
* **Features:** Includes VirtIO drivers, QEMU Guest Agent, and `systemd-boot`.

### LXC Container
Standard system container for Proxmox or Incus.
* **Build:** `nix build github:Dyrkon/PhoeNix#packages.x86_64-linux.lxc`
* **Note:** Automatically configures `boot.isContainer = true`.

## 3. Zero-Touch Remote Deployment
You can deploy PhoeNix to a bare-metal server or fresh VM without cloning the repo using `nixos-anywhere`.

```bash
nix run github:nix-community/nixos-anywhere -- \
  --flake github:Dyrkon/PhoeNix#phoenix-x86 \
  root@<TARGET_IP>
```
*This command partitions the disk via **Disko**, installs NixOS, and sets up the Phoenix stack in one step.*

## 4. Post-Install Access
* **Default User:** `phoenix-admin`
* **Initial Password:** `phoenix-default-pass` (Change this immediately!)
* **SSH:** `ssh phoenix-admin@<IP>`
* **Web UI:** `http://<IP>/` (App) and `http://<IP>/prometheus/` (Metrics)