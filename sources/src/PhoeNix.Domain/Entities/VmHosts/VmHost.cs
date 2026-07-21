using System.Text.RegularExpressions;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.VmHosts;

public class VmHost : AggregateRoot<VmHostId>
{
    private static readonly Regex NameRegex =
        new(@"^[a-zA-Z0-9]([a-zA-Z0-9._-]*[a-zA-Z0-9])?$", RegexOptions.Compiled);

    private VmHost(VmHostId id) : base(id)
    {
    }

    public UserId OwnerId { get; private set; } = default!;

    public string Name { get; private set; } = default!;

    public VmHostProvider Provider { get; private set; }

    public VmHostCredential Credential { get; private set; } = default!;

    public VmHostResources? Resources { get; private set; }

    public DateTime? LastSyncedAtUtc { get; private set; }

    public bool Enabled { get; private set; }

    public Result ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 128 || !NameRegex.IsMatch(name))
            return Result.Failure(new Error(
                "VmHost.InvalidName",
                $"VM host name '{name}' is invalid. Use alphanumeric characters, dots, hyphens, and underscores (max 128 chars)."));

        Name = name;
        return Result.Success();
    }

    public Result UpdateCredential(VmHostCredential credential)
    {
        if (string.IsNullOrWhiteSpace(credential.Host))
            return Result.Failure(new Error(
                "VmHost.CredentialHostRequired",
                "VM host credential must have a host address."));

        Credential = credential;
        return Result.Success();
    }

    public Result UpdateResources(VmHostResources resources, DateTime nowUtc)
    {
        Resources = resources;
        LastSyncedAtUtc = nowUtc;
        return Result.Success();
    }

    public Result Enable()
    {
        if (Enabled)
            return Result.Failure(new Error(
                "VmHost.AlreadyEnabled",
                $"VM host '{Name}' is already enabled."));

        Enabled = true;
        return Result.Success();
    }

    public Result Disable()
    {
        if (!Enabled)
            return Result.Failure(new Error(
                "VmHost.AlreadyDisabled",
                $"VM host '{Name}' is already disabled."));

        Enabled = false;
        return Result.Success();
    }

    public static Result<VmHost> Create(
        VmHostId id,
        UserId ownerId,
        string name,
        VmHostProvider provider,
        VmHostCredential credential)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 128 || !NameRegex.IsMatch(name))
            return Result.Failure<VmHost>(new Error(
                "VmHost.InvalidName",
                $"VM host name '{name}' is invalid. Use alphanumeric characters, dots, hyphens, and underscores (max 128 chars)."));

        if (string.IsNullOrWhiteSpace(credential.Host))
            return Result.Failure<VmHost>(new Error(
                "VmHost.CredentialHostRequired",
                "VM host credential must have a host address."));

        return Result.Success(new VmHost(id)
        {
            OwnerId = ownerId,
            Name = name,
            Provider = provider,
            Credential = credential,
            Enabled = true
        });
    }
}
