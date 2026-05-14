using System.Net;
using PhoeNix.Application.Models.SshIdentity;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.Systems;

namespace PhoeNix.Application.Models.Deployment;

public record DeploymentJob(
    MachineId MachineId,
    IPAddress TargetIpAddress,
    string TargetHostname,
    string FlakeDirectory,
    string SystemAttribute,
    DeploySshAccessMaterial SshMaterial,
    ConfigurationId ConfigurationId,
    string ConfigurationTitle,
    SystemId SystemId,
    string SystemName,
    List<string> BoundDiskPaths);

public enum DeploymentJobState
{
    None,
    Queued,
    Running,
    Succeeded,
    Failed
}

public record DeploymentJobStatus(
    DeploymentJobState State,
    string? ErrorCode = null,
    string? ErrorMessage = null);