using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Inputs;

public class Input : Entity<InputId>
{
    public ConfigurationId ConfigurationId { get; private set; }

    private readonly List<FollowInput> _followers = new();

    private Input(InputId id) : base(id)
    {
    }

    public string Source { get; private set; }
    public string Name { get; private set; }

    public IReadOnlyCollection<FollowInput> Followers => _followers;

    public Result ChangeSource(string newSource)
    {
        if (string.IsNullOrEmpty(newSource))
            return Result.Failure(new Error("", "Source can't be empty"));

        Source = newSource;
        return Result.Success();
    }

    public Result ChangeName(string newName)
    {
        if (string.IsNullOrEmpty(newName))
            return Result.Failure(new Error("", "Name can't be empty"));

        Name = newName;
        return Result.Success();
    }

    public Result AddFollow(string followName, string followValue)
    {
        if (Followers.Any(f => f.FollowName == followName))
            return Result.Failure(new Error("FlakeInputAlreadyFollows",
                $"This input already follows this input ({Name})"));

        if (followName == Name)
            return Result.Failure(new Error("FlakeInputCannotFollowItself", "Input can't follow itself"));

        _followers.Add(new FollowInput(Guid.NewGuid(), Id, followName, followValue));
        return Result.Success();
    }

    public Result RemoveFollow(Guid followId)
    {
        var removedModules = _followers.RemoveAll(f => f.Id == followId);
        if (removedModules == 0)
            return Result.Failure(new Error("FlakeInputUnableToRemoveFollow",
                $"There is no follower with id {followId} in this ({Name}) input"));

        return Result.Success();
    }

    public static Result<Input> Create(InputId id, ConfigurationId configurationId, string source, string name,
        Input? follows = null)
    {
        Result<Input> newInput = new Input(id)
        {
            ConfigurationId = configurationId,
            Source = source,
            Name = name
        };
        return follows is not null
            ? newInput
                .Tap(i => i.AddFollow(follows.Name, follows.Name))
            : newInput;
    }

    private string BuildFollow(FollowInput followInput)
    {
        return $"{followInput.FollowName}.follows = \"{followInput.FollowValue}\";";
    }

    public Result<InputBuildResult> Build()
    {
        var follows = Followers.Aggregate("", (current, result) => current + $"{BuildFollow(result)}\n");
        return new InputBuildResult(
            $"{Name} = {{ url = \"{Source}\";\n " +
            $"inputs = {{ {follows} }};" +
            $"}};");
    }
}

public record FollowInput(Guid Id, InputId InputId, string FollowName, string FollowValue);