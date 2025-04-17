using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Inputs;

public class Input : Entity<InputId>
{
    private Input(InputId id) : base(id)
    {
    }

    public string Source { get; private set; }
    public string Name { get; private set; }
    public InputId? Follows { get; private set; }

    public Result ChangeSource(string newSource)
    {
        if (newSource == string.Empty)
            return Result.Failure(new Error("", "Source can't be empty"));

        // TODO add regex
        Source = newSource;
        return Result.Success();
    }

    public Result ChangeName(string newName)
    {
        if (newName == string.Empty)
            return Result.Failure(new Error("", "Name can't be empty"));

        Name = newName;
        return Result.Success();
    }

    public Result ChangeFollows(InputId newInputId)
    {
        if (newInputId == Follows)
            return Result.Failure(new Error("", $"This input already follows this input ({newInputId})"));

        if (newInputId == Id)
            return Result.Failure(new Error("", "Input can't follow itself"));

        Follows = newInputId;
        return Result.Success();
    }

    public static Result<Input> Create(InputId id, string source, string name, InputId? follows = null)
    {
        return new Input(id)
        {
            Source = source,
            Name = name,
            Follows = follows
        };
    }
}