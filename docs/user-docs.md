# phoenix-UI user documentation

This is a practical, UI-first walkthrough for people who already know the basics of Nix/NixOS and want to manage a small homelab (or a few office machines) with PhoeNix.

If you do not know anything about Nix, use this change [IMPORTANT SETTINGS](./user-docs.md#settings-pxe-endpoint-and-machine-resolution) and add agent integration to help you using [MCP docs](./mcp-server.md#connecting-claude-code).


## Sign up and sign in

Open PhoeNix in your browser and sign in. If this is your first time, create a new account.

![Sign in page](assets/phoenix-UI1.png)

The "Create a new account" link takes you to the registration form.

![Sign in filled](assets/phoenix-UI2.png)

Registration is username + password (confirm password) and then you can sign in.

![Register page](assets/phoenix-UI3.png)

![Create account link](assets/phoenix-UI4.png)

## Navigation overview

The left sidebar is the main navigation:
- Machines: inventory of managed machines
- Setup sessions: provisioning/install workflows
- Nix configurations: configuration catalogue
- Templates: reusable module templates
- Settings: global orchestration settings

![Sidebar navigation](assets/phoenix-UI6.png)

## Machines: create and manage inventory

The Machines page is where you maintain an inventory of hosts you want to manage. Use the "+" button to register a machine.

![Machine inventory (add button)](assets/phoenix-UI5.png)

When creating a machine you typically provide:
- Title: how you recognize it later (e.g. "pve-one", "nas", "kiosk-01")
- MAC address: used for PXE workflows and identification
- Architecture: must match the system you will install (e.g. `x86_64-linux`)
- Install disk preference: which disk to pick when auto-partitioning (useful when multiple disks exist)

![Create machine dialog](assets/phoenix-UI7.png)

## Configurations: catalogue and detail page

Configurations are similar to Nix flakes, they store individual machine configurations which have modules (using NixOS module system) that you can add from the template library.

![Configuration detail](assets/phoenix-UI8.png)

On a configuration detail page you see the overall structure:
- Basic information (title, supported architectures)
- Inputs (flake inputs like `nixpkgs`)
- Shared modules (applied to all systems in the configuration)
- Systems (machine-targeted entry points; usually one per role/host type)

![Configurations list](assets/phoenix-UI9.png)

## Systems and modules: structure and validation

Inside a configuration, systems contain a set of modules. Each module exposes "entries" (editable values) you fill in.

In the system view you can:
- Add additional systems or modules
- Expand a module to see its entries and current values
- Run validation (button next to a system/module)

![System with modules](assets/phoenix-UI10.png)

Validation runs Nix evaluation/tests for the selected architecture and reports status next to the system or module.

Module validation uses tests (Nix checks) written by the template authors. System test uses *nixos-anywhere*s *--vm-test*, this can take long, but verifies that the configuration builds and boots. It will **not** test disko **partitioning**.

![Validation failed indicator](assets/phoenix-UI11.png)

When validation fails, click into details to see the Nix error output. The dialog includes the error output which you can copy and share.

![Validation failed dialog](assets/phoenix-UI12.png)

### Fixing a validation error

In this example the module has a placeholder value like `ssh-ed25519 YOUR KEY` and Nix fails because a value of one of the modules is a placeholder default value that needs to be replaced.

Fixes usually look like one of these:
- Provide a correct value (e.g. a real SSH public key)
- Adjust quoting/list syntax (depending on the module entry kind)
- Correct a missing required value

> [!IMPORTANT]
> Right now, there are three types where you can freely write values (Text, Single choice and List). These are for now raw values, meaning, if you put True in there, it will be a Nix bool. If you want to fill in a string, you need to use quotes: "string", like in our SSH key example.

![Module entries (before fix)](assets/phoenix-UI13.png)

Use the edit action on the module, replace the placeholder value with the real value, then validate again.

![Edit the problematic entry](assets/phoenix-UI14.png)

Once the values are correct, validation turns green and shows how long the check took.

![Validation succeeded](assets/phoenix-UI15.png)

## Module templates: reusable building blocks

Module templates define reusable modules (generic modules and system modules) that configurations reference.

The templates list shows:
- Name and type (Generic/System)
- Enabled state
- Supported architectures
- Number of entry definitions (how many editable values it exposes)

![Module templates list](assets/phoenix-UI16.png)

Open a template to see its details. The template detail page has two main views:
- Overview: metadata, entry definitions, and tests
- Code: the module code with highlighted placeholders and entry list
- Edit button that allows you to modify the template using an [editor](./user-docs.md#what-are-module-entries-and-placeholders)

![Template detail (overview)](assets/phoenix-UI17.png)

### What are module entries and placeholders?

Each "Entry Value Definition" has:
- Name: a human label
- Placeholder: an identifier that is replaced by the value
- Value kind: type of value (text, list, ranges, choices, ...)
- Binding kind: where it comes from (most are user-provided; some can be bound during setup, e.g. disk candidates)
- Default value

In the module code, placeholders appear as variables (e.g. `UserName`, `UserGroups`). When the template is used in a configuration, you fill in the entry values and PhoeNix materializes them into a `values.nix` file that the module imports. The PLACEHOLDER is substituted for args.PLACEHOLDER (args are imported values).

### Editing templates

Use "Edit template" to change:
- Module content
- Entry definitions (add/edit/remove)
- Required flake inputs (if the template expects extra inputs from the flake)
- Tests

![Module editor](assets/phoenix-UI19.png)

![Add entry definition dialog (example)](assets/phoenix-UI19.1.png)

The "Code" view helps you understand the template quickly: it highlights placeholders and shows a list of entry values on the side.

### Tests: catching regressions early

Templates can have tests. A test is a small Nix snippet that evaluates the module and asserts something about it.

When editing a test:
- "Test code" is the part you write.
- "Variables (from entries)" lets you select which placeholders should be exposed as variables for the test scaffolding.
- The prefix/suffix scaffolding wraps your test code so it can be evaluated consistently.

![Test editor](assets/phoenix-UI20.png)

## Setup sessions: provisioning and installing machines

Setup sessions guide you through provisioning/installing one or more machines. The Setup Sessions list shows:
- Start time
- How many machines were part of the session
- Progress
- Status (Completed / Completed with errors)

![Setup sessions](assets/phoenix-UI21.png)

Create a new setup session with the "+" button. The wizard typically goes through:
1. Select Machines
2. Select Configurations
3. Assign Systems
4. Review
5. Start

![Select machine for setup](assets/phoenix-UI22.png)


Pick which configurations are allowed in this setup session (only systems from these configurations will be available for assignment).

![Setup sessions configuration list](assets/phoenix-UI23.png)

Assign a concrete system (from the selected configurations) to each machine you want to provision.

![New setup session - select configurations](assets/phoenix-UI24.png)

Review confirms exactly what will be installed on each machine (machine + MAC + configuration + system).

![New setup session - assign systems](assets/phoenix-UI25.png)

Finally, start the session. At this point the session is ready and machines can PXE boot and begin provisioning.

![New setup session - review](assets/phoenix-UI26.png)

Watch the progress for the whole process and individual machines in their tabs

![New setup session - start](assets/phoenix-UI27.png)

### Setup session progress and stages

The session detail page auto-refreshes and shows per-machine stages.

You will see these stages (marking which stage the machine **completed**) and how long from the process will you see them in that state:
- Waiting for PXE - Machine hasn't booted into the mini NixOS image yet                  [5%]
- Artefacts assigned - The session has generated/assigned boot artefacts for the machine [15%]
- Bootstrapped - The mini NixOS image running in PXE is up and can reach PhoeNix         [5%]
- Probed - Hardware probed; for example disk candidates are discovered                   [65%]
- Orchestrated - nixos-anywhere deployed the configuration to the machine                [10%]
- Finished - The machine has your configuration installed and it called back

![Session detail - artefacts assigned](assets/phoenix-UI28.png)

If something goes wrong during install, the session shows a failure with logs (including an error code and command output).

> [!IMPORTANT]
> Right now, there is a bug in Disko 1.12.0 which give produces this error, they installation finishes anyway. This version is used, because the newer 1.13.0 breaks VM tests for nixos-anywhere.

![Session detail - waiting for PXE](assets/phoenix-UI30.png)

The machine spends the the most time with probed state, because after it has been probed, the installation takes place which takes up most of the time from the whole process.

![Session detail - probed (disk candidates)](assets/phoenix-UI29.png)

![Session detail - finished](assets/phoenix-UI31.png)

Right after the machine has been set up, metrics can be offline for a bit. It takes time for *Prometheus* to get the information about the new machines and to start logging them.

![Session detail - completed with errors](assets/phoenix-UI32.png)

## Machines after provisioning: metrics and updates

After a machine is provisioned, the machine detail page shows:
- Machine info (title, architecture, MAC, state)
- Live metrics (CPU/RAM/disk/network) when monitoring is configured and the machine is reachable

Metrics can take a bit to appear after provisioning (new Prometheus targets need time to be scraped; the UI may need enough samples for charts/queries).

![Machine metrics (online)](assets/phoenix-UI33.png)

### Updating a machine

Use "Update configuration" to apply a configuration/system to a machine that is already managed.

If the update succeeds you get a success dialog.
If it fails you can get an error dialog (similar to setup session failures) with details you can use to troubleshoot.

![Update result dialog](assets/phoenix-UI34.png)

## Settings: PXE endpoint and machine resolution

Settings as configurations, machines and templates are unique for every user for now. This means, each user can choose how they set up the application. Bellow are two important settings values for which will depend on your lab/organization setup.

### Netboot (PXE) public API base URL

Machines booting from the network must be able to reach the PhoeNix API early (before a full OS is installed and sometimes before DNS is available).

Use:
- A hostname if your LAN DNS is reliable during boot
- An IP address if you want the simplest/most reliable setup.

If the orchestrator address changes, update this setting, otherwise new PXE installs will fail.

### Machine resolution (DNS, mDNS, last known IP)

PhoeNix can resolve machine addresses for monitoring/target discovery using:
- DNS hostname: requires working DNS
- mDNS hostname (.local): requires Avahi/mDNS to work in your environment
    - Avahi is included in in the [VM/LXC image](../nix/configurations/phoenix-server/default.nix), not the module
- Last known IP: uses the IP recorded during provisioning; can go stale if DHCP changes (prefer static leases/reservations if you pick this)

![Settings - Netboot (PXE)](assets/phoenix-UI35.png)
