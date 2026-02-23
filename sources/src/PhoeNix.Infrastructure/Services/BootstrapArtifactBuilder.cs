using System.Text;
using System.IO;
using Microsoft.Extensions.Options;
using PhoeNix.Application.Abstractions.Bootstrap;
using PhoeNix.Application.Abstractions.Processes;
using PhoeNix.Application.Models.Bootstrap;
using PhoeNix.Domain.Entities.ProvisioningSessions;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Options;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services;

public sealed class BootstrapArtifactBuilder(
    IProcessRunner processRunner,
    IOptions<BootstrapArtifactsOptions> options) : IBootArtifactBuilder
{
    private const string DefaultWorkdir = "phoenix-bootstrap";
    private readonly BootstrapArtifactsOptions _options = options.Value;

    public async Task<Result<BootArtefactDescriptor>> BuildAsync(
        BootstrapBuildRequest request,
        IBootstrapProgressSink progress,
        CancellationToken cancellationToken)
    {
        await progress.ReportAsync(new BootstrapBuildProgress(ProvisioningStage.Create, "Preparing bootstrap overlay"),
            cancellationToken);

        var baseImage = _options.BaseImages.FirstOrDefault(i => i.Architecture == request.Architecture);
        if (baseImage is null)
            return Result.Failure<BootArtefactDescriptor>(new Error(
                "BootstrapBaseImageMissing",
                $"No base boot image registered for {request.Architecture}."));

        var baseWorkDir = ResolveStagingRoot();

        var overlayRoot = Path.Combine(
            baseWorkDir,
            request.SessionId.Value.ToString("N"),
            request.MachineId.Value.ToString("N"));

        var stagingPath = Path.Combine(overlayRoot, "staging");
        var overlayCpioPath = Path.Combine(overlayRoot, "overlay.cpio");
        var overlayGzipPath = overlayCpioPath + ".gz";
        var combinedInitrdPath = Path.Combine(overlayRoot, "initrd");

        if (Directory.Exists(overlayRoot))
            Directory.Delete(overlayRoot, true);
        Directory.CreateDirectory(stagingPath);

        var writeFiles = await WriteOverlayFilesAsync(stagingPath, request, cancellationToken);
        if (writeFiles.IsFailure) return Result.Failure<BootArtefactDescriptor>(writeFiles.Error);

        var cpio = processRunner.RunProcess(
            _options.CpioExecutable,
            ["-o", "-H", "newc", "-O", overlayCpioPath, "-0"],
            cancellationToken,
            stagingPath,
            BuildManifest(stagingPath));

        if (cpio.IsFailure) return Result.Failure<BootArtefactDescriptor>(cpio.Error with { Code = "BootstrapCpioFailed" });

        var gzip = processRunner.RunProcess(
            _options.GzipExecutable,
            ["-f", overlayCpioPath],
            cancellationToken);
        if (gzip.IsFailure) return Result.Failure<BootArtefactDescriptor>(gzip.Error with { Code = "BootstrapGzipFailed" });

        var combine = CombineInitrds(baseImage.InitrdPath, overlayGzipPath, combinedInitrdPath);
        if (combine.IsFailure) return Result.Failure<BootArtefactDescriptor>(combine.Error);

        var storePathResult = AddToStore(combinedInitrdPath, cancellationToken);
        if (storePathResult.IsFailure)
            return Result.Failure<BootArtefactDescriptor>(storePathResult.Error);

        var cmdline = BuildCommandLine(baseImage.KernelParams, request);
        var descriptor = new BootArtefactDescriptor(baseImage.KernelPath, storePathResult.Value, cmdline);

        await progress.ReportAsync(
            new BootstrapBuildProgress(ProvisioningStage.ArtefactsBuilt, "Bootstrap artefact ready"),
            cancellationToken);

        TryCleanUp(overlayRoot);

        return Result.Success(descriptor);
    }

    private async Task<Result> WriteOverlayFilesAsync(
        string stagingPath,
        BootstrapBuildRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            async Task Write(string relativePath, string content)
            {
                var fullPath = Path.Combine(stagingPath, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                await File.WriteAllTextAsync(fullPath, content, cancellationToken);
            }

            if (!File.Exists(request.SshIdentity.CertificatePath) || !File.Exists(request.SshIdentity.PublicKeyPath))
                return Result.Failure(new Error(
                    "BootstrapSshMaterialMissing",
                    "SSH certificate or public key is missing for bootstrap overlay."));

            await Write("etc/phoenix/callback-token", request.CallbackToken);
            await Write("etc/phoenix/session-id", request.SessionId.Value.ToString("D"));
            await Write("etc/phoenix/machine-id", request.MachineId.Value.ToString("D"));

            var cert = await File.ReadAllTextAsync(request.SshIdentity.CertificatePath, cancellationToken);
            await Write("etc/phoenix/session-cert.pub", cert);

            var pub = await File.ReadAllTextAsync(request.SshIdentity.PublicKeyPath, cancellationToken);
            await Write("etc/phoenix/session.pub", pub);

            if (!string.IsNullOrWhiteSpace(request.UserAuthorizedKey))
            {
                var normalizedKey = request.UserAuthorizedKey.Trim();
                await Write("root/.ssh/authorized_keys", normalizedKey + Environment.NewLine);
            }

            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(new Error("BootstrapOverlayWriteFailed", e.Message));
        }
    }

    private static string BuildManifest(string stagingPath)
    {
        var files = Directory.GetFiles(stagingPath, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(stagingPath, path).Replace("\\", "/"))
            .ToArray();

        var manifest = new StringBuilder();
        foreach (var file in files) manifest.Append(file).Append('\0');

        return manifest.ToString();
    }

    private static Result CombineInitrds(string baseInitrdPath, string overlayPath, string outputPath)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

            using var output = File.Create(outputPath);
            using (var baseStream = File.OpenRead(baseInitrdPath))
            {
                baseStream.CopyTo(output);
            }

            using (var overlayStream = File.OpenRead(overlayPath))
            {
                overlayStream.CopyTo(output);
            }

            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(new Error("BootstrapCombineInitrdFailed", e.Message));
        }
    }

    private Result<string> AddToStore(string path, CancellationToken cancellationToken)
    {
        var add = processRunner.RunProcess(
            _options.NixStoreExecutable,
            ["--add", path],
            cancellationToken);

        if (add.IsFailure) return Result.Failure<string>(add.Error with { Code = "BootstrapStoreAddFailed" });

        var storePath = add.Value.StandardOutput
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .LastOrDefault();

        if (string.IsNullOrWhiteSpace(storePath))
            return Result.Failure<string>(new Error(
                "BootstrapStoreAddFailed",
                "Could not determine store path for composed initrd."));

        return Result.Success(storePath);
    }

    private static string BuildCommandLine(string baseParams, BootstrapBuildRequest request)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(baseParams)) parts.Add(baseParams);

        parts.Add($"phoenix.session-id={request.SessionId.Value:D}");
        parts.Add($"phoenix.machine-id={request.MachineId.Value:D}");
        parts.Add($"phoenix.callback-token={request.CallbackToken}");

        if (!string.IsNullOrWhiteSpace(request.AdditionalKernelParams))
            parts.Add(request.AdditionalKernelParams);

        return string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }

    private string ResolveStagingRoot()
    {
        var configured = string.IsNullOrWhiteSpace(_options.WorkDirectory)
            ? DefaultWorkdir
            : _options.WorkDirectory;

        if (Path.IsPathRooted(configured))
            return configured;

        var trimmed = configured.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return Path.Combine(Path.GetTempPath(), trimmed);
    }

    private static void TryCleanUp(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
        catch
        {
        }
    }
}
