using System;

namespace Auth.Data;

using System;
using Auth.Data.Persistence;
using Auth.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

public static class DataDependencyInjection
{
    public static IServiceCollection AddAuthData(this IServiceCollection services)
        => services.AddScoped<IUserRepository, UserRepository>()
                   .AddScoped<AuthDbContext>();
              
}