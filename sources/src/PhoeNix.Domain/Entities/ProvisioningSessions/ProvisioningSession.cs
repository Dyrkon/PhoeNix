using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.ProvisioningSessions;

public class ProvisioningSession : AggregateRoot<ProvisioningSessionId>
{
    private ProvisioningSession(ProvisioningSessionId id) : base(id)
    {
    }

    public bool ReadyForProvisioning => BootArtefactDescriptor is not null && BootstrapCredential is not null &&
                                        CallbackToken is not null;

    public ProvisioningStage ProvisioningStage { get; private set; }
    public BootArtefactDescriptor? BootArtefactDescriptor { get; private set; }
    public BootstrapCredential? BootstrapCredential { get; private set; }
    public CallbackToken? CallbackToken { get; private set; }

    public Result AssignBootstrapArtefact(string kernelLocation, string initRdLocation, string cmdLine)
    {
        if (kernelLocation.First() != '/')
            return Result.Failure(new Error("ProvisioningSessionIncorrectArtefact",
                $"Kernel location has to be absolute path."));
        if (initRdLocation.First() != '/')
            return Result.Failure(new Error("ProvisioningSessionIncorrectArtefact",
                $"Ramdisk location has to be absolute path."));
        if (kernelLocation.Split('/').ToList().Slice(1, 2) is not ["nix", "store"])
            return Result.Failure(new Error("ProvisioningSessionIncorrectArtefact",
                $"Kernel location has to be a store path."));
        if (initRdLocation.Split('/').ToList().Slice(1, 2) is not ["nix", "store"])
            return Result.Failure(new Error("ProvisioningSessionIncorrectArtefact",
                $"Ramdisk location has to be a store path."));

        BootArtefactDescriptor = new BootArtefactDescriptor(kernelLocation, initRdLocation, cmdLine);
        return Result.Success();
    }

    public static Result<ProvisioningSession> Create(ProvisioningSessionId id)
    {
        return new ProvisioningSession(id)
        {
            ProvisioningStage = ProvisioningStage.Create
        };
    }
}