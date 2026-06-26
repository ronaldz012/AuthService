using Common.Contracts.authentication;
using Common.Contracts.branches;
using Common.Contracts.Seeder;
using Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Module.Auth.Application.Abstraction;
using Module.Auth.Application.UseCases.Autentication;
using Module.Auth.Application.UseCases.Autentication.Login;
using Module.Auth.Application.UseCases.Autentication.PublicLogin;
using Module.Auth.Application.UseCases.Autentication.VerifiUser;
using Module.Auth.Application.UseCases.Branches;
using Module.Auth.Application.UseCases.Branches.CreateBranch;
using Module.Auth.Application.UseCases.Branches.GetBranches;
using Module.Auth.Application.UseCases.Features;
using Module.Auth.Application.UseCases.Roles;
using Module.Auth.Application.UseCases.Users;
using Module.Auth.Application.UseCases.Users.CreateUser;
using Module.Auth.Application.UseCases.Users.GetAllUsers;
using Module.Auth.Application.UseCases.Users.Pending;
using Module.Auth.Infrastructure.Authentication;
using Module.Auth.Infrastructure.Authentication.EmailTemplates;
using Module.Auth.Infrastructure.Branches;
using Module.Auth.Infrastructure.Persistence;
using Module.Auth.Infrastructure.Seeder;
using SmtpSettings = Common.Services.SmtpSettings;

namespace Module.Auth;

public static class SharedDependencyInjection
{
    public static IServiceCollection AuthDependencyInjection (this IServiceCollection services, IConfiguration configuration)
    {
        
         services.AddScoped<FeatureUseCases>()
            .AddScoped<CreateFeature>()
            .AddScoped<GetFeature>()
            .AddScoped<ListFeatures>();
         

         services.AddScoped<BranchesUseCases>()
             .AddScoped<CreateBranch>()
             .AddScoped<GetBranches>();

         services.AddScoped<RoleUseCases>()
             .AddScoped<GetRole>()
             .AddScoped<AddRole>();
         
        
         services.AddScoped<AutenticationUseCases>()
             .AddScoped<RegisterUser>()
             .AddScoped<RegisterDefaultUser>()
             .AddScoped<Login>()
             .AddScoped<AutenticateMe>()
             .AddScoped<CompletePublicRegister>()
             .AddScoped<VerifyUser>();
         
         services.AddScoped<UserUserCases>()
                 .AddScoped<GetAllUsers>()
                 .AddScoped<CreateUser>();

         
         //OTHERS
         IConfigurationSection tokenSettingsSection = configuration.GetSection(TokenSettings.SectionName);
         services.Configure<TokenSettings>(tokenSettingsSection);

         IConfigurationSection authSettingsSection = configuration.GetSection(AuthenticationSettings.SectionName);
         services.Configure<AuthenticationSettings>(authSettingsSection);

         IConfigurationSection smtpSettingsSection = configuration.GetSection(SmtpSettings.SectionName);
         services.Configure<SmtpSettings>(smtpSettingsSection);

         IConfigurationSection projectInfoSection = configuration.GetSection(ProjectInfo.SectionName);
         services.Configure<ProjectInfo>(projectInfoSection);

         services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();
         services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();

         services.AddSingleton<EmailTemplateRenderer>();

         services.AddScoped<IAuthDbContext>(sp =>
             sp.GetRequiredService<AuthDbContext>());
         // Infrastructure service registrations
         services.AddScoped<IBranchService, BranchService>();
         services.AddScoped<IEmailVerificationService, EmailVerificationService>();
         //INTEGRATION
         services.AddScoped<ICurrentUser, CurrentUserService>()
             .AddScoped<IUserIntegrationService, UserIntegrationService>();
         services.AddScoped<IUserPermissionsCacheService, UserPermissionsCacheService>();
         
         services.AddScoped<IDataSeeder, DataBaseSeeder>()
             .AddScoped<IDataSeeder, FeatureSeeder>()
             .AddScoped<IDataSeeder, PlanSeeder>();
             

         
         return services;
    }
    
}