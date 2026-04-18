using FluentAssertions;
using NSubstitute;
using PhoeNix.Application.Models.Modules;
using PhoeNix.Application.Modules.Commands;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.UnitTests.Handlers;

public class CreateModuleTemplateHandlerTests
{
    private readonly IModuleTemplateRepository _moduleTemplateRepository =
        Substitute.For<IModuleTemplateRepository>();

    private static CreateModuleTemplateCommand BuildCommand(string name = "TestModule") =>
        new(
            name,
            true,
            ModuleType.Generic,
            "some nix content",
            new List<Architecture> { Architecture.X86Linux },
            new List<ModuleTemplateEntryValueDefinitionModel>(),
            new List<ModuleTemplateTestUpsertModel>(),
            new List<RequiredInputDefinitionModel>());

    [Fact]
    public async Task Handle_Should_Create_ModuleTemplate_Successfully()
    {
        _moduleTemplateRepository.GetByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ModuleTemplate?)null);

        var handler = new CreateModuleTemplateHandler(_moduleTemplateRepository);
        var result = await handler.Handle(BuildCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("TestModule");
        _moduleTemplateRepository.Received(1).Add(Arg.Any<ModuleTemplate>());
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Name_Already_Exists()
    {
        var existing = ModuleTemplate.Create(
            new ModuleTemplateId(Guid.NewGuid()),
            "TestModule",
            true,
            ModuleType.Generic,
            new List<Architecture> { Architecture.X86Linux }).Value;
        _moduleTemplateRepository.GetByNameAsync("TestModule", Arg.Any<CancellationToken>())
            .Returns(existing);

        var handler = new CreateModuleTemplateHandler(_moduleTemplateRepository);
        var result = await handler.Handle(BuildCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Modules");
        _moduleTemplateRepository.DidNotReceive().Add(Arg.Any<ModuleTemplate>());
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Module_Name_Empty()
    {
        _moduleTemplateRepository.GetByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ModuleTemplate?)null);

        var handler = new CreateModuleTemplateHandler(_moduleTemplateRepository);
        var result = await handler.Handle(BuildCommand(""), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}
