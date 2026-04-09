using System.Reflection;
using FluentAssertions;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;
using Xunit.Abstractions;

namespace PhoeNix.Persistence.Tests;

public class ModuleTemplateRepositoryTests : PersistenceTestsBase
{
    public ModuleTemplateRepositoryTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task Should_Persist_Module_And_Retrieve_ByName_Including_EditableValueTypes()
    {
        // Arrange
        var moduleId = new ModuleTemplateId(Guid.NewGuid());
        var module = ModuleTemplate.Create(
            moduleId,
            "MyTestModule",
            true,
            ModuleType.Generic,
            new List<Architecture> { Architecture.Aarch64Linux }
        ).Value;

        var def1 = new EntryValueDefinition(
            moduleId,
            "ENTRY_ONE",
            "ENTRY_ONE",
            EntryBindingKind.UserProvided,
            EntryValueKind.Text);

        var def2 = new EntryValueDefinition(
            moduleId,
            "ENTRY_TWO",
            "ENTRY_TWO",
            EntryBindingKind.UserProvided,
            EntryValueKind.Text);

        var change = module.ChangeContent("ENTRY_ONE ENTRY_TWO", new List<EntryValueDefinition> { def1, def2 });
        change.IsSuccess.Should().BeTrue();

        ModuleTemplateRepository.Add(module);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        // Act (Contains)
        var loaded = await ModuleTemplateRepository.GetByNameAsync("MyTest", CancellationToken.None);

        // Assert
        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(moduleId);
        loaded.Name.Should().Be("MyTestModule");
        loaded.Type.Should().Be(ModuleType.Generic);
        loaded.Content.Should().Be("ENTRY_ONE ENTRY_TWO");
        loaded.SupportedArchitectures.Should().ContainSingle().Which.Should().Be(Architecture.Aarch64Linux);

        // Included collection
        loaded.EditableValueTypes.Should().HaveCount(2);
        loaded.EditableValueTypes.Should().Contain(e =>
            e.Name == "ENTRY_ONE" && e.Placeholder == "ENTRY_ONE");
        loaded.EditableValueTypes.Should().Contain(e =>
            e.Name == "ENTRY_TWO" && e.Placeholder == "ENTRY_TWO");
    }

    [Fact]
    public void ChangeContent_Should_Fail_If_EntryDefinition_Name_Missing_In_Content()
    {
        var module = ModuleTemplate.Create(
            new ModuleTemplateId(Guid.NewGuid()),
            "InvalidModule",
            true,
            ModuleType.Generic,
            new List<Architecture> { Architecture.Aarch64Linux }
        ).Value;

        var missing = new EntryValueDefinition(
            module.Id,
            "NOT_PRESENT",
            "NOT_PRESENT",
            EntryBindingKind.UserProvided,
            EntryValueKind.Text);

        var result = module.ChangeContent("some content without token", new List<EntryValueDefinition> { missing });

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_Should_Include_EditableValueTypes_And_Tests()
    {
        // Arrange
        var moduleId = new ModuleTemplateId(Guid.NewGuid());
        var module = ModuleTemplate.Create(
            moduleId,
            "WithIncludes",
            true,
            ModuleType.Generic,
            new List<Architecture> { Architecture.X86Linux }
        ).Value;

        var def = new EntryValueDefinition(
            moduleId,
            "A",
            "A",
            EntryBindingKind.UserProvided,
            EntryValueKind.Text);

        module.ChangeContent("A", new List<EntryValueDefinition> { def });

        // Add tests via reflection (AddModuleTest doesn't add into _tests in current entity)
        InjectTests(module, new List<Test>
        {
            Test.Create(new TestId(Guid.NewGuid()), "test01").Value
        });

        ModuleTemplateRepository.Add(module);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        // Act
        var loaded = await ModuleTemplateRepository.GetByIdAsync(moduleId, CancellationToken.None);

        // Assert
        loaded.Should().NotBeNull();
        loaded!.EditableValueTypes.Should().ContainSingle(e => e.Name == "A");
        loaded.Tests.Should().ContainSingle(t => t.Name == "test01");
    }

    [Fact]
    public async Task GetAllAsync_Should_Include_EditableValueTypes_And_Tests()
    {
        // Arrange
        var module1 = ModuleTemplate.Create(
            new ModuleTemplateId(Guid.NewGuid()),
            "M1",
            true,
            ModuleType.Generic,
            new List<Architecture> { Architecture.X86Linux }
        ).Value;

        module1.ChangeContent("X", new List<EntryValueDefinition>
        {
            new(module1.Id, "X", "X", EntryBindingKind.UserProvided, EntryValueKind.Text)
        });

        InjectTests(module1, new List<Test> { Test.Create(new TestId(Guid.NewGuid()), "t1").Value });

        var module2 = ModuleTemplate.Create(
            new ModuleTemplateId(Guid.NewGuid()),
            "M2",
            true,
            ModuleType.Generic,
            new List<Architecture> { Architecture.Aarch64Linux }
        ).Value;

        module2.ChangeContent("Y", new List<EntryValueDefinition>
        {
            new(module2.Id, "Y", "Y", EntryBindingKind.UserProvided, EntryValueKind.Text)
        });

        InjectTests(module2, new List<Test> { Test.Create(new TestId(Guid.NewGuid()), "t2").Value });

        ModuleTemplateRepository.Add(module1);
        ModuleTemplateRepository.Add(module2);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        // Act
        var all = (await ModuleTemplateRepository.GetAllAsync(CancellationToken.None)).ToList();

        // Assert
        all.Should().HaveCountGreaterThanOrEqualTo(2);

        all.Should().Contain(m => m.Name == "M1" && m.EditableValueTypes.Any() && m.Tests.Any());
        all.Should().Contain(m => m.Name == "M2" && m.EditableValueTypes.Any() && m.Tests.Any());
    }

    private static void InjectTests(ModuleTemplate module, List<Test> tests)
    {
        var field = typeof(ModuleTemplate).GetField("_tests",
            BindingFlags.NonPublic | BindingFlags.Instance);

        field.Should().NotBeNull("ModuleTemplate should have a private backing field named _tests");
        field!.SetValue(module, tests);
    }
}