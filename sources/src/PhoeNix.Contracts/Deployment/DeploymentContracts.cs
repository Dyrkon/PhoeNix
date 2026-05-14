using FluentValidation;

namespace PhoeNix.Contracts.Deployment;

public record UpdateMachineRequest(Guid ConfigurationId, Guid SystemId, Guid MachineId);

public record DeploymentStatusResponse(string State, string? ErrorCode, string? ErrorMessage);

public sealed class UpdateMachineRequestValidator : AbstractValidator<UpdateMachineRequest>
{
    public UpdateMachineRequestValidator()
    {
        // TODO
    }
}