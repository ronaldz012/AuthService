using System.Api.Result;
using Common.Contracts.authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Sales.Application.UseCases;
using Module.Sales.Application.UseCases.Sales.Create;
using Module.Sales.Application.UseCases.Sales.Get;

namespace System.Api.Controllers.Sales
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Sales | Sales")]
    [Authorize]
    public class SaleController(SaleUseCases saleUseCases, ICurrentUser currentUser) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateSale([FromBody] CreateSaleDto dto)
        {
            return await saleUseCases.CreateSale.Execute(currentUser.ToActorContext(), dto).ToValueOrProblemDetails();
        }
        [HttpGet]
        public async Task<IActionResult> GetSales([FromQuery] SalesQueryDto queryDto)
        {
            return await saleUseCases.GetListSales.Execute(currentUser.ToActorContext(), queryDto).ToValueOrProblemDetails();
        }
        [HttpGet("{id:guid}/details")]
        public async Task<IActionResult> GetSaleDetail([FromRoute] Guid id)
        {
            return await saleUseCases.GetSaleDetail.Execute(currentUser.ToActorContext(), id).ToValueOrProblemDetails();
        }
    }
}
