using Microsoft.Extensions.DependencyInjection;
using shared.Contracts.interfaces;
using shared.UseCases.cache;
using shared.UseCases.mapper;
using shared.UseCases.Roles;
using shared.UseCases.Users;

namespace shared.UseCases;

public static class UseCasesDependencyInjection
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
        => services.AddRolesUseCases()
                    .AddCache()
                    .AddUsersUseCases()
                    .AddMapper()
                    .AddServices()
                    .AddScoped<ICurrentUser, CurrentUserService>()
                    .AddScoped<IUserIntegrationService, UserIntegrationService>();
    static IServiceCollection AddServices(this IServiceCollection services)

    static IServiceCollection AddUsersUseCases(this IServiceCollection services)
        => services.AddScoped<UserUserCases>()
            .AddScoped<GetAllUsers>()
            .AddScoped<CreateUser>();

    static IServiceCollection AddCache(this IServiceCollection services)
    => services.AddScoped<IUserPermissionsCacheService, UserPermissionsCacheService>();


    static IServiceCollection AddRolesUseCases(this IServiceCollection services)
    => services.AddScoped<RoleUseCases>()
                .AddScoped<AddRole>()
                .AddScoped<GetRole>();
    static IServiceCollection AddMapper(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(typeof(MappingConfig).Assembly);
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();
        return services;
    }
}
