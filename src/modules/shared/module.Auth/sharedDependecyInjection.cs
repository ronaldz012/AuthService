using Microsoft.Extensions.DependencyInjection;
using shared.Contracts.interfaces;
using shared.UseCases.UseCases;
using shared.UseCases.UseCases.Autentication;
using shared.UseCases.UseCases.Features;

namespace shared.UseCases;

public static class SharedDependencyInjection
{
    public static IServiceCollection AddShared (this IServiceCollection services)
    {
        
         services.AddScoped<FeatureUseCases>()
            .AddScoped<CreateFeature>()
            .AddScoped<GetFeature>()
            .AddScoped<ListFeatures>();
         

         
         services.AddScoped<IFeatureService, FeatureService>();
         services.AddScoped<TenantService>();
         
         services.AddScoped<AutenticationUseCases>()
             .AddScoped<RegisterUser>()
             .AddScoped<RegisterDefaultUser>()
             .AddScoped<Login>()
             .AddScoped<IAuthenticateMe, AutenticateMe>()
             .AddScoped<CompletePublicRegister>()
             .AddScoped<VerifyUser>();
         
         return services;



    }
    
}