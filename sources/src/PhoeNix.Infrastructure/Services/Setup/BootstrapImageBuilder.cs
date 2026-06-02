using System.Text.Json;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Bootstrap;
using PhoeNix.Application.Abstractions.Processes;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services.Setup;

public sealed class BootstrapImageBuilder(
    IProcessRunner processRunner,
    ISshKeyFileStore sshKeyFileStore,
    IAppSettingsRepository settingsRepository)
    : IBootstrapImageBuilder
{
    public async Task<Result<BootstrapImageDescriptor>> BuildAsync(
        Architecture architecture,
        UserId ownerId,
        CancellationToken cancellationToken)
    {
        var caKeyResult = await sshKeyFileStore.ReadCaPublicKeyAsync(cancellationToken);
        if (caKeyResult.IsFailure)
            return Result.Failure<BootstrapImageDescriptor>(caKeyResult.Error);

        if (architecture != Architecture.X86Linux && architecture != Architecture.Aarch64Linux)
            return Result.Failure<BootstrapImageDescriptor>(new Error(
                "BootstrapArchitectureUnsupported",
                $"Architecture '{architecture}' is not supported by the bootstrap image builder."));

        var settings = await settingsRepository.GetAsync(ownerId, cancellationToken);
        if (settings is null)
            return Result.Failure<BootstrapImageDescriptor>(new Error(
                "AppSettings.NotFound",
                "Application settings have not been initialized."));

        var arguments = new List<string>();

        if (settings.BootstrapUseSubstituters && settings.Substituters.Count > 0)
        {
            arguments.Add("--option");
            arguments.Add("extra-substituters");
            arguments.Add(string.Join(' ', settings.Substituters));

            arguments.Add("--option");
            arguments.Add("extra-trusted-public-keys");
            arguments.Add(string.Join(' ', settings.SubstituterKeys));
        }

        var environmentVariables = new Dictionary<string, string>
        {
            ["PHOENIX_USER_CA_PUBLIC_KEY"] = caKeyResult.Value,
            ["PHOENIX_TARGET_SYSTEM"] = architecture.ToArchitectureString()
        };

        var result = processRunner.RunProcess(
            "phoenix-create-pxe-image",
            arguments,
            cancellationToken,
            environmentVariables);

        if (result.IsFailure)
            return Result.Failure<BootstrapImageDescriptor>(result.Error with { Code = "BootstrapImageBuildFailed" });

        var json = result.Value.StandardOutput.Trim();
        if (string.IsNullOrWhiteSpace(json))
            return Result.Failure<BootstrapImageDescriptor>(new Error(
                "BootstrapImageBuildFailed",
                "Nix app did not return any JSON output."));

        try
        {
            var parsed = JsonSerializer.Deserialize<BootstrapImageAppResult>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (parsed is null)
                return Result.Failure<BootstrapImageDescriptor>(new Error(
                    "BootstrapImageParseFailed",
                    "Nix app returned empty or invalid JSON."));

            if (string.IsNullOrWhiteSpace(parsed.Kernel) ||
                string.IsNullOrWhiteSpace(parsed.RamDisk) ||
                string.IsNullOrWhiteSpace(parsed.Init) ||
                string.IsNullOrWhiteSpace(parsed.System))
                return Result.Failure<BootstrapImageDescriptor>(new Error(
                    "BootstrapImageParseFailed",
                    "Nix app JSON is missing one or more required fields: kernel, ramDisk, init, system."));

            return Result.Success(new BootstrapImageDescriptor(
                parsed.Kernel,
                parsed.RamDisk,
                parsed.Init));
        }
        catch (JsonException e)
        {
            return Result.Failure<BootstrapImageDescriptor>(new Error(
                "BootstrapImageParseFailed",
                $"Failed to parse Nix app JSON output: {e.Message}"));
        }
    }

    private sealed class BootstrapImageAppResult
    {
        public string Kernel { get; init; } = string.Empty;
        public string RamDisk { get; init; } = string.Empty;
        public string Init { get; init; } = string.Empty;
        public string System { get; init; } = string.Empty;
    }
}