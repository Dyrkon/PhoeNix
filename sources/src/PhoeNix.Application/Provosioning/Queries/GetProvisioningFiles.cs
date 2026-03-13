using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Domain.Entities.ProvisioningSessions;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Provosioning.Queries;

public sealed record ProvisioningFileResponse(
    string Path,
    string ContentType,
    string? DownloadName = null);

public record GetProvisioningFiles(
    ProvisioningSessionId SessionId,
    BootFileType BootFileType) : IQuery<ProvisioningFileResponse>;

internal sealed class GetProvisioningFilesHandler(
    IProvisioningSessionRepository provisioningSessionRepository)
    : IQueryHandler<GetProvisioningFiles, ProvisioningFileResponse>
{
    public async Task<Result<ProvisioningFileResponse>> Handle(
        GetProvisioningFiles request,
        CancellationToken cancellationToken)
    {
        var sessionResult = await provisioningSessionRepository
            .GetByIdAsync(request.SessionId, cancellationToken)
            .EnsureNotNull(new Error(
                "ProvisioningSessionNotFound",
                $"Provisioning session '{request.SessionId.Value}' was not found."));

        if (sessionResult.IsFailure)
            return Result.Failure<ProvisioningFileResponse>(sessionResult.Error);

        var session = sessionResult.Value;

        if (session.BootArtefactDescriptor is null)
            return Result.Failure<ProvisioningFileResponse>(new Error(
                "ProvisioningSessionBootArtefactMissing",
                $"Provisioning session '{request.SessionId.Value}' does not have a boot artefact assigned."));

        return request.BootFileType switch
        {
            BootFileType.Kernel => BuildKernelResponse(session),
            BootFileType.RamDisk => BuildRamDiskResponse(session),
            _ => Result.Failure<ProvisioningFileResponse>(new Error(
                "ProvisioningBootFileTypeUnsupported",
                $"Boot file type '{request.BootFileType}' is not supported."))
        };
    }

    private static Result<ProvisioningFileResponse> BuildKernelResponse(ProvisioningSession session)
    {
        var path = session.BootArtefactDescriptor!.Kernel;

        if (string.IsNullOrWhiteSpace(path))
            return Result.Failure<ProvisioningFileResponse>(new Error(
                "ProvisioningKernelPathMissing",
                "Kernel path is missing from the boot artefact descriptor."));

        if (!File.Exists(path))
            return Result.Failure<ProvisioningFileResponse>(new Error(
                "ProvisioningKernelPathNotFound",
                $"Kernel file '{path}' was not found."));

        return Result.Success(new ProvisioningFileResponse(
            path,
            "application/octet-stream",
            "bzImage"));
    }

    private static Result<ProvisioningFileResponse> BuildRamDiskResponse(ProvisioningSession session)
    {
        var path = session.BootArtefactDescriptor!.RamDisk;

        if (string.IsNullOrWhiteSpace(path))
            return Result.Failure<ProvisioningFileResponse>(new Error(
                "ProvisioningInitrdPathMissing",
                "Initrd path is missing from the boot artefact descriptor."));

        if (!File.Exists(path))
            return Result.Failure<ProvisioningFileResponse>(new Error(
                "ProvisioningInitrdPathNotFound",
                $"Initrd file '{path}' was not found."));

        return Result.Success(new ProvisioningFileResponse(
            path,
            "application/octet-stream",
            "initrd"));
    }
}