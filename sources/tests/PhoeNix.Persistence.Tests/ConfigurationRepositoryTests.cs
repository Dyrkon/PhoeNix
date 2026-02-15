using System.Reflection;
using FluentAssertions;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using Xunit.Abstractions;

namespace PhoeNix.Persistence.Tests;

public class ConfigurationRepositoryTests : PersistenceTestsBase
{
    public ConfigurationRepositoryTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnConfigurationWithAllIncludedRelations()
    {
        // Arrange
        var configurationId = new ConfigurationId(Guid.NewGuid());
        var config = Configuration.Create(configurationId, "Title", "Unique Description 123").Value;

        var moduleTemplateId = new ModuleTemplateId(Guid.NewGuid());
        var moduleTemplate = ModuleTemplate.Create(
            moduleTemplateId, "Foo", true, ModuleType.Generic, new List<Architecture> { Architecture.X86Linux }
        ).Value;
        await PhoeNixDbContextSUT.ModuleTemplates.AddAsync(moduleTemplate);

        config.AddModule(moduleTemplateId, true);
        var configModule = config.Modules.Single(m => m.ModuleTemplateId == moduleTemplateId);
        InjectEditableValues(configModule, new List<EntryValue>
        {
            TextValue.Create(new EntryValueId(Guid.NewGuid()), "abc", "CONF_TXT", "{CONF_TXT}").Value
        });

        var input = config.AddInput("github:nixos", "nixpkgs").Value;
        config.AddInputFollow(input.Id, "flake-utils", "github:numtide/flake-utils");

        var systemId = new SystemId(Guid.NewGuid());
        config.AddSystem(systemId, Architecture.X86Linux, "Test System");
        config.AddSystemModule(systemId, moduleTemplateId, true);

        var system = config.SystemSpecifications.Single(s => s.Id == systemId);
        system.Modules.Should().ContainSingle(m => m.ModuleTemplateId == moduleTemplateId);

        var systemModule = system.Modules.Single(m => m.ModuleTemplateId == moduleTemplateId);
        InjectEditableValues(systemModule, new List<EntryValue>
        {
            TextValue.Create(new EntryValueId(Guid.NewGuid()), "xyz", "SYS_TXT", "{SYS_TXT}").Value
        });

        await PhoeNixDbContextSUT.Configurations.AddAsync(config);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        // Act
        var loaded = await ConfigurationRepository.GetByIdAsync(configurationId, CancellationToken.None);

        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(configurationId);

        loaded.Modules.Should().ContainSingle(m => m.ModuleTemplateId == moduleTemplateId);
        loaded.Modules.Single().EditableValues.Should().ContainSingle(e =>
            e.Name == "CONF_TXT" && e.Placeholder == "{CONF_TXT}" && e.Value == "abc");

        loaded.Inputs.Should().ContainSingle(i => i.Name == "nixpkgs" && i.Source == "github:nixos");
        loaded.Inputs.Single().Followers.Should().ContainSingle(f =>
            f.FollowName == "flake-utils" && f.FollowValue == "github:numtide/flake-utils");

        loaded.SystemSpecifications.Should().ContainSingle(s =>
            s.Id == systemId && s.Architecture == Architecture.X86Linux && s.Name == "Test System");

        var loadedSystem = loaded.SystemSpecifications.Single(s => s.Id == systemId);
        loadedSystem.Modules.Should().ContainSingle(m => m.ModuleTemplateId == moduleTemplateId);

        loadedSystem.Modules.Single().EditableValues.Should().ContainSingle(e =>
            e.Name == "SYS_TXT" && e.Placeholder == "{SYS_TXT}" && e.Value == "xyz");
    }

    [Fact]
    public async Task GetByDescriptionAsync_ShouldUse_Contains_AndReturnMatchingConfiguration()
    {
        // Arrange
        var configId = new ConfigurationId(Guid.NewGuid());
        var config = Configuration.Create(configId, "Search Title", "Unique Description 123").Value;

        await PhoeNixDbContextSUT.Configurations.AddAsync(config);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        // Act (substring)
        var loaded = await ConfigurationRepository.GetByDescriptionAsync("Unique Description", CancellationToken.None);

        // Assert
        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(configId);
        loaded.Description.Should().Contain("Unique Description 123");
    }

    [Fact]
    public async Task GetByTitleAsync_ShouldUse_Contains_AndReturnMatchingConfiguration()
    {
        // Arrange
        var configId = new ConfigurationId(Guid.NewGuid());
        var config = Configuration.Create(configId, "Config Title ABC", "Irrelevant").Value;

        await PhoeNixDbContextSUT.Configurations.AddAsync(config);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        // Act (substring)
        var loaded = await ConfigurationRepository.GetByTitleAsync("Title ABC", CancellationToken.None);

        // Assert
        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(configId);
        loaded.Title.Should().Contain("Config Title ABC");
    }

    [Fact]
    public async Task RemoveByIdAsync_ShouldRemove_When_Exists()
    {
        // Arrange
        var configId = new ConfigurationId(Guid.NewGuid());
        var config = Configuration.Create(configId, "To Remove", "Desc").Value;

        await PhoeNixDbContextSUT.Configurations.AddAsync(config);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        // Act
        var result = await ConfigurationRepository.RemoveByIdAsync(configId, CancellationToken.None);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        (await ConfigurationRepository.GetByIdAsync(configId, CancellationToken.None)).Should().BeNull();
    }

    [Fact]
    public async Task RemoveByIdAsync_ShouldFail_When_NotFound()
    {
        // Act
        var missingId = new ConfigurationId(Guid.NewGuid());
        var result = await ConfigurationRepository.RemoveByIdAsync(missingId, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be($"Configuration with id {missingId.Value} was not found");
    }

    private static void InjectEditableValues(ModuleValue moduleValue, List<EntryValue> values)
    {
        var prop = typeof(ModuleValue).GetProperty("EditableValues",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (prop is not null && prop.CanRead)
        {
            var current = prop.GetValue(moduleValue);
            if (current is ICollection<EntryValue> coll)
            {
                foreach (var v in values) coll.Add(v);
                return;
            }
        }

        var field = typeof(ModuleValue).GetField("_editableValues",
            BindingFlags.NonPublic | BindingFlags.Instance);

        field.Should()
            .NotBeNull("ModuleValue should have an EditableValues collection or a backing field named _editableValues");
        field!.SetValue(moduleValue, values);
    }
}