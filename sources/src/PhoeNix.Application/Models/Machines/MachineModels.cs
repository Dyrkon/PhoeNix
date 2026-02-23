using FluentValidation;

namespace PhoeNix.Application.Models.Machines;

public record CreateMachineRequest(string Title, bool Enabled, string MacAddress);

public sealed class CreateMachineRequestValidator : AbstractValidator<CreateMachineRequest>
{
    public CreateMachineRequestValidator()
    {
        // TODO
    }
}