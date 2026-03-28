using FluentValidation;

namespace PhoeNix.Application.Models.Deployment;

public record UpdateMachineRequest(Guid ConfigurationId, Guid SystemId, Guid MachineId);

public sealed class CreateMachineRequestValidator : AbstractValidator<UpdateMachineRequest>
{
    public CreateMachineRequestValidator()
    {
        // TODO
    }
}