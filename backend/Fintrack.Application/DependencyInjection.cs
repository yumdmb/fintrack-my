using Microsoft.Extensions.DependencyInjection;

namespace Fintrack.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
