using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Modules;

public class ModuleTest : Entity<ModuleTestId>
{
    private ModuleTest(ModuleTestId id) : base(id)
    {
    }


    public ModuleId ModuleId { get; private set; }

    public TestId TestId { get; private set; }

    public Module Module { get; private set; }

    public Test Test { get; private set; }

    public static Result<ModuleTest> Create(ModuleTestId id, ModuleId moduleId, TestId testId)
    {
        return new ModuleTest(id)
        {
            ModuleId = moduleId,
            TestId = testId
        };
    }
}