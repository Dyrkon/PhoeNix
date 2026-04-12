using System.Text.RegularExpressions;
using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Application.Models.Modules;
using PhoeNix.Application.Models.Setup;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services.ConfigurationManagement;

public class NixBuildMaterializer : INixBuildMaterializer
{
    public NixModuleScaffolding GetModuleScaffolding(ModuleType type, string? argsImportValue = "./values.nix")
    {
        var config = type == ModuleType.System ? "config, " : string.Empty;

        var prefix =
            $"{{ inputs, pkgs, lib, system, {config}... }}: let\n" +
            $"  args = import {argsImportValue};\n" +
            "in { ";

        var suffix = " }";

        return new NixModuleScaffolding(prefix, suffix);
    }

    public NixTestScaffolding GetTestScaffolding(string testName, string? testedModule = "./module.nix",
        string? argsInputLocation = "./values.nix")
    {
        var prefix =
            "{ inputs, pkgs, ... }: let\n" +
            "  inherit (pkgs) lib;\n" +
            "  inherit inputs;\n" +
            "  inherit (lib) runTests;\n" +
            $"  testedModule = import {testedModule} {{ inherit lib inputs pkgs; }};\n" +
            "  testResults = lib.runTests { ";

        var suffix =
            " };\n" +
            $"  args = import {argsInputLocation};\n" +
            "in\n" +
            $"pkgs.runCommand \"{testName}\" {{ failures = builtins.toJSON testResults; }} ''\n" +
            "if [ \"$failures\" = \"[]\" ]; then\n" +
            "  echo \"All tests passed!\";\n" +
            "  touch $out;\n" +
            "else\n" +
            "  printf '%s' \"$failures\";\n" +
            "  exit 1\n" +
            "fi''";

        return new NixTestScaffolding(prefix, suffix);
    }

    private string BuildFollow(FollowInput followInput)
    {
        return $"{followInput.FollowName}.follows = \"{followInput.FollowValue}\";";
    }

    private Result<InputBuildResult> BuildInput(Input input)
    {
        var follows =
            input.Followers.Aggregate(string.Empty, (current, result) => current + $"{BuildFollow(result)}\n");

        return new InputBuildResult(
            $"{input.Name} = {{ url = \"{input.Source}\";\n" +
            $"inputs = {{ {follows} }};" +
            "};");
    }

    private List<ModuleBuildResult> BuildBuiltInSystemModules(BuiltInModuleParameters? builtInModules)
    {
        var modules = new List<ModuleBuildResult>();

        if (builtInModules?.Callback is not null)
            modules.Add(BuildCallbackBuiltInModule(builtInModules.Callback));

        if (builtInModules?.DeployAccess is not null)
            modules.Add(BuildDeployAccessBuiltInModule(builtInModules.DeployAccess));

        return modules;
    }

    private ModuleBuildResult BuildDeployAccessBuiltInModule(DeployAccessModuleParameters parameters)
    {
        var deployUser = parameters.DeployUser.Trim();
        var deployCaPublicKey = parameters.DeployCaPublicKey.Trim();

        var content =
            "{ pkgs, ... }:\n" +
            "{\n" +
            $"  users.users.{deployUser} = {{\n" +
            "    isNormalUser = true;\n" +
            "    createHome = true;\n" +
            "    extraGroups = [ \"wheel\" ];\n" +
            "    hashedPassword = \"!\";\n" +
            "  };\n" +
            "\n" +
            "  services.openssh.enable = true;\n" +
            $"  environment.etc.\"ssh/phoenix_deploy_ca.pub\".text = {ToNixString(deployCaPublicKey)};\n" +
            "  services.openssh.settings = {\n" +
            "    PubkeyAuthentication = true;\n" +
            "    TrustedUserCAKeys = \"/etc/ssh/phoenix_deploy_ca.pub\";\n" +
            "  };\n" +
            "\n" +
            $"  nix.settings.trusted-users = [ \"root\" \"{deployUser}\" ];\n" +
            "\n" +
            "  security.sudo.extraRules = [\n" +
            "    {\n" +
            $"      users = [ \"{deployUser}\" ];\n" +
            "      commands = [\n" +
            "        {\n" +
            "          command = \"ALL\";\n" +
            "          options = [ \"NOPASSWD\" ];\n" +
            "        }\n" +
            "      ];\n" +
            "    }\n" +
            "  ];\n" +
            "}";

        return new ModuleBuildResult(
            new ModuleTemplateId(Guid.NewGuid()),
            "PhoenixDeployAccess",
            content,
            "{ }",
            "values",
            Guid.NewGuid().ToString(),
            []);
    }

    private ModuleBuildResult BuildCallbackBuiltInModule(CallbackModuleParameters parameters)
    {
        var content =
            "{ pkgs, ... }:\n" +
            "let\n" +
            $"  finalizeUrl = {ToNixString(parameters.FinalizeUrl)};\n" +
            $"  bearerToken = {ToNixString(parameters.BearerToken)};\n" +
            "  callbackScript = pkgs.writeShellScript \"phoenix-finalize-setup\" ''\n" +
            "    set -euo pipefail\n" +
            "    mkdir -p /var/lib/phoenix/setup\n" +
            "    if [ -f /var/lib/phoenix/setup/finalized ]; then\n" +
            "      exit 0\n" +
            "    fi\n" +
            "    ${pkgs.curl}/bin/curl \\\n" +
            "      --fail \\\n" +
            "      --silent \\\n" +
            "      --show-error \\\n" +
            "      -X POST \\\n" +
            "      -d \"\" \\\n" +
            "      -H \"Authorization: Bearer ${bearerToken}\" \\\n" +
            "      \"${finalizeUrl}\"\n" +
            "    touch /var/lib/phoenix/setup/finalized\n" +
            "  '';\n" +
            "in {\n" +
            "  systemd.tmpfiles.rules = [\n" +
            "    \"d /var/lib/phoenix 0755 root root -\"\n" +
            "    \"d /var/lib/phoenix/setup 0755 root root -\"\n" +
            "  ];\n" +
            "  systemd.services.phoenix-finalize-setup = {\n" +
            "    description = \"Finalize PhoeNix machine setup\";\n" +
            "    wantedBy = [ \"multi-user.target\" ];\n" +
            "    after = [ \"network-online.target\" ];\n" +
            "    wants = [ \"network-online.target\" ];\n" +
            "    unitConfig.ConditionPathExists = \"!/var/lib/phoenix/setup/finalized\";\n" +
            "    serviceConfig = {\n" +
            "      Type = \"oneshot\";\n" +
            "      ExecStart = callbackScript;\n" +
            "      Restart = \"on-failure\";\n" +
            "      RestartSec = \"10s\";\n" +
            "    };\n" +
            "  };\n" +
            "}";

        return new ModuleBuildResult(
            new ModuleTemplateId(Guid.NewGuid()),
            "PhoenixFinalizeSetup",
            content,
            "{ }",
            "values",
            Guid.NewGuid().ToString(),
            []);
    }

    private static string ToNixString(string value)
    {
        var escaped = value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        return $"\"{escaped}\"";
    }

    private Result<ModuleTestBuildResult> BuildModuleTest(Test test, string moduleValuesName = "values")
    {
        var inputsLocationPlaceholder = Guid.NewGuid().ToString();
        var testedModulePathPlaceholder = Guid.NewGuid().ToString();

        var variableNames = test.VariableNames
            .Distinct()
            .OrderByDescending(v => v.Length)
            .ToList();

        var outputContent = test.Content;
        var placeholders = new Dictionary<string, string>();

        foreach (var variableName in variableNames)
        {
            var placeholder = $"__phoenix_{Guid.NewGuid():N}_{variableName}__";
            placeholders[variableName] = placeholder;

            outputContent = Regex.Replace(
                outputContent,
                $@"\b{Regex.Escape(variableName)}\b",
                placeholder);
        }

        var unresolvedVariables = variableNames
            .Where(variableName =>
                Regex.IsMatch(outputContent, $@"\b{Regex.Escape(variableName)}\b"))
            .ToList();

        if (unresolvedVariables.Count != 0)
            return Result.Failure<ModuleTestBuildResult>(new Error(
                "ModuleTestContainsUnresolvedVariables",
                $"Test '{test.Name}' contains unresolved variables: {string.Join(", ", unresolvedVariables)}."));

        foreach (var pair in placeholders)
            outputContent = outputContent.Replace(pair.Value, $"args.{pair.Key}");

        var moduleTestScaffolding = GetTestScaffolding(test.Name, testedModulePathPlaceholder,
            $"{inputsLocationPlaceholder}/{moduleValuesName}.nix");

        var moduleTestContent = $"{moduleTestScaffolding.Prefix} {outputContent} {moduleTestScaffolding.Suffix}";

        return new ModuleTestBuildResult(
            test.Id,
            moduleTestContent,
            test.Name,
            testedModulePathPlaceholder,
            inputsLocationPlaceholder);
    }

    private Result<ModuleBuildResult> BuildModule(
        ModuleTemplate moduleTemplate,
        ModuleValue moduleValue,
        string moduleValuesName = "values")
    {
        var inputs = "{ ";
        var outputContent = moduleTemplate.Content;

        foreach (var value in moduleValue.EditableValues)
        {
            var definition = moduleTemplate.EditableValueTypes
                .FirstOrDefault(v => v.Placeholder == value.Placeholder);

            string? nixValue;
            if (value.Value == string.Empty)
            {
                nixValue = definition?.DefaultValue;
                if (nixValue is null && value.Kind == EntryValueKind.List)
                    nixValue = "[ ]";
            }
            else
            {
                nixValue = value.GetNixExpression();
            }

            if (nixValue is null)
                return Result.Failure<ModuleBuildResult>(new Error(
                    "Modules.MissingEntryValue",
                    $"Entry '{value.Placeholder}' has no value and no default value is configured."));

            inputs += $"{value.Placeholder} = {nixValue};";
            outputContent = outputContent.Replace(value.Placeholder, $"args.{value.Placeholder}");
        }

        inputs += " }";

        var inputsLocationPlaceholder = Guid.NewGuid().ToString();

        var scaffolding =
            GetModuleScaffolding(moduleTemplate.Type, $"{inputsLocationPlaceholder}/{moduleValuesName}.nix");
        var moduleContent = $"{scaffolding.Prefix} {outputContent} {scaffolding.Suffix}";

        var moduleTests = moduleTemplate.Tests
            .Select(t => BuildModuleTest(t, moduleValuesName))
            .ToList();

        var moduleTestFailure = moduleTests.FirstOrDefault(t => t.IsFailure);
        if (moduleTestFailure is not null && moduleTestFailure.IsFailure)
            return Result.Failure<ModuleBuildResult>(moduleTestFailure.Error);

        return new ModuleBuildResult(
            moduleTemplate.Id,
            moduleTemplate.Name,
            moduleContent,
            inputs,
            moduleValuesName,
            inputsLocationPlaceholder,
            moduleTests.Select(t => t.Value).ToList());
    }

    private Result<SystemBuildResult> BuildSystem(
        Domain.Entities.Systems.System system,
        List<ModuleTemplate> moduleTemplates,
        BuiltInModuleParameters? builtInModules)
    {
        var modules = system.Modules
            .Where(m => m.Enabled)
            .Select(m => BuildModule(moduleTemplates.First(i => i.Id == m.ModuleTemplateId), m))
            .ToList();

        var moduleFailure = modules.FirstOrDefault(m => m.IsFailure);
        if (moduleFailure is not null && moduleFailure.IsFailure)
            return Result.Failure<SystemBuildResult>(moduleFailure.Error);

        var moduleResults = modules.Select(m => m.Value).ToList();
        moduleResults.AddRange(BuildBuiltInSystemModules(builtInModules));

        var modulesListPlaceholder = Guid.NewGuid().ToString();

        var systemContent =
            "{ inputs, lib, sharedModules }:\n" +
            $"inputs.nixpkgs.lib.nixosSystem {{ specialArgs = {{ inherit inputs; }}; system = \"{system.Architecture.ToArchitectureString()}\"; modules = sharedModules ++ [ {modulesListPlaceholder} ]; }}";

        return new SystemBuildResult(
            system.Id,
            system.Name,
            system.Architecture,
            systemContent,
            moduleResults,
            modulesListPlaceholder);
    }

    public Result<ConfigurationBuildResult> MaterializeConfiguration(
        Configuration configuration,
        IEnumerable<ModuleTemplate> templates,
        SystemId? systemId = null,
        BuiltInModuleParameters? builtInModules = null)
    {
        var templateList = templates.ToList();

        var inputs = configuration.Inputs
            .Select(BuildInput)
            .ToList();

        var inputFailure = inputs.FirstOrDefault(i => i.IsFailure);
        if (inputFailure is not null && inputFailure.IsFailure)
            return Result.Failure<ConfigurationBuildResult>(inputFailure.Error);

        var modules = configuration.Modules
            .Where(m => m.Enabled)
            .Select(m => BuildModule(templateList.First(i => i.Id == m.ModuleTemplateId), m))
            .ToList();

        var moduleFailure = modules.FirstOrDefault(m => m.IsFailure);
        if (moduleFailure is not null && moduleFailure.IsFailure)
            return Result.Failure<ConfigurationBuildResult>(moduleFailure.Error);

        var systemsToBuild = systemId is null
            ? configuration.SystemSpecifications.ToList()
            : configuration.SystemSpecifications.Where(s => s.Id == systemId).ToList();

        if (systemsToBuild.Count == 0)
            return Result.Failure<ConfigurationBuildResult>(new Error(
                "SystemNotFound",
                $"System '{systemId}' not found in configuration."));

        var systems = systemsToBuild
            .Select(s => BuildSystem(s, templateList, builtInModules))
            .ToList();

        var systemFailure = systems.FirstOrDefault(s => s.IsFailure);
        if (systemFailure is not null && systemFailure.IsFailure)
            return Result.Failure<ConfigurationBuildResult>(systemFailure.Error);

        var supportedArchitectures = configuration.SupportedSystemArchitectures();
        if (supportedArchitectures.Count == 0)
            return Result.Failure<ConfigurationBuildResult>(new Error(
                "",
                $"Failed to get supported architectures for configuration {configuration.Title}"));

        var systemsPlaceholder = Guid.NewGuid().ToString();
        var sharedModulesPlaceholder = Guid.NewGuid().ToString();
        var checksPlaceholder = Guid.NewGuid().ToString();

        var inputsValues = inputs.Aggregate(string.Empty, (current, result) => current + $"{result.Value.Input}\n");

        var content =
            $"{{ description = \"{configuration.Description}\"; " +
            $"inputs = {{ flake-utils.url = \"github:numtide/flake-utils\"; disko.url = \"github:nix-community/disko/latest\"; disko.inputs.nixpkgs.follows = \"nixpkgs\"; {inputsValues} }};\n" +
            "outputs = {self, nixpkgs, flake-utils, ...} @ inputs: " +
            "let\n" +
            $"systems = [{supportedArchitectures.Aggregate(string.Empty, (s, architecture) => $"\"{s + architecture.ToArchitectureString()}\" ")}];" +
            $"sharedModules = [ {sharedModulesPlaceholder} ];\n" +
            "lib = nixpkgs.lib;" +
            "\nin\n" +
            "flake-utils.lib.eachSystem systems (system: let " +
            "pkgs = nixpkgs.legacyPackages.${system}; \n" +
            "in {\n" +
            "formatter = pkgs.nixfmt;\n" +
            $"checks = {{ {checksPlaceholder} }};" +
            $"}}) // {{ nixosConfigurations = {{ {systemsPlaceholder} }}; }}; }}";

        return new ConfigurationBuildResult(
            configuration.Id,
            configuration.Title,
            content,
            sharedModulesPlaceholder,
            systemsPlaceholder,
            checksPlaceholder,
            supportedArchitectures,
            modules.Select(m => m.Value).ToList(),
            systems.Select(s => s.Value).ToList());
    }
}