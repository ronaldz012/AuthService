using Common.Contracts.sales;
using Microsoft.Extensions.DependencyInjection;
using Module.Sales.Application.UseCases;
using Module.Sales.Application.UseCases.Registers.Open;
using Module.Sales.Application.UseCases.Sales.Create;
using Module.Sales.Application.UseCases.Sales.Get;
using Module.Sales.Application.UseCases.Sales.GetById;
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
                
                services.AddScoped<ISalesIntegrationService, SalesIntegrationService>();

                return services;
        }
}