using GymManagement.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GymManagement.Infrastructure;                 

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ISubscriptionsService, SubscriptionsService>();
        return services;
    }
}