using System.Linq;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Bootstrap;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Models.Bootstrap;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.ProvisioningSessions;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Provosioning.Commands;

public record StartProvisioningSessionCommand(MachineId MachineId) : ICommand<string>;

internal sealed class StartProvisioningSessionCommandHandler(
    ISshKeyProvider sshKeyProvider,
    IProvisioningSessionRepository provisioningSessionRepository,
    IMachineRepository machineRepository,
    ICallbackTokenService callbackTokenService,
    IBootArtifactBuilder bootArtifactBuilder,
    INetbootHostService netbootHostService)
    : ICommandHandler<StartProvisioningSessionCommand, string>
{
    public async Task<Result<string>> Handle(StartProvisioningSessionCommand request,
        CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;
        var machine = await machineRepository.GetByIdAsync(request.MachineId, cancellationToken)
            .EnsureNotNull(new Error("MachineNotFound"));

        if (machine.IsFailure) return machine.Error.Description;

        var sessionResult = ProvisioningSession.Create(new ProvisioningSessionId(Guid.NewGuid()));
        if (sessionResult.IsFailure) return sessionResult.Error.Description;

        var session = sessionResult.Value;

        var ssh = await sshKeyProvider.GetOrCreateAsync(session, cancellationToken);
        if (ssh.IsFailure) return ssh.Error.Description;

        var tokenResult = callbackTokenService.Create(session.Id, request.MachineId, nowUtc, TimeSpan.FromMinutes(10));
        if (tokenResult.IsFailure) return tokenResult.Error.Description;

        var token = tokenResult.Value;
        var enroll = session.EnrollMachine(machine.Value.Id, token, nowUtc);
        if (enroll.IsFailure) return enroll.Error.Description;

        session.UpdateMachineStage(request.MachineId, ProvisioningStage.SecretsGenerated);

        var progressSink = new SessionBootstrapProgressSink(session, request.MachineId);
        var buildRequest = new BootstrapBuildRequest(
            session.Id,
            request.MachineId,
            Architecture.X86Linux,
            ssh.Value,
            token.Token,
            null,
            null);

        var artefact = await bootArtifactBuilder.BuildAsync(buildRequest, progressSink, cancellationToken);
        if (artefact.IsFailure) return artefact.Error.Description;

        var assignArtefact = session.AssignBootstrapArtefact(
            artefact.Value.KernelLocation,
            artefact.Value.InitRdLocation,
            artefact.Value.Cmdline);
        if (assignArtefact.IsFailure) return assignArtefact.Error.Description;

        var startHost = await netbootHostService.StartAsync(session.Id, artefact.Value, cancellationToken);
        if (startHost.IsFailure) return startHost.Error.Description;

        session.UpdateMachineStage(request.MachineId, ProvisioningStage.WaitingForPxe);

        provisioningSessionRepository.Add(session);

        return Result.Success(session.Id.Value.ToString());
    }

    private sealed class SessionBootstrapProgressSink(ProvisioningSession session, MachineId machineId)
        : IBootstrapProgressSink
    {
        public Task ReportAsync(BootstrapBuildProgress progress, CancellationToken cancellationToken)
        {
            var target = session.Targets.FirstOrDefault(t => t.MachineId == machineId);
            if (target is null) return Task.CompletedTask;

            if ((int)progress.Stage <= (int)target.Stage)
                return Task.CompletedTask;

            session.UpdateMachineStage(machineId, progress.Stage);
            return Task.CompletedTask;
        }
    }
}