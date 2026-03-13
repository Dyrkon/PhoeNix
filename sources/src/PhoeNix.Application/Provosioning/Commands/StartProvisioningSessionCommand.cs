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
    IProvisioningSessionRepository provisioningSessionRepository,
    IMachineRepository machineRepository,
    INetbootHostService netbootHostService,
    ICallbackTokenService callbackTokenService,
    IBootstrapImageBuilder bootstrapImageBuilder,
    ISshKeyProvider sshKeyProvider)
    : ICommandHandler<StartProvisioningSessionCommand, string>
{
    public async Task<Result<string>> Handle(
        StartProvisioningSessionCommand request,
        CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;
        var machine = await machineRepository.GetByIdAsync(request.MachineId, cancellationToken)
            .EnsureNotNull(new Error("MachineNotFound"));

        if (machine.IsFailure)
            return machine.Error.Description;

        var sessionResult = ProvisioningSession.Create(new ProvisioningSessionId(Guid.NewGuid()))
            .Tap(session => sshKeyProvider.GetOrCreateAsync(session, cancellationToken));
        if (sessionResult.IsFailure)
            return sessionResult.Error.Description;

        var session = sessionResult.Value;

        var enroll = session.EnrollMachine(machine.Value.Id, DateTime.UtcNow);
        if (enroll.IsFailure)
            return enroll.Error.Description;

        session.UpdateMachineStage(machine.Value.Id, ProvisioningStage.WaitingForPxe);

        // Todo machines have to have set architecture
        var architecture = Architecture.X86Linux;
        var imageResult = await bootstrapImageBuilder.BuildAsync(architecture, cancellationToken);
        if (imageResult.IsFailure)
            return Result.Failure<string>(imageResult.Error);

        var tokenResult = callbackTokenService.Create(
            session.Id,
            machine.Value.Id,
            nowUtc,
            TimeSpan.FromMinutes(10));

        if (tokenResult.IsFailure)
            return Result.Failure<string>(tokenResult.Error);

        var token = tokenResult.Value;
        session.AssignMachineCallbackToken(machine.Value.Id, token);
        session.AssignBootstrapArtefact(imageResult.Value.Kernel, imageResult.Value.RamDisk, imageResult.Value.Init);

        await netbootHostService.StartAsync(cancellationToken);

        provisioningSessionRepository.Add(session);

        return Result.Success(session.Id.Value.ToString());
    }
}