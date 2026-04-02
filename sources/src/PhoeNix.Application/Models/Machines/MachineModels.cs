using System.Net.NetworkInformation;
using FluentValidation;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.Models.Machines;

public record CreateMachineRequest(
    string Title,
    bool Enabled,
    string MacAddress,
    Architecture Architecture,
    InstallDiskSelectionPreference InstallDiskSelectionPreference);

public record MachineListResponse(
    Guid Id,
    string Title,
    bool Enabled,
    string MacAddress,
    Architecture Architecture,
    MachineState MachineState);

public sealed class CreateMachineRequestValidator : AbstractValidator<CreateMachineRequest>
{
    public CreateMachineRequestValidator()
    {
        // TODO
    }
}