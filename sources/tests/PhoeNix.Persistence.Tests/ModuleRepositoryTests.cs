using FluentAssertions;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;
using Xunit.Abstractions;

namespace PhoeNix.Persistence.Tests;

public class ModuleRepositoryTests : PersistenceTestsBase
{
    public ModuleRepositoryTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task Add_ShouldPersistModuleWithEntriesAndArchitectures()
    {
        // Arrange
        var id = new ModuleId(Guid.NewGuid());
        var entryId = new ModuleEntryId(Guid.NewGuid());

        var entry = ModuleEntry.Create(entryId).Value!;
        var module = Module.Create(
            id,
            "Main",
            enabled: true,
            ModuleType.Generic,
            [Architecture.Aarch64Linux]
        ).Value!;

        module.AddEntry(entry);

        // Act
        ModuleRepository.Add(module);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        // Assert
        var persisted = await ModuleRepository.GetByIdAsync(id, CancellationToken.None);
        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be("Main");
        persisted.Entries.Should().ContainSingle(e => e.Id == entryId);
        persisted.SupportedArchitectures.Should().ContainSingle(a => a == Architecture.Aarch64Linux);
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnCorrectModule()
    {
        // Arrange
        var module = Module.Create(
            new ModuleId(Guid.NewGuid()),
            "Analytics",
            enabled: true,
            ModuleType.Generic,
            [Architecture.Aarch64Linux]
        ).Value!;

        ModuleRepository.Add(module);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        // Act
        var result = await ModuleRepository.GetByNameAsync("Analytics", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Analytics");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldIncludeEntries()
    {
        // Arrange
        var moduleId = new ModuleId(Guid.NewGuid());
        var entryId = new ModuleEntryId(Guid.NewGuid());
        var entry = ModuleEntry.Create(entryId).Value!;

        var module = Module.Create(
            moduleId,
            "InitModule",
            enabled: false,
            ModuleType.Generic,
            [Architecture.Aarch64Linux]
        ).Value!;

        module.AddEntry(entry);

        ModuleRepository.Add(module);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        // Act
        var result = await ModuleRepository.GetByIdAsync(moduleId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Entries.Should().ContainSingle(e => e.Id == entryId);
    }
}