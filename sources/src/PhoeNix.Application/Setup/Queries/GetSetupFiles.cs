using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Setup.Queries;

public sealed record SetupFileResponse(
    string Path,
    string ContentType,
    string? DownloadName = null);

public record GetSetupFiles(
    SetupSessionId SessionId,
    BootFileType BootFileType) : IQuery<SetupFileResponse>;

internal sealed class GetSetupFilesHandler(
    ISetupSessionRepository setupSessionRepository)
    : IQueryHandler<GetSetupFiles, SetupFileResponse>
{
    public async Task<Result<SetupFileResponse>> Handle(
        GetSetupFiles request,
        CancellationToken cancellationToken)
    {
        var sessionResult = await setupSessionRepository
            .GetByIdAsync(request.SessionId, cancellationToken)
            .EnsureNotNull(new Error(
                "SetupSessionNotFound",
                $"Setup session '{request.SessionId.Value}' was not found."));

        if (sessionResult.IsFailure)
            return Result.Failure<SetupFileResponse>(sessionResult.Error);

        var session = sessionResult.Value;

        if (session.BootArtefactDescriptor is null)
            return Result.Failure<SetupFileResponse>(new Error(
                "SetupSessionBootArtefactMissing",
                $"Setup session '{request.SessionId.Value}' does not have a boot artefact assigned."));

        return request.BootFileType switch
        {
            BootFileType.Kernel => BuildKernelResponse(session),
            BootFileType.RamDisk => BuildRamDiskResponse(session),
            _ => Result.Failure<SetupFileResponse>(new Error(
                "SetupBootFileTypeUnsupported",
                $"Boot file type '{request.BootFileType}' is not supported."))
        };
    }

    private static Result<SetupFileResponse> BuildKernelResponse(SetupSession session)
    {
        var path = session.BootArtefactDescriptor!.Kernel;

        if (string.IsNullOrWhiteSpace(path))
            return Result.Failure<SetupFileResponse>(new Error(
                "SetupKernelPathMissing",
                "Kernel path is missing from the boot artefact descriptor."));

        if (!File.Exists(path))
            return Result.Failure<SetupFileResponse>(new Error(
                "SetupKernelPathNotFound",
                $"Kernel file '{path}' was not found."));

        return Result.Success(new SetupFileResponse(
            path,
            "application/octet-stream",
            "bzImage"));
    }

    private static Result<SetupFileResponse> BuildRamDiskResponse(SetupSession session)
    {
        var path = session.BootArtefactDescriptor!.RamDisk;

        if (string.IsNullOrWhiteSpace(path))
            return Result.Failure<SetupFileResponse>(new Error(
                "SetupInitrdPathMissing",
                "Initrd path is missing from the boot artefact descriptor."));

        if (!File.Exists(path))
            return Result.Failure<SetupFileResponse>(new Error(
                "SetupInitrdPathNotFound",
                $"Initrd file '{path}' was not found."));

        return Result.Success(new SetupFileResponse(
            path,
            "application/octet-stream",
            "initrd"));
    }
}