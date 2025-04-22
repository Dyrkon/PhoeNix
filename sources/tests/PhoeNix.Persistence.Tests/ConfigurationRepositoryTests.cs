using FluentAssertions;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Homes;
using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using Xunit.Abstractions;

namespace PhoeNix.Persistence.Tests;

public class ConfigurationRepositoryTests : PersistenceTestsBase
{
    public ConfigurationRepositoryTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnConfigurationWithAllRelations()
    {
        // Arrange
        var configurationId = new ConfigurationId(Guid.NewGuid());
        var configuration = Configuration.Create(configurationId, "Title", "Description").Value;

        var moduleId = new ModuleId(Guid.NewGuid());
        var systemId = new SystemId(Guid.NewGuid());
        var homeId = new HomeId(Guid.NewGuid());
        var inputId = new InputId(Guid.NewGuid());

        var module = Module.Create(moduleId, "Foo", true, ModuleType.Generic, [Architecture.X86Linux]).Value;
        var system = Domain.Entities.Systems.System.Create(systemId, Architecture.X86Linux, "Test System").Value;
        var home = Home.Create(homeId, "Test Home").Value;
        var input = Input.Create(inputId, "Input1", "source").Value;

        await PhoeNixDbContextSUT.Modules.AddAsync(module);
        await PhoeNixDbContextSUT.Systems.AddAsync(system);
        await PhoeNixDbContextSUT.Homes.AddAsync(home);
        await PhoeNixDbContextSUT.Inputs.AddAsync(input);

        configuration.AddModule(moduleId);
        configuration.AddSystem(systemId);
        configuration.AddHome(homeId);
        configuration.AddInput(inputId);

        await PhoeNixDbContextSUT.Configurations.AddAsync(configuration);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        // Act
        var result = await ConfigurationRepository.GetByIdAsync(configurationId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Modules.Should().ContainSingle(m => m.ModuleId == moduleId);
        result.Homes.Should().ContainSingle(h => h.HomeId == homeId);
        result.Inputs.Should().ContainSingle(i => i.InputId == inputId);
        result.Systems.Should().ContainSingle(s => s.SystemId == systemId);
    }
    
    [Fact]
    public async Task GetByDescriptionAsync_ShouldReturnMatchingConfigurationWithRelations()
    {
        // Arrange
        var configId = new ConfigurationId(Guid.NewGuid());
        var configuration = Configuration.Create(configId, "Search Title", "Unique Description 123").Value;

        var moduleId = new ModuleId(Guid.NewGuid());
        var module = Module.Create(moduleId, "Foo", true, ModuleType.Generic, [Architecture.X86Linux]).Value;

        await PhoeNixDbContextSUT.Modules.AddAsync(module);
        configuration.AddModule(moduleId);

        await PhoeNixDbContextSUT.Configurations.AddAsync(configuration);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        // Act
        var result = await ConfigurationRepository.GetByDescriptionAsync("Unique Description", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(configId);
        result.Modules.Should().ContainSingle(m => m.ModuleId == moduleId);
    }

    [Fact]
    public async Task GetByTitleAsync_ShouldReturnMatchingConfigurationWithRelations()
    {
        // Arrange
        var configId = new ConfigurationId(Guid.NewGuid());
        var configuration = Configuration.Create(configId, "Config Title ABC", "Irrelevant").Value;

        var inputId = new InputId(Guid.NewGuid());
        var input = Input.Create(inputId, "TestInput", "some-source").Value;

        await PhoeNixDbContextSUT.Inputs.AddAsync(input);
        configuration.AddInput(inputId);

        await PhoeNixDbContextSUT.Configurations.AddAsync(configuration);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        // Act
        var result = await ConfigurationRepository.GetByTitleAsync("Title ABC", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(configId);
        result.Inputs.Should().ContainSingle(i => i.InputId == inputId);
    }

}
