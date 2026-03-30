using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Users;

public sealed class User : AggregateRoot<UserId>
{
    private readonly List<string> _userSshKeys = new();

    private User(UserId id) : base(id)
    {
    }

    public string Name { get; private set; } = string.Empty;

    public string NormalizedName { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public IReadOnlyList<string> UserSshKeys => _userSshKeys;

    public static string NormalizeName(string name)
    {
        return name.Trim().ToUpperInvariant();
    }

    public Result SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            return Result.Failure(new Error("UserPasswordHashMissing", "Password hash is required."));

        PasswordHash = passwordHash;
        return Result.Success();
    }

    public Result AddSshKey(string sshKey)
    {
        if (string.IsNullOrWhiteSpace(sshKey))
            return Result.Failure(new Error("UserSshKeyMissing", "SSH key is required."));

        var normalizedKey = sshKey.Trim();

        if (_userSshKeys.Contains(normalizedKey))
            return Result.Failure(new Error("UserSshKeyAddedAlready", $"Ssh key: {normalizedKey} already exists."));

        _userSshKeys.Add(normalizedKey);
        return Result.Success();
    }

    public Result RemoveSshKey(string sshKey)
    {
        var normalizedKey = sshKey.Trim();
        var removed = _userSshKeys.RemoveAll(x => x == normalizedKey);

        if (removed == 0)
            return Result.Failure(new Error("UserSshKeyNotFound", $"Ssh key: {normalizedKey} does not exist."));

        return Result.Success();
    }

    public static Result<User> Create(UserId id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<User>(new Error("UserNameMissing", "User name is required."));

        var trimmedName = name.Trim();

        return Result.Success(new User(id)
        {
            Name = trimmedName,
            NormalizedName = NormalizeName(trimmedName)
        });
    }
}