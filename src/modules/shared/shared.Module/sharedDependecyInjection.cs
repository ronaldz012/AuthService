using Microsoft.Extensions.DependencyInjection;
using shared.Contracts.interfaces;
using shared.Module.Data;
using shared.Module.UseCases.Features;
using shared.Module.UseCases.Modules;


namespace shared.Module.UseCases;

public static class SharedDependencyInjection
{
    public static IServiceCollection AddShared (this IServiceCollection services)
    {
        
         services.AddScoped<FeatureUseCases>()
            .AddScoped<CreateFeature>()
            .AddScoped<GetFeature>()
            .AddScoped<ListFeatures>();
         
         services.AddScoped<ModuleUseCases>()
             .AddScoped<CreateModuleUseCase>().AddScoped<ListModules>();
         
         services.AddScoped<IFeatureService, FeatureService>();
         services.AddScoped<SharedDbContext>();
         services.AddScoped<TenantService>();
         return services;
    }
    
}