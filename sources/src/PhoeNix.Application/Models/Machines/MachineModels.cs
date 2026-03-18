using FluentValidation;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.Models.Machines;

public record CreateMachineRequest(
    string Title,
    bool Enabled,
    string MacAddress,
    Architecture Architecture,
    InstallDiskSelectionPreference InstallDiskSelectionPreference);

public sealed class CreateMachineRequestValidator : AbstractValidator<CreateMachineRequest>
{
    public CreateMachineRequestValidator()
    {
        // TODO
    }
}