using System;
using Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common;

public static class CommonDependencyInjection
{
    public static IServiceCollection AddCommon(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IEmailService, EmailService>();
        IConfigurationSection tokenSettingsSection = configuration.GetSection(SmtpSettings.SectionName);
        services.Configure<SmtpSettings>(tokenSettingsSection);
        return services;
    }
}
