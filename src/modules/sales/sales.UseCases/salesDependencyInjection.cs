using Microsoft.Extensions.DependencyInjection;
using sales.UseCases.UseCases;

namespace sales.UseCases;

public static class salesDependencyInjection
{

        public static IServiceCollection AddSales(this IServiceCollection services)
        {
                services.AddScoped<SaleUseCases>()
                        .AddScoped<CreateSale>()
                        .AddScoped<GetListSales>()
                        .AddScoped<GetSaleDetail>();
                return services;
        }
}