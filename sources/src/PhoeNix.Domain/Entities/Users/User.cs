using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Users;

public class User : AggregateRoot<UserId>
{
    private readonly List<string> _userSshKeys;

    private User(UserId id) : base(id)
    {
    }

    public string Name { get; private set; }

    public IReadOnlyList<string> UserSshKeys => _userSshKeys;

    public Result AddSshKey(string sshKey)
    {
        if (_userSshKeys.Contains(sshKey))
            return Result.Failure(new Error("UserSshKeyAddedAlready", $"Ssh key: {sshKey} already exists."));
        _userSshKeys.Add(sshKey);
        return Result.Success();
    }

    public Result RemoveSshKey(string sshKey)
    {
        var removed = _userSshKeys.RemoveAll(s => s == sshKey);
        if (removed == 0)
            return Result.Failure(new Error("UserSshKeyNotFound", $"Ssh key: {sshKey} does not exist."));
        return Result.Success();
    }

    public static Result<User> Create(UserId id, string name)
    {
        return new User(id) { Name = name };
    }
}