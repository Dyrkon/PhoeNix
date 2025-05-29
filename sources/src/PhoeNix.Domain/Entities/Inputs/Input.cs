using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Inputs;

public class Input : Entity<InputId>
{
    private readonly List<Input> _followers = new();

    private Input(InputId id) : base(id)
    {
    }

    public string Source { get; private set; }
    public string Name { get; private set; }

    public InputId? FollowsId { get; private set; }
    public Input? Follows { get; private set; }

    public IReadOnlyCollection<Input> Followers => _followers;

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

    public Result ChangeFollows(Input newInput)
    {
        if (newInput.Id == Follows?.Id)
            return Result.Failure(new Error("", $"This input already follows this input ({newInput.Id})"));

        if (newInput.Id == Id)
            return Result.Failure(new Error("", "Input can't follow itself"));

        Follows = newInput;
        FollowsId = newInput.Id;
        return Result.Success();
    }

    public static Result<Input> Create(InputId id, string source, string name, Input? follows = null)
    {
        return new Input(id)
        {
            Source = source,
            Name = name,
            Follows = follows,
            FollowsId = follows?.Id
        };
    }

    public Result<InputBuildResult> Build()
    {
        // TODO follows not implemented
        return new InputBuildResult($"{Name} = {{ {Source} }};");
    }
}