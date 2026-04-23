using Microsoft.Extensions.DependencyInjection;

namespace Fintrack.Application.Tests;

public sealed class ArchitectureSmokeTests
{
    [Fact]
    public void AddApplication_LeavesServiceCollectionUsable()
    {
        var services = new ServiceCollection();

        var result = services.AddApplication();

        Assert.Same(services, result);
    }
}
