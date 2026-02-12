using FluentAssertions;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;
using Xunit.Abstractions;

namespace PhoeNix.Persistence.Tests;

public class ModuleRepositoryTests : PersistenceTestsBase
{
    public ModuleRepositoryTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task Should_Persist_Module_And_Retrieve_ByName()
    {
        // Arrange
        var moduleId = new ModuleId(Guid.NewGuid());
        var module = ModuleTemplate.Create(
            moduleId,
            "MyTestModule",
            true,
            ModuleType.Generic,
            [Architecture.Aarch64Linux]
        ).Value;

        var entry1 = TextValue.Create(new EntryValueId(Guid.NewGuid()), "entry1", "val1").Value;
        var entry2 = TextValue.Create(new EntryValueId(Guid.NewGuid()), "entry2", "val2").Value;

        var result = module.ChangeContent("entry1 entry2", [entry1, entry2]);
        result.IsSuccess.Should().BeTrue();

        ModuleRepository.Add(module);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        // Act
        var loaded = await ModuleRepository.GetByNameAsync("MyTestModule", CancellationToken.None);

        // Assert
        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(moduleId);
        loaded.Name.Should().Be("MyTestModule");
        loaded.Enabled.Should().BeTrue();
        loaded.SupportedArchitectures.Should().ContainSingle().Which.Should().Be(Architecture.Aarch64Linux);
        loaded.EditableValues.Should().HaveCount(2);
    }

    [Fact]
    public void Should_Fail_If_Entry_Missing_In_Content()
    {
        var module = ModuleTemplate.Create(
            new ModuleId(Guid.NewGuid()),
            "InvalidModule",
            true,
            ModuleType.Generic,
            [Architecture.Aarch64Linux]
        ).Value;

        var invalidEntry = TextValue.Create(new EntryValueId(Guid.NewGuid()), "entryNotInContent", "test").Value;

        var result = module.AddEntry(invalidEntry);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("not present");
    }
}