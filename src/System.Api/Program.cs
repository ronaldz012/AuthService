using System.Api.Data;
using System.Api.Filters;
using System.Api.Middlewares;
using System.Api.Migration;
using System.Api.Result;
using System.Text;
using Auth.Data;
using Auth.Infrastructure;
using Auth.Infrastructure.Authentication;
using Auth.UseCases;
using Branches.module;
using Branches.module.Data;
using Inventory.Infrastructure;
using Inventory.Infrastructure.Notifications;
using Inventory.UseCases;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using sales.Module.Data;
using sales.UseCases;
using Common;
using Common.Data;
using Common.Services;
using Inventory.Data;
using shared.Module.Data;
using shared.Module.UseCases;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo { Title = "Sales API", Version = "v1" });

  // ── Security definition: Bearer + Branch IDs ─────────────────
  c.AddSecurityDefinition("BearerWithBranch", new OpenApiSecurityScheme
  {
    Type = SecuritySchemeType.ApiKey,   // "ApiKey" permite campo libre
    In = ParameterLocation.Header,
    Name = "Authorization",             // header real que se envía
    Description =
          "**JWT** — ingrese: `Bearer <token>`\n\n" +
          "**X-Branch-Id** — IDs de sucursal separados por coma (ej: `1,2,3`)\n\n" +
          "Formato combinado en este campo → `Bearer <token> | branches: 1,2,3`\n\n" +
          "> El UI enviará el valor tal cual; use el campo de abajo para los branch IDs."
  });

  // ── Definición separada para X-Branch-Id ────────────────────
  c.AddSecurityDefinition("BranchId", new OpenApiSecurityScheme
  {
    Type = SecuritySchemeType.ApiKey,
    In = ParameterLocation.Header,
    Name = "X-Branch-Id",
    Description = "IDs de sucursal separados por coma. Ejemplo: `1,2,3`"
  });
  c.AddSecurityDefinition("Tenant", new OpenApiSecurityScheme  // "Tenant"
  {
    Type = SecuritySchemeType.ApiKey,
    In = ParameterLocation.Header,
    Name = "X-Forwarded-Host",
    Description = "Schema del tenant. Ejemplo: `client1`"
  });

  // ── Ambos requeridos globalmente ─────────────────────────────
  c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "BranchId"
                }
            },
            Array.Empty<string>()
        },
        {
          new OpenApiSecurityScheme
          {
            Reference = new OpenApiReference
            {
              Type = ReferenceType.SecurityScheme,
              Id = "Tenant"  // mismo nombre
            }
          },
          Array.Empty<string>()
        }
        
    });

  // ── Bearer estándar (para el candado verde) ──────────────────
  c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    Description =
          "JWT Authorization header usando el esquema Bearer.\n\n" +
          "Ingrese **Bearer** [espacio] y luego su token.\n\n" +
          "Ejemplo: `Bearer eyJhbGci...`",
    Name = "Authorization",
    In = ParameterLocation.Header,
    Type = SecuritySchemeType.ApiKey,
    Scheme = "Bearer"
  });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(options =>
{
  // The default scheme for authenticating API requests (JWT)
  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

  // The default scheme for challenging unauthenticated users (JWT)
  options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddGoogle(options =>
{
  options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
  options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
  // Solo para desarrollo - callback URL
  options.CallbackPath = "/api/ExternalAuth/google-login-complete";
}

)
.AddJwtBearer(options =>
{
  options.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = builder.Configuration["TokenSettings:Issuer"]!,
    ValidAudience = builder.Configuration["TokenSettings:Audience"]!,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["TokenSettings:SecretKey"]!))
  };
});

builder.Services.AddDbContext<SharedDbContext>((sp, options) =>
{
  var connection = builder.Configuration.GetConnectionString("SharedConnection");
  options.UseNpgsql(connection,
    x => x.MigrationsHistoryTable("__EFMigrationsHistory_shared", null));
});


var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection")!;

builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddDbContext<AuthDbContext>((sp, options) =>
{
  options.UseNpgsql(defaultConnection,
    x => x.MigrationsHistoryTable("__EFMigrationsHistory_auth", null));
});

builder.Services.AddDbContext<BranchDbContext>((sp, options) =>
{
  options.UseNpgsql(defaultConnection,
    x => x.MigrationsHistoryTable("__EFMigrationsHistory_branches", null));
});

builder.Services.AddDbContext<InvDbContext>((sp, options) =>
{
  options.UseNpgsql(defaultConnection,
    x => x.MigrationsHistoryTable("__EFMigrationsHistory_inventory", null));
});

builder.Services.AddScoped<ITenantContext, TenantContext>();

builder.Services.AddDbContext<SalesDbContext>((sp, options) =>
{

  var tenantContext = sp.GetRequiredService<ITenantContext>();

  options.UseNpgsql(defaultConnection, x =>
    x.MigrationsHistoryTable("__EFMigrationsHistory_sales", tenantContext.Schema));

});

builder.Services.Configure<TenantOptions>(
  builder.Configuration.GetSection(TenantOptions.Section));



builder.Services.AddControllers(options =>
{
  options.Filters.Add<ValidationFilter>();
});
// Desactivar el comportamiento automático de [ApiController]
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
  options.SuppressModelStateInvalidFilter = true;
});
builder.Services.AddMemoryCache();
builder.Services.AddAuthData()
                .AddUseCases()
                .AddInfrastructure(builder.Configuration)
                .AddCommon(builder.Configuration)
                .AddBranch(builder.Configuration)
                .AddInventory()
                .AddSales()
                .AddShared();
//EXTRAER en un DI
builder.Services.AddSignalR();
builder.Services.AddScoped<InventorySignalRStockNotifier>();   // tu notifier
builder.Services.AddScoped<MigrationService>();
builder.Services.AddScoped<TenantMigrationOrchestrator>();
//
builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowAll", policy =>
  {
    policy.AllowAnyOrigin()   // Permite solicitudes desde cualquier origen
      .AllowAnyHeader()   // Permite cualquier encabezado
      .AllowAnyMethod();  // Permite cualquier método HTTP
  });
});


var app = builder.Build();
app.UseCors("AllowAll");
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}
app.MapHub<NotificationHub>("/hubs/notifications");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TenantMiddleware>();
app.UseMiddleware<BranchMiddleware>();
app.MapControllers();
app.Run();


