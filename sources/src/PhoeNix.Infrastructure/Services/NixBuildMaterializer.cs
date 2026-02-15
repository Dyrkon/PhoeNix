using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services;

public class NixBuildMaterializer : INixBuildMaterializer
{
    private string BuildFollow(FollowInput followInput)
    {
        return $"{followInput.FollowName}.follows = \"{followInput.FollowValue}\";";
    }

    private Result<InputBuildResult> BuildInput(Input input)
    {
        var follows = input.Followers.Aggregate("", (current, result) => current + $"{BuildFollow(result)}\n");
        return new InputBuildResult(
            $"{input.Name} = {{ url = \"{input.Source}\";\n " +
            $"inputs = {{ {follows} }};" +
            $"}};");
    }

    private Result<ModuleBuildResult> BuildModule(ModuleTemplate moduleTemplate, ModuleValue moduleValue,
        string moduleValuesName = "values")
    {
        var inputs = "{ ";
        var outputContent = moduleTemplate.Content;
        foreach (var value in moduleValue.EditableValues)
        {
            inputs += $"{value.Name} = {value.Value};";
            outputContent = outputContent.Replace(value.Name, $"args.{value.Name}");
        }

        inputs += " }";
        var config = moduleTemplate.Type == ModuleType.System ? "config, " : "";
        var inputsLocationPlaceholder = Guid.NewGuid().ToString();
        var moduleContent =
            $"{{ inputs, pkgs, lib, system, {config}... }}: let\n args = import {inputsLocationPlaceholder}/{moduleValuesName}.nix; \nin {{ {outputContent} }}";

        var moduleTests = moduleTemplate.Tests.Select(t => t.Build()).ToList();
        if (moduleTests.Any(i => i.IsFailure))
            return Result.Failure<ModuleBuildResult>(
                new Error("", $"Failed to build tests for module {moduleTemplate.Name}."));

        return new ModuleBuildResult(moduleTemplate.Id, moduleTemplate.Name, moduleContent, inputs, moduleValuesName,
            inputsLocationPlaceholder,
            moduleTests.Select(t => t.Value).ToList());
    }

    private Result<SystemBuildResult> BuildSystem(Domain.Entities.Systems.System system,
        List<ModuleTemplate> moduleTemplates)
    {
        var modules =
            system.Modules.Select(m => BuildModule(moduleTemplates.First(i => i.Id == m.ModuleTemplateId), m));
        if (modules.Any(m => m.IsFailure))
            return Result.Failure<SystemBuildResult>(
                new Error("", $"Failed to build module/s for system {system.Name}"));

        var moduleResults = modules.Select(m => m.Value);
        var modulesListPlaceholder = Guid.NewGuid().ToString();
        // TODO Can't use lib.nixosSystem for darwin
        var systemContent =
            $"{{ inputs, lib, sharedModules }}:\ninputs.nixpkgs.lib.nixosSystem {{ specialArgs = {{ inherit inputs; }}; system = \"{system.Architecture.ToArchitectureString()}\"; modules = sharedModules ++ [ {modulesListPlaceholder} ]; }}";

        return new SystemBuildResult(system.Id, system.Name, system.Architecture, systemContent, moduleResults,
            modulesListPlaceholder);
    }

    public Result<ConfigurationBuildResult> MaterializeConfiguration(Configuration configuration,
        IReadOnlyCollection<ModuleTemplate> templates)
    {
        var inputs = configuration.Inputs.Select(BuildInput);
        if (inputs.Any(i => i.IsFailure))
            return Result.Failure<ConfigurationBuildResult>(new Error("",
                $"Failed to build input in configuration {configuration.Title}"));
        var modules =
            configuration.Modules.Select(m => BuildModule(templates.First(i => i.Id == m.ModuleTemplateId), m));
        if (modules.Any(i => i.IsFailure))
            return Result.Failure<ConfigurationBuildResult>(new Error("",
                $"Failed to build module in configuration {configuration.Title}"));
        var systems = configuration.SystemSpecifications.Select(s => BuildSystem(s, templates.ToList()));
        if (systems.Any(i => i.IsFailure))
            return Result.Failure<ConfigurationBuildResult>(new Error("",
                $"Failed to build system in configuration {configuration.Title}"));
        var supportedArchitectures = configuration.SupportedSystemArchitectures();
        if (supportedArchitectures.IsFailure || supportedArchitectures.Value.Count == 0)
            return Result.Failure<ConfigurationBuildResult>(new Error("",
                $"Failed to get supported architectures for configuration {configuration.Title}"));

        var systemsPlaceholder = Guid.NewGuid().ToString();
        var sharedModulesPlaceholder = Guid.NewGuid().ToString();
        var checksPlaceholder = Guid.NewGuid().ToString();
        var inputsValues = inputs.Aggregate("", (current, result) => current + $"{result.Value.Input}\n");

        var content =
            $"{{ description = \"{configuration.Description}\"; " +
            $"inputs = {{ flake-utils.url = \"github:numtide/flake-utils\"; disko.url = \"github:nix-community/disko/latest\"; disko.inputs.nixpkgs.follows = \"nixpkgs\"; {inputsValues} }};\n " +
            $"outputs = {{self, nixpkgs, flake-utils, ...}} @ inputs: " +
            $"let\n systems = [{supportedArchitectures.Value.Aggregate("", (s, architecture) => $"\"{s + architecture.ToArchitectureString()}\" ")}];" +
            $"sharedModules = [ {sharedModulesPlaceholder} ];\n" +
            $"lib = nixpkgs.lib;" +
            $"in\n" +
            $"flake-utils.lib.eachSystem systems (system: let " +
            $"pkgs = nixpkgs.legacyPackages.${{system}}; \nin {{ \n" +
            $"formatter = pkgs.nixfmt;\n " +
            $"checks = {{ {checksPlaceholder} }};" +
            $"}}) // {{ nixosConfigurations = {{ {systemsPlaceholder} }}; }}; }}";

        return new ConfigurationBuildResult(configuration.Id, configuration.Title, content, sharedModulesPlaceholder,
            systemsPlaceholder, checksPlaceholder, supportedArchitectures.Value,
            modules.Select(m => m.Value),
            systems.Select(s => s.Value));
    }
}