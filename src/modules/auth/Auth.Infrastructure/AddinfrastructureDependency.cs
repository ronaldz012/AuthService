using System;
using Auth.Infrastructure.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        IConfigurationSection tokenSettingsSection = configuration.GetSection(TokenSettings.SectionName);
        services.Configure<TokenSettings>(tokenSettingsSection);

        IConfigurationSection authSettingsSection = configuration.GetSection(AuthenticationSettings.SectionName);
        services.Configure<AuthenticationSettings>(authSettingsSection);
        return services;
        
    }
       
}
