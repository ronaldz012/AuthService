using Auth.Data.Persistence;
using Microsoft.Extensions.DependencyInjection;
namespace Auth.Data;


public static class DataDependencyInjection
{
    public static IServiceCollection AddAuthData(this IServiceCollection services)
        => services.AddScoped<AuthDbContext>();
              
}