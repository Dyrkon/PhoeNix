using PhoeNix.Domain.Entities.VmHosts;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Domain.Entities.Machines;

public class ManagementProfile
{
    private ManagementProfile()
    {
    }

    public VmHostId VmHostId { get; private set; } = default!;

    public string ExternalId { get; private set; } = default!;

    public VmPowerState PowerState { get; private set; }

    public DateTime? LastPowerStateCheckUtc { get; private set; }

    public void UpdatePowerState(VmPowerState state, DateTime nowUtc)
    {
        PowerState = state;
        LastPowerStateCheckUtc = nowUtc;
    }

    public static ManagementProfile Create(VmHostId vmHostId, string externalId)
    {
        return new ManagementProfile
        {
            VmHostId = vmHostId,
            ExternalId = externalId,
            PowerState = VmPowerState.Unknown
        };
    }
}
