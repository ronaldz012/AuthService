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
        var tokenSettings = tokenSettingsSection.Get<TokenSettings>();
        if (tokenSettings == null || string.IsNullOrEmpty(tokenSettings.SecretKey))
        {
            throw new ApplicationException($"{TokenSettings.SectionName} no está configurado correctamente");
        }

        services.AddScoped<ITokenGenerator, TokenGenerator>();
        return services;
        
    }
       
}
