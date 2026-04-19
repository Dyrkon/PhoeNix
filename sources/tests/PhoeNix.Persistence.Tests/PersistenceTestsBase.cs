using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhoeNix.Common.Tests;
using PhoeNix.Application.Repositories;
using Xunit.Abstractions;

namespace PhoeNix.Persistence.Tests;

public class PersistenceTestsBase : IAsyncLifetime
{
    protected PersistenceTestsBase(ITestOutputHelper output)
    {
        var services = new ServiceCollection();

        services.AddInMemoryPersistence(Guid.NewGuid().ToString());

        ServiceProvider = services.BuildServiceProvider();
        PhoeNixDbContextSUT = ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    protected ServiceProvider ServiceProvider { get; }
    protected ApplicationDbContext PhoeNixDbContextSUT { get; }

    protected IUserRepository UserRepository => ServiceProvider.GetRequiredService<IUserRepository>();

    protected IModuleTemplateRepository ModuleTemplateRepository =>
        ServiceProvider.GetRequiredService<IModuleTemplateRepository>();

    protected IConfigurationRepository ConfigurationRepository =>
        ServiceProvider.GetRequiredService<IConfigurationRepository>();

    public async Task InitializeAsync()
    {
        await PhoeNixDbContextSUT.Database.EnsureDeletedAsync();
        await PhoeNixDbContextSUT.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await PhoeNixDbContextSUT.Database.EnsureDeletedAsync();
        await PhoeNixDbContextSUT.DisposeAsync();
    }
}