using Auth.UseCases.mapper;
using Auth.UseCases.Menus;
using Auth.UseCases.Modules;
using Auth.UseCases.Roles;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.UseCases;

public static class UseCasesDependencyInjection
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
        => services.AddMenuUseCases()
                    .AddModulesUseCases()
                    .AddRolesUseCases()
                   .AddMapper();

    public static IServiceCollection AddMenuUseCases(this IServiceCollection services)
    => services.AddScoped<MenuUseCases>()
                .AddScoped<AddMenu>()
                .AddScoped<GetMenu>()
                .AddScoped<GetAllMenus>()
                .AddScoped<UpdateMenu>()
                .AddScoped<DeleteMenu>();

    public static IServiceCollection AddModulesUseCases(this IServiceCollection services)
    => services.AddScoped<ModulesUseCases>()
                .AddScoped<AddModule>()
                .AddScoped<GetModule>()
                .AddScoped<GetAllModules>();
    // .AddScoped<UpdateModule>()
    // .AddScoped<DeleteModule>();

    public static IServiceCollection AddRolesUseCases(this IServiceCollection services)
    => services.AddScoped<RoleUseCases>()
                .AddScoped<AddRole>()
                .AddScoped<GetRole>();
    public static IServiceCollection AddMapper(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(typeof(MappingConfig).Assembly);
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();
        return services;
    }
}
