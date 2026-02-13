using FluentAssertions;
using PhoeNix.Domain.Entities.Inputs;
using Xunit;
using Xunit.Abstractions;

namespace PhoeNix.Persistence.Tests;

public class InputRepositoryTests(ITestOutputHelper output) : PersistenceTestsBase(output)
{
    [Fact]
    public async Task Input_FollowChain_PersistedCorrectly()
    {
        // Arrange
        var root = Input.Create(new InputId(Guid.NewGuid()), "root-src", "Root").Value;
        var mid = Input.Create(new InputId(Guid.NewGuid()), "mid-src", "Mid").Value;
        var leaf = Input.Create(new InputId(Guid.NewGuid()), "leaf-src", "Leaf").Value;

        mid.ChangeFollows(root);
        leaf.ChangeFollows(mid);

        InputRepository.Add(root);
        InputRepository.Add(mid);
        InputRepository.Add(leaf);

        await PhoeNixDbContextSUT.SaveChangesAsync();

        // Act
        var loadedLeaf = await InputRepository.GetByIdAsync(leaf.TemplateId, CancellationToken.None);
        var loadedMid = await InputRepository.GetByIdAsync(mid.TemplateId, CancellationToken.None);
        var loadedRoot = await InputRepository.GetByIdAsync(root.TemplateId, CancellationToken.None);

        // Assert
        loadedLeaf!.Follows!.Id.Should().Be(mid.TemplateId);
        loadedMid!.Follows!.Id.Should().Be(root.TemplateId);

        loadedRoot!.Followers.Should().ContainSingle(f => f.Id == mid.TemplateId);
        loadedMid.Followers.Should().ContainSingle(f => f.Id == leaf.TemplateId);
    }

    [Fact]
    public async Task Change_Follows_To_Another_Input_Persisted()
    {
        // Arrange
        var original = Input.Create(new InputId(Guid.NewGuid()), "src-original", "Original").Value;
        var targetA = Input.Create(new InputId(Guid.NewGuid()), "src-A", "Target A").Value;
        var targetB = Input.Create(new InputId(Guid.NewGuid()), "src-B", "Target B").Value;

        original.ChangeFollows(targetA);

        InputRepository.Add(original);
        InputRepository.Add(targetA);
        InputRepository.Add(targetB);

        await PhoeNixDbContextSUT.SaveChangesAsync();

        // Act
        var tracked = await InputRepository.GetByIdAsync(original.TemplateId, CancellationToken.None);
        var result = tracked!.ChangeFollows(targetB);
        result.IsSuccess.Should().BeTrue();

        await PhoeNixDbContextSUT.SaveChangesAsync();

        // Assert
        var reloaded = await InputRepository.GetByIdAsync(original.TemplateId, CancellationToken.None);
        reloaded!.Follows!.Id.Should().Be(targetB.TemplateId);

        var updatedTargetA = await InputRepository.GetByIdAsync(targetA.TemplateId, CancellationToken.None);
        updatedTargetA!.Followers.Should().NotContain(f => f.Id == original.TemplateId);

        var updatedTargetB = await InputRepository.GetByIdAsync(targetB.TemplateId, CancellationToken.None);
        updatedTargetB!.Followers.Should().ContainSingle(f => f.Id == original.TemplateId);
    }

    [Fact]
    public async Task Input_Cannot_Follow_Itself()
    {
        // Arrange
        var input = Input.Create(new InputId(Guid.NewGuid()), "self", "self-follower").Value;

        InputRepository.Add(input);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        // Act
        var tracked = await InputRepository.GetByIdAsync(input.TemplateId, CancellationToken.None);
        var result = tracked!.ChangeFollows(tracked);

        // Assert
        result.IsFailure.Should().BeTrue();
        await PhoeNixDbContextSUT.SaveChangesAsync();

        var reloaded = await InputRepository.GetByIdAsync(input.TemplateId, CancellationToken.None);
        reloaded!.Follows.Should().BeNull();
    }
}