using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Setup.Commands;

public record StartSetupSessionCommand() : ICommand<string>;

internal sealed class StartSetupSessionCommandHandler(
    ISetupSessionRepository setupSessionRepository,
    ISetupSshKeyProvider sshKeyProvider)
    : ICommandHandler<StartSetupSessionCommand, string>
{
    public async Task<Result<string>> Handle(
        StartSetupSessionCommand request,
        CancellationToken cancellationToken)
    {
        var sessionResult = SetupSession.Create(new SetupSessionId(Guid.NewGuid()), DateTime.UtcNow);
        if (sessionResult.IsFailure)
            return Result.Failure<string>(sessionResult.Error);

        var session = sessionResult.Value;

        var sshResult = await sshKeyProvider.GetOrCreateAsync(session, cancellationToken);
        if (sshResult.IsFailure)
            return Result.Failure<string>(sshResult.Error);

        var assignSshResult = session.AssignSshCredential(
            new SshCredential(
                await File.ReadAllTextAsync(sshResult.Value.PublicKeyPath, cancellationToken),
                await File.ReadAllTextAsync(sshResult.Value.CertificatePath, cancellationToken),
                sshResult.Value.ExpiresAtUtc,
                null),
            DateTime.UtcNow);

        if (assignSshResult.IsFailure)
            return Result.Failure<string>(assignSshResult.Error);

        setupSessionRepository.Add(session);

        return Result.Success(session.Id.Value.ToString());
    }
}
