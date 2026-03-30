using FluentAssertions;
using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Application.Models.Files;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;
using PhoeNix.Infrastructure.Services;
using PhoeNix.Infrastructure.Services.ConfigurationManagement;
using Xunit;
using Xunit.Abstractions;

namespace PhoeNix.Infrastructure.Tests.Services;

public class ConfigurationFilesBuilderTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ConfigurationFilesBuilderTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void BuildConfigurationFiles_Should_Create_Flake_And_Folders_And_Replace_All_Placeholders()
    {
        // Arrange
        var layout = new ConfigurationLayout();

        var configId = new ConfigurationId(Guid.NewGuid());
        var systemsPlaceholder = Guid.NewGuid().ToString();
        var sharedModulesPlaceholder = Guid.NewGuid().ToString();
        var checksPlaceholder = Guid.NewGuid().ToString();

        var sharedModuleTemplateId = new ModuleTemplateId(Guid.NewGuid());
        var sharedTestId = new TestId(Guid.NewGuid());

        var systemId = new SystemId(Guid.NewGuid());
        var systemArch = Architecture.X86Linux;

        var systemModuleTemplateId = new ModuleTemplateId(Guid.NewGuid());
        var systemTestId = new TestId(Guid.NewGuid());

        var commonModules = new[]
        {
            new ModuleBuildResult(
                sharedModuleTemplateId,
                "SharedMod",
                "{ }",
                "{ }",
                "values",
                Guid.NewGuid().ToString(),
                new List<ModuleTestBuildResult>
                {
                    new(
                        sharedTestId,
                        "test content",
                        "sharedTest",
                        Guid.NewGuid().ToString(),
                        Guid.NewGuid().ToString()
                    )
                })
        };

        var systemModules = new[]
        {
            new ModuleBuildResult(
                systemModuleTemplateId,
                "SysMod",
                "{ }",
                "{ }",
                "values",
                Guid.NewGuid().ToString(),
                new List<ModuleTestBuildResult>
                {
                    new(
                        systemTestId,
                        "test content",
                        "systemTest",
                        Guid.NewGuid().ToString(),
                        Guid.NewGuid().ToString()
                    )
                })
        };

        var systems = new[]
        {
            new SystemBuildResult(
                systemId,
                "Sys",
                systemArch,
                "system content",
                systemModules,
                Guid.NewGuid().ToString())
        };

        var content =
            $"SYS={systemsPlaceholder}\n" +
            $"SHARED={sharedModulesPlaceholder}\n" +
            $"CHECKS={checksPlaceholder}\n";

        var build = new ConfigurationBuildResult(
            configId,
            "T",
            content,
            sharedModulesPlaceholder,
            systemsPlaceholder,
            checksPlaceholder,
            new[] { systemArch },
            commonModules,
            systems);

        var fakeModuleBuilder = new FakeModuleFilesBuilder();
        var sut = new ConfigurationFilesBuilder(fakeModuleBuilder);

        // Act
        var result = sut.BuildConfigurationFiles(build);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var folder = result.Value;
        folder.Name.Should().Be(configId);

        var flake = FindText(folder, "flake.nix");
        flake.Should().NotBeNull();
        flake!.Content.Should().NotContain(systemsPlaceholder);
        flake.Content.Should().NotContain(sharedModulesPlaceholder);
        flake.Content.Should().NotContain(checksPlaceholder);

        flake.Content.Should().Contain(systemId.ToStringWithPrefix());
        flake.Content.Should().Contain(layout.SystemPath(systemId, systemArch));

        flake.Content.Should().Contain(layout.SharedModulePath(sharedModuleTemplateId));

        flake.Content.Should().Contain(sharedTestId.ToStringWithPrefix());
        flake.Content.Should().Contain(layout.SharedModuleTestPath(sharedModuleTemplateId, sharedTestId));

        flake.Content.Should().Contain(systemTestId.ToStringWithPrefix());
        flake.Content.Should().Contain(layout.SystemModuleTestPath(systemId, systemModuleTemplateId, systemTestId));


        FindFolder(folder, layout.SharedModulesPath).Should().NotBeNull();
        FindFolder(folder, layout.SystemsPath).Should().NotBeNull();

        fakeModuleBuilder.BuildModuleCalls.Should().Be(commonModules.Length);
        fakeModuleBuilder.BuildSystemModuleCalls.Should().Be(systems.Length);
    }

    [Fact]
    public void BuildConfigurationFiles_Should_Produce_Empty_Checks_When_No_Tests()
    {
        // Arrange
        var configId = new ConfigurationId(Guid.NewGuid());
        var systemsPlaceholder = Guid.NewGuid().ToString();
        var sharedModulesPlaceholder = Guid.NewGuid().ToString();
        var checksPlaceholder = Guid.NewGuid().ToString();

        var build = new ConfigurationBuildResult(
            configId,
            "T",
            $"SYS={systemsPlaceholder}\nSHARED={sharedModulesPlaceholder}\nCHECKS={checksPlaceholder}\n",
            sharedModulesPlaceholder,
            systemsPlaceholder,
            checksPlaceholder,
            new[] { Architecture.X86Linux },
            new[]
            {
                new ModuleBuildResult(
                    new ModuleTemplateId(Guid.NewGuid()),
                    "M",
                    "{ }",
                    "{ }",
                    "values",
                    Guid.NewGuid().ToString(),
                    null)
            },
            Array.Empty<SystemBuildResult>());

        var sut = new ConfigurationFilesBuilder(new FakeModuleFilesBuilder());

        // Act
        var result = sut.BuildConfigurationFiles(build);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var flake = FindText(result.Value, "flake.nix")!;
        flake.Content.Should().NotContain(checksPlaceholder);
        flake.Content.Should().NotContain("test-");
    }

    private static TextFile? FindText(Folder root, string name)
    {
        return Enumerate(root).OfType<TextFile>().SingleOrDefault(f => f.Name == name);
    }

    private static Folder? FindFolder(Folder root, string name)
    {
        return Enumerate(root).OfType<Folder>().SingleOrDefault(f => f.Name == name);
    }

    private static IEnumerable<FileBase> Enumerate(Folder root)
    {
        foreach (var f in root.Files)
        {
            yield return f;
            if (f is Folder folder)
                foreach (var nested in Enumerate(folder))
                    yield return nested;
        }
    }

    private sealed class FakeModuleFilesBuilder : IModuleFilesBuilder
    {
        public int BuildModuleCalls { get; private set; }
        public int BuildSystemModuleCalls { get; private set; }

        public Folder BuildModule(ModuleBuildResult moduleBuild)
        {
            BuildModuleCalls++;
            return new Folder($"{moduleBuild.Id.ToStringWithPrefix()}", Array.Empty<FileBase>());
        }

        public Folder BuildSystemModule(SystemBuildResult systemBuild)
        {
            BuildSystemModuleCalls++;
            return new Folder($"{systemBuild.Id.ToStringWithPrefix()}", Array.Empty<FileBase>());
        }
    }
}