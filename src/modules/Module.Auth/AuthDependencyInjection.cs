using Common.Contracts.authentication;
using Common.Contracts.branches;
using Common.Contracts.Seeder;
using Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Module.Auth.Application.Abstraction;
using Module.Auth.Application.UseCases.Autentication;
using Module.Auth.Application.UseCases.Autentication.AuthMe;
using Module.Auth.Application.UseCases.Autentication.Login;
using Module.Auth.Application.UseCases.Autentication.PublicLogin;
using Module.Auth.Application.UseCases.Autentication.SetupUserPassword;
using Module.Auth.Application.UseCases.Autentication.VerifiUser;
using Module.Auth.Application.UseCases.Autentication.VerifyToken;
using Module.Auth.Application.UseCases.Branches;
using Module.Auth.Application.UseCases.Branches.CreateBranch;
using Module.Auth.Application.UseCases.Branches.GetBranches;
using Module.Auth.Application.UseCases.Branches.UpdateBranch;
using Module.Auth.Application.UseCases.Branches.ToggleBranchStatus;
using Module.Auth.Application.UseCases.Branches.GetBranchDetails;
using Module.Auth.Application.UseCases.Features;
using Module.Auth.Application.UseCases.Roles;
using Module.Auth.Application.UseCases.Tenant;
using Module.Auth.Application.UseCases.Tenant.Create;
using Module.Auth.Application.UseCases.TenantDatabases;
using Module.Auth.Application.UseCases.TenantDatabases.Get;
using Module.Auth.Application.UseCases.TenantDatabases.GetById;
using Module.Auth.Application.UseCases.Users;
using Module.Auth.Application.UseCases.Users.CreateUser;
using Module.Auth.Application.UseCases.Users.CreateTenantAdmin;
using Module.Auth.Application.UseCases.Users.GetAllUsers;
using Module.Auth.Application.UseCases.Users.UpdateUserStatus;
using Module.Auth.Application.UseCases.Users.UpdateUser;
using Module.Auth.Application.UseCases.Users.GetUserDetails;
using Module.Auth.Application.UseCases.Users.ToggleUserType;
using Module.Auth.Application.UseCases.Users.Pending;
using Module.Auth.Infrastructure.Authentication;
using Module.Auth.Infrastructure.Authentication.EmailTemplates;
using Module.Auth.Infrastructure.Branches;
using Module.Auth.Infrastructure.Databases;
using Module.Auth.Infrastructure.Persistence;
using Module.Auth.Infrastructure.Seeder;
using SmtpSettings = Common.Services.SmtpSettings;
using Module.Auth.Application.UseCases.Roles.GetById;
using Module.Auth.Application.UseCases.Roles.Create;
using Module.Auth.Application.UseCases.Roles.GetRoles;

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
             .AddScoped<GetBranches>()
             .AddScoped<UpdateBranch>()
              .AddScoped<ToggleBranchStatus>()
              .AddScoped<GetBranchDetails>();

         services.AddScoped<RoleUseCases>()
             .AddScoped<GetRoleById>()
             .AddScoped<AddRole>()
             .AddScoped<GetRoles>();
         
        
         services.AddScoped<AutenticationUseCases>()
               .AddScoped<RegisterUser>()
               .AddScoped<RegisterDefaultUser>()
               .AddScoped<Login>()
               .AddScoped<CompletePublicRegister>()
               .AddScoped<VerifyUser>()
               .AddScoped<SetupUserPassword>()
               .AddScoped<VerifyToken>()
               .AddScoped<AuthMe>();
         
         services.AddScoped<UserUserCases>()
                 .AddScoped<GetAllUsers>()
                 .AddScoped<CreateUser>()
                 .AddScoped<CreateTenantAdmin>()
                 .AddScoped<UpdateUserStatus>()
                 .AddScoped<UpdateUser>()
                 .AddScoped<GetUserDetails>()
                 .AddScoped<ToggleUserType>();

         services.AddScoped<TenantDatabaseUseCases>()
             .AddScoped<GetTenantDatabases>()
             .AddScoped<GetTenantDatabaseDetails>();

         services.AddScoped<TenantUseCases>()
             .AddScoped<CreateTenant>() ;            

         

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
          services.AddScoped<ISessionStateService, SessionStateService>();

         services.AddScoped<IDbConnectionTester, DbConnectionTester>();
         services.AddScoped<ITenantDatabaseResolver, TenantDatabaseResolverService>();
         
         services.AddScoped<IDataSeeder, TenantDataBaseSeeder>()
             .AddScoped<IDataSeeder, FeatureSeeder>()
             .AddScoped<IDataSeeder, PlanSeeder>()
             .AddScoped<IDataSeeder, TenantSeeder>();

        services.AddScoped<ITenantConnectionContext, TenantConnectionContext>();

        services.AddDbContext<AuthDbContext>((sp, options) =>
        {
            var connection = configuration.GetConnectionString("DefaultConnection");
            options.UseNpgsql(connection,
                x => x.MigrationsHistoryTable("__EFMigrationsHistory_shared", null));
        });

         return services;
    }
    
}