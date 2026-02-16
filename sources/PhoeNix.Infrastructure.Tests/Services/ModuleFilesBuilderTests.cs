using FluentAssertions;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Models.Files;
using PhoeNix.Domain.Shared;
using PhoeNix.Infrastructure.Services;

namespace PhoeNix.Infrastructure.Tests.Services;

public class ModuleFilesBuilderTests
{
    [Fact]
    public void BuildModule_Should_Create_Folder_With_Module_And_Inputs_Files_And_Replace_Placeholders()
    {
        // Arrange
        var builder = new ModuleFilesBuilder();

        var templateId = new ModuleTemplateId(Guid.NewGuid());
        var inputsLocationPlaceholder = Guid.NewGuid().ToString();
        var testedModulePathPlaceholder = Guid.NewGuid().ToString();

        var testId = new TestId(Guid.NewGuid());

        var moduleBuild = new ModuleBuildResult(
            templateId,
            "MyModule",
            $"{{ ... }}: let args = import {inputsLocationPlaceholder}/values.nix; in {{ foo = 1; }}",
            "{ x = 1; }",
            "values",
            inputsLocationPlaceholder,
            new List<ModuleTestBuildResult>
            {
                new(
                    testId,
                    $"{{ inputs, ... }}: let x = import {inputsLocationPlaceholder}/values.nix; in import {testedModulePathPlaceholder} {{ }}",
                    "test01",
                    testedModulePathPlaceholder,
                    inputsLocationPlaceholder
                )
            }
        );

        // Act
        var folder = builder.BuildModule(moduleBuild);

        // Assert
        folder.Name.Should().Be(templateId.ToStringWithPrefix());

        var files = folder.Files.ToList();

        var textFiles = files.OfType<TextFile>().ToList();

        textFiles.Should().ContainSingle(f => f.Name == $"{DefaultNames.ModuleName}.nix");
        textFiles.Should().ContainSingle(f => f.Name == "values.nix");
        textFiles.Should().ContainSingle(f => f.Name == $"{testId.ToStringWithPrefix()}.nix");

        var moduleFile = textFiles.Single(f => f.Name == $"{DefaultNames.ModuleName}.nix");
        moduleFile.Content.Should().NotContain(inputsLocationPlaceholder);
        moduleFile.Content.Should().Contain("import ./values.nix");

        var inputsFile = textFiles.Single(f => f.Name == "values.nix");
        inputsFile.Content.Should().Be("{ x = 1; }");

        var testFile = textFiles.Single(f => f.Name == $"{testId.ToStringWithPrefix()}.nix");
        testFile.Content.Should().NotContain(inputsLocationPlaceholder);
        testFile.Content.Should().NotContain(testedModulePathPlaceholder);
        testFile.Content.Should().Contain("import ./values.nix");
        testFile.Content.Should().Contain($"./{DefaultNames.ModuleName}.nix");
    }

    [Fact]
    public void BuildModule_Should_Not_Create_Test_Files_When_No_Tests()
    {
        // Arrange
        var builder = new ModuleFilesBuilder();

        var templateId = new ModuleTemplateId(Guid.NewGuid());
        var inputsLocationPlaceholder = Guid.NewGuid().ToString();

        var moduleBuild = new ModuleBuildResult(
            templateId,
            "MyModule",
            $"{{ ... }}: import {inputsLocationPlaceholder}/values.nix",
            "{ x = 1; }",
            "values",
            inputsLocationPlaceholder,
            null
        );

        // Act
        var folder = builder.BuildModule(moduleBuild);

        // Assert
        var textFiles = folder.Files.OfType<TextFile>().ToList();
        textFiles.Should().HaveCount(2);
        textFiles.Should().ContainSingle(f => f.Name == $"{DefaultNames.ModuleName}.nix");
        textFiles.Should().ContainSingle(f => f.Name == "values.nix");
    }

    [Fact]
    public void
        BuildSystemModule_Should_Create_System_Folder_With_Modules_Subfolder_And_Replace_ModulesListPlaceholder()
    {
        // Arrange
        var builder = new ModuleFilesBuilder();
        var layout = new ConfigurationLayout();

        var systemId = new SystemId(Guid.NewGuid());
        var systemArch = Domain.Enums.Architecture.X86Linux;

        var m1 = new ModuleBuildResult(
            new ModuleTemplateId(Guid.NewGuid()),
            "M1",
            "{ }",
            "{ }",
            "values",
            Guid.NewGuid().ToString());

        var m2 = new ModuleBuildResult(
            new ModuleTemplateId(Guid.NewGuid()),
            "M2",
            "{ }",
            "{ }",
            "values",
            Guid.NewGuid().ToString());

        var modulesListPlaceholder = Guid.NewGuid().ToString();

        var systemBuild = new SystemBuildResult(
            systemId,
            "Sys",
            systemArch,
            $"modules = [ {modulesListPlaceholder} ];",
            new[] { m1, m2 },
            modulesListPlaceholder);

        // Act
        var folder = builder.BuildSystemModule(systemBuild);

        // Assert
        folder.Name.Should().Be(systemId.ToStringWithPrefix());

        var files = folder.Files.ToList();

        var folders = files.OfType<Folder>().ToList();
        folders.Should().ContainSingle(f => f.Name == "Modules");

        var textFiles = files.OfType<TextFile>().ToList();
        textFiles.Should().ContainSingle();

        var modulesFolder = folders.Single(f => f.Name == "Modules");
        var moduleFolders = modulesFolder.Files.OfType<Folder>().ToList();
        moduleFolders.Should().HaveCount(2);

        var systemFile = textFiles.Single();
        systemFile.Name.Should().Be(layout.SystemName(systemId, systemArch));
        systemFile.Content.Should().NotContain(modulesListPlaceholder);

        systemFile.Content.Should().Contain(m1.Id.ToStringWithPrefix());
        systemFile.Content.Should().Contain(m2.Id.ToStringWithPrefix());
    }
}