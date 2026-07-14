using Common.Contracts.authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Module.Inventory.Application.Abstraction;
using Module.Sales.Application.Abstraction;
using System.Infrastructure.Persistence;

namespace System.Infrastructure;

public static class SystemInfrastructureDependencyInjection
{
    public static IServiceCollection AddAppInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var tenant = sp.GetRequiredService<ITenantConnectionContext>();

            options.UseNpgsql(tenant.Connection);
        });

        services.AddScoped<ISalesDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IInvDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        return services;
    }
}
