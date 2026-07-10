using Common.Contracts.authentication;
using Common.Contracts.sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Module.Sales.Application.Abstraction;
using Module.Sales.Application.UseCases;
using Module.Sales.Application.UseCases.Sales.Create;
using Module.Sales.Application.UseCases.Sales.Get;
using Module.Sales.Application.UseCases.Sales.GetById;
using Module.Sales.Infrastructure.Persistence;
using Module.Sales.Infrastructure;

namespace Module.Sales;

public static class SalesDependencyInjection
{

        public static IServiceCollection AddSales(this IServiceCollection services)
        {
                services.AddScoped<SaleUseCases>()
                        .AddScoped<CreateSale>()
                        .AddScoped<GetListSales>()
                        .AddScoped<GetSaleDetail>();
                
                services.AddScoped<ISalesDbContext>(sp =>
                        sp.GetRequiredService<SalesDbContext>());

                services.AddScoped<ISalesIntegrationService, SalesIntegrationService>();

                services.AddDbContext<SalesDbContext>((sp, options) =>
                {
                    var configuration = sp.GetRequiredService<IConfiguration>();
                    var tenantConnection = configuration.GetConnectionString("TenantConnection")!;
                    var tenant = sp.GetRequiredService<ITenantConnectionContext>();

                    if (string.IsNullOrEmpty(tenant.DatabaseName))
                        throw new InvalidOperationException("DatabaseName is not set on tenant context");
                    if (string.IsNullOrEmpty(tenant.Schema))
                        throw new InvalidOperationException("Schema is not set on tenant context");

                    var connString = BuildConnectionString(tenantConnection, tenant.Schema, tenant.DatabaseName);
                    options.UseNpgsql(connString,
                        x => x.MigrationsHistoryTable("__EFMigrationsHistory_sales", tenant.Schema));
                });

                return services;
        }

        private static string BuildConnectionString(string baseConnection, string? schema, string? databaseName)
        {
            if (string.IsNullOrEmpty(databaseName))
                return baseConnection;

            var builder = new NpgsqlConnectionStringBuilder(baseConnection)
            {
                Database = databaseName,
                SearchPath = schema ?? "",
            };
            return builder.ConnectionString;
        }
}