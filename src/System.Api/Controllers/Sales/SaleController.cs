using System.Api.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Sales.Application.UseCases;
using Module.Sales.Application.UseCases.Sales.Create;

namespace System.Api.Controllers.Sales
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Sales | Sales")]
    [Authorize]
    public class SaleController(SaleUseCases saleUseCases) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateSale([FromBody] CreateSaleDto dto)
        {
            return await saleUseCases.CreateSale.Execute(dto).ToValueOrProblemDetails();
        }
    }
}
