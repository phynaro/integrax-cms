using Api.Repositories;
using Api.Services;
using Audit.Core.Interfaces;
using Clients.Core.Interfaces;
using Identity.Core.Interfaces;
using Portfolios.Core.Interfaces;

namespace Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IPortfolioRepository, PortfolioRepository>();
        services.AddScoped<IAuditEventRepository, AuditEventRepository>();
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuditService, AuditService>();
        return services;
    }
}
