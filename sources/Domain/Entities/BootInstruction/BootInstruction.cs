using Domain.Primitives;
using Domain.ValueObjects;

namespace Domain.Entities.BootInstruction;

public class BootInstruction : AggregateRoot<BootInstructionId>
{
    private BootInstruction(
        BootInstructionId id,
        KernelLocation kernelLocation,
        List<InitrdLocation> initrdLocations,
        CommandLineInstructions? cmdLine = null) : base(id)
    {
        KernelLocation = kernelLocation;
        InitrdLocations = initrdLocations;
        CommandLineInstructions = cmdLine ?? CommandLineInstructions.CreateEmpty();
    }

    public KernelLocation KernelLocation { get; private set; }
    public List<InitrdLocation> InitrdLocations { get; private set; }
    public CommandLineInstructions CommandLineInstructions { get; private set; }
}