using Microsoft.Extensions.DependencyInjection;
using Module.Sales.Application.Abstraction;
using Module.Sales.Application.UseCases;
using Module.Sales.Application.UseCases.Sales.Create;
using Module.Sales.Application.UseCases.Sales.Get;
using Module.Sales.Application.UseCases.Sales.GetById;
using Module.Sales.Infrastructure.Persistence;

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
                return services;
        }
}