using System.Api.Attributes;
using System.Api.Result;
using System.Diagnostics.CodeAnalysis;
using Common.Contracts.authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Auth.Application.Abstraction;
using Module.Sales.Application.UseCases;
using Module.Sales.Application.UseCases.Sales.Create;
using Module.Sales.Application.UseCases.Sales.Get;

namespace System.Api.Controllers.Sales
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Sales | Sales")]
    [Authorize]
    public class SaleController(SaleUseCases saleUseCases, ISessionStateService currentUser) : ControllerBase
    {
        [HttpPost]
        [RequireFeature("pos", "create")]
        public async Task<IActionResult> CreateSale([FromBody] CreateSaleDto dto)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
            return await saleUseCases.CreateSale.Execute(actorResult.Value, dto).ToValueOrProblemDetails();
        }
        [HttpGet]
        [RequireFeature("sales", "read")]
        public async Task<IActionResult> GetSales([FromQuery] SalesQueryDto queryDto)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
            return await saleUseCases.GetListSales.Execute(actorResult.Value, queryDto).ToValueOrProblemDetails();
        }
        [HttpGet("{id:guid}/details")]
        [RequireFeature("sales", "read")]
        public async Task<IActionResult> GetSaleDetail([FromRoute] Guid id)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
            return await saleUseCases.GetSaleDetail.Execute(actorResult.Value, id).ToValueOrProblemDetails();
        }
    }
}
