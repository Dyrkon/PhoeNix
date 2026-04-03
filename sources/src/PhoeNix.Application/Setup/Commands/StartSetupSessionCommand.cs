using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Bootstrap;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Setup.Commands;

public record StartSetupSessionCommand() : ICommand<string>;

internal sealed class StartSetupSessionCommandHandler(
    ISetupSessionRepository setupSessionRepository,
    INetbootHostService netbootHostService,
    IBootstrapImageBuilder bootstrapImageBuilder,
    ISetupSshKeyProvider sshKeyProvider)
    : ICommandHandler<StartSetupSessionCommand, string>
{
    public async Task<Result<string>> Handle(
        StartSetupSessionCommand request,
        CancellationToken cancellationToken)
    {
        var sessionResult = SetupSession.Create(new SetupSessionId(Guid.NewGuid()), DateTime.UtcNow);
        if (sessionResult.IsFailure)
            return sessionResult.Error.Description;

        var session = sessionResult.Value;

        var sshResult = await sshKeyProvider.GetOrCreateAsync(session, cancellationToken);
        if (sshResult.IsFailure)
            return sshResult.Error.Description;

        var assignSshResult = session.AssignSshCredential(
            new SshCredential(
                await File.ReadAllTextAsync(sshResult.Value.PublicKeyPath, cancellationToken),
                await File.ReadAllTextAsync(sshResult.Value.CertificatePath, cancellationToken),
                sshResult.Value.ExpiresAtUtc,
                null),
            DateTime.UtcNow);

        if (assignSshResult.IsFailure)
            return assignSshResult.Error.Description;

        // Todo machines need to have preset architecture
        var imageResult = await bootstrapImageBuilder.BuildAsync(Architecture.X86Linux, cancellationToken);
        if (imageResult.IsFailure)
            return imageResult.Error.Description;

        var assignArtefactResult = session.AssignBootstrapArtefact(
            imageResult.Value.Kernel,
            imageResult.Value.RamDisk,
            imageResult.Value.Init);

        if (assignArtefactResult.IsFailure)
            return assignArtefactResult.Error.Description;

        var startHostResult = await netbootHostService.StartAsync(cancellationToken);
        if (startHostResult.IsFailure)
            return startHostResult.Error.Description;

        setupSessionRepository.Add(session);

        return Result.Success(session.Id.Value.ToString());
    }
}