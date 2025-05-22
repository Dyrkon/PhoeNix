using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Users;

public class User : AggregateRoot<UserId>
{
    private readonly List<string> _extraGroups = new();

    public string Name { get; private set; }

    public string Description { get; private set; }

    public bool IsNormalUser { get; private set; }

    public string HomePath { get; private set; }

    public string Group { get; private set; }

    public uint Uid { get; private set; }

    public Shell Shell { get; private set; }

    public IReadOnlyList<string> ExtraGroups => _extraGroups;
    
    private User(UserId id) : base(id)
    {
    }
    
    public Result SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(new Error("User.InvalidName", "Name cannot be empty."));

        Name = name;
        return Result.Success();
    }

    public Result SetDescription(string description)
    {
        Description = description;
        return Result.Success();
    }

    public Result SetNormalUserStatus(bool isNormalUser)
    {
        IsNormalUser = isNormalUser;
        return Result.Success();
    }

    public Result SetHomeLocation(string location)
    {
        if (string.IsNullOrWhiteSpace(location))
            return Result.Failure(new Error("User.InvalidHomeLocation", "Home location cannot be empty."));
        
        HomePath = location;
        return Result.Success();
    }

    public Result SetGroup(string group)
    {
        if (string.IsNullOrWhiteSpace(group))
            return Result.Failure(new Error("User.InvalidGroup", "Group cannot be empty."));

        Group = group;
        return Result.Success();
    }

    public Result SetUid(uint uid)
    {
        if (uid > 999 || uid < 100)
            return Result.Failure(new Error("User.InvalidUID", $"UID {uid} out of range <100, 999>"));
        
        Uid = uid;
        return Result.Success();
    }

    public Result SetShell(Shell shell)
    {
        Shell = shell;
        return Result.Success();
    }

    public Result AddExtraGroup(string group)
    {
        if (string.IsNullOrWhiteSpace(group))
            return Result.Failure(new Error("User.InvalidExtraGroup", "Group name cannot be empty."));

        if (_extraGroups.Contains(group))
            return Result.Failure(new Error("User.DuplicateExtraGroup", "Group already exists."));

        _extraGroups.Add(group);
        return Result.Success();
    }

    public Result RemoveExtraGroup(string group)
    {
        var removed = _extraGroups.RemoveAll(g => g == group);
        if (removed == 0)
            return Result.Failure(new Error("User.ExtraGroupNotFound", "Group not found."));

        return Result.Success();
    }

    public Result ClearExtraGroups()
    {
        _extraGroups.Clear();
        return Result.Success();
    }
    

    public static Result<User> Create(UserId id, string name, string description, string group, bool isNormalUser, uint uid, Shell shell, string homePath)
    {
        return new User(id) {Name = name, Description = description, Group = group, IsNormalUser = isNormalUser, Shell = shell, HomePath = homePath, Uid = uid};
    }

    public Result<UserBuildResult> Build()
    {
        var shellPlaceholder = Guid.NewGuid().ToString();
        var extraGroups = $"\"{ExtraGroups.First()}\"";
        extraGroups = ExtraGroups.Aggregate(extraGroups, (current, extraGroup) => current + $" {extraGroup}");
        return new UserBuildResult($"{Name} = {{ description  = \"{Description}\"; group = \"{Group}\";" +
                                   $" home = \"{HomePath}\"; shell = \"{shellPlaceholder}\"; uid = {Uid};" +
                                   $" extraGroups = [{extraGroups}]; }};", Shell, shellPlaceholder);
    }
}