using System;
using Auth.Infrastructure.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TokenSettings>(
            options => configuration.GetSection(TokenSettings.SectionName)
    );

        services.AddScoped<ITokenGenerator, TokenGenerator>();
        
        return services;
        
    }
       
}
