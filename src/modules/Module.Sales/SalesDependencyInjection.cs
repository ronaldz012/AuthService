using Common.Contracts.authentication;
using Common.Contracts.sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Module.Sales.Application.Abstraction;
using Module.Sales.Application.UseCases;
using Module.Sales.Application.UseCases.Registers.Open;
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
                        .AddScoped<GetSaleDetail>()
                        .AddScoped<OpenCashRegister>();
                
                services.AddScoped<ISalesDbContext>(sp =>
                        sp.GetRequiredService<SalesDbContext>());

                services.AddScoped<ISalesIntegrationService, SalesIntegrationService>();

                services.AddDbContext<SalesDbContext>((sp, options) =>
                {
                    var tenant = sp.GetRequiredService<ITenantConnectionContext>();

                    options.UseNpgsql(tenant.Connection,
                        x => x.MigrationsHistoryTable("__EFMigrationsHistory_sales", tenant.Schema));
                });

                return services;
        }
}