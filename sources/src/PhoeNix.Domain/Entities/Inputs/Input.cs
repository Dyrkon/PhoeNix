using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Inputs;

public sealed class Input : Entity<InputId>
{
    private readonly List<FollowInput> _followers = [];

    private Input(InputId id) : base(id)
    {
    }

    public ConfigurationId ConfigurationId { get; private set; } = default!;

    public string Source { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public IReadOnlyCollection<FollowInput> Followers => _followers;

    public Result ChangeSource(string newSource)
    {
        if (string.IsNullOrWhiteSpace(newSource))
            return Result.Failure(new Error("Inputs.SourceEmpty", "Source can't be empty."));

        Source = newSource.Trim();
        return Result.Success();
    }

    public Result ChangeName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return Result.Failure(new Error("Inputs.NameEmpty", "Name can't be empty."));

        Name = newName.Trim();
        return Result.Success();
    }

    public Result AddFollow(string followName, string followValue)
    {
        if (_followers.Any(f => f.FollowName == followName))
            return Result.Failure(
                new Error("Inputs.FollowAlreadyExists", $"Input '{Name}' already follows '{followName}'."));

        if (followName == Name)
            return Result.Failure(
                new Error("Inputs.CannotFollowItself", "Input can't follow itself."));

        _followers.Add(new FollowInput(Guid.NewGuid(), Id, followName, followValue));
        return Result.Success();
    }

    public Result RemoveFollow(Guid followId)
    {
        var removedFollowers = _followers.RemoveAll(f => f.Id == followId);

        return removedFollowers == 0
            ? Result.Failure(
                new Error("Inputs.FollowNotFound", $"There is no follow with id '{followId}' in input '{Name}'."))
            : Result.Success();
    }

    public Result ReplaceFollows(IReadOnlyCollection<InputFollowDraft> follows)
    {
        if (follows.GroupBy(x => x.FollowName, StringComparer.Ordinal).Any(g => g.Count() > 1))
            return Result.Failure(
                new Error("Inputs.DuplicateFollowName", "Follow names must be unique within an input."));

        _followers.Clear();

        foreach (var follow in follows)
        {
            var addResult = AddFollow(follow.FollowName, follow.FollowValue);
            if (addResult.IsFailure)
                return addResult;
        }

        return Result.Success();
    }

    public static Result<Input> Create(
        InputId id,
        ConfigurationId configurationId,
        string source,
        string name,
        Input? follows = null)
    {
        if (string.IsNullOrWhiteSpace(source))
            return Result.Failure<Input>(new Error("Inputs.SourceEmpty", "Source can't be empty."));

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Input>(new Error("Inputs.NameEmpty", "Name can't be empty."));

        Result<Input> newInput = new Input(id)
        {
            ConfigurationId = configurationId,
            Source = source.Trim(),
            Name = name.Trim()
        };

        return follows is not null
            ? newInput.Tap(i => i.AddFollow(follows.Name, follows.Name))
            : newInput;
    }
}

public sealed record FollowInput(Guid Id, InputId InputId, string FollowName, string FollowValue);