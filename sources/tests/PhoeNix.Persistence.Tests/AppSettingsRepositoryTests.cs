using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.AppSettings;
using PhoeNix.Domain.Entities.Users;
using Xunit.Abstractions;

namespace PhoeNix.Persistence.Tests;

public class AppSettingsRepositoryTests : PersistenceTestsBase
{
    private static readonly UserId OwnerId = new(Guid.NewGuid());
    private IAppSettingsRepository AppSettingsRepository =>
        ServiceProvider.GetRequiredService<IAppSettingsRepository>();

    public AppSettingsRepositoryTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task GetAsync_Should_Return_Null_When_No_Settings_Exist()
    {
        var result = await AppSettingsRepository.GetAsync(OwnerId, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_Should_Return_Settings_When_Seeded()
    {
        var settings = AppSettings.CreateDefault(new AppSettingsId(Guid.NewGuid()), OwnerId);
        await PhoeNixDbContextSUT.AppSettings.AddAsync(settings);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        var result = await AppSettingsRepository.GetAsync(OwnerId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.FileStorageRootPath.Should().Be(settings.FileStorageRootPath);
        result.SshCaKeyName.Should().Be(settings.SshCaKeyName);
    }
}