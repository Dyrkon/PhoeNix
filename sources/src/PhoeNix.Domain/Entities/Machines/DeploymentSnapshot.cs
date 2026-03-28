using System.Net;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Machines;

public sealed class DeploymentSnapshot
{
    private readonly List<DeploymentDiskBinding> _boundDisks = [];

    private DeploymentSnapshot()
    {
    }

    public ConfigurationId ConfigurationId { get; private set; }

    public SystemId SystemId { get; private set; }

    public IPAddress LastKnownIpAddress { get; private set; }

    public DateTime ProvisionedAtUtc { get; private set; }

    public IReadOnlyCollection<DeploymentDiskBinding> BoundDisks => _boundDisks;

    public static Result<DeploymentSnapshot> Create(
        ConfigurationId configurationId,
        SystemId systemId,
        IPAddress lastKnownIpAddress,
        DateTime provisionedAtUtc,
        IReadOnlyList<string> boundDiskPaths)
    {
        var snapshot = new DeploymentSnapshot
        {
            ConfigurationId = configurationId,
            SystemId = systemId,
            LastKnownIpAddress = lastKnownIpAddress,
            ProvisionedAtUtc = provisionedAtUtc
        };

        for (var i = 0; i < boundDiskPaths.Count; i++)
        {
            var bindingResult = DeploymentDiskBinding.Create(i, boundDiskPaths[i]);
            if (bindingResult.IsFailure)
                return Result.Failure<DeploymentSnapshot>(bindingResult.Error);

            snapshot._boundDisks.Add(bindingResult.Value);
        }

        return Result.Success(snapshot);
    }
}

public sealed class DeploymentDiskBinding
{
    private DeploymentDiskBinding()
    {
    }

    public int Index { get; private set; }

    public string StableDevicePath { get; private set; } = string.Empty;

    public static Result<DeploymentDiskBinding> Create(int index, string stableDevicePath)
    {
        if (index < 0)
            return Result.Failure<DeploymentDiskBinding>(new Error(
                "DeploymentDiskBindingIndexInvalid",
                "Deployment disk binding index must be greater than or equal to zero."));

        if (string.IsNullOrWhiteSpace(stableDevicePath))
            return Result.Failure<DeploymentDiskBinding>(new Error(
                "DeploymentDiskBindingPathMissing",
                "Deployment disk binding path must not be empty."));

        return Result.Success(new DeploymentDiskBinding
        {
            Index = index,
            StableDevicePath = stableDevicePath
        });
    }
}