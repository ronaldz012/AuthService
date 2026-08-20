using System.Api.Attributes;
using System.Api.Result;
using Common.Contracts.authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Auth.Application.Abstraction;
using Module.Inventory.Application.UseCases.Transfers;
using Module.Inventory.Application.UseCases.Transfers.Create;
using Module.Inventory.Application.UseCases.Transfers.Get;
using Module.Inventory.Application.UseCases.Transfers.Resolve;

namespace System.Api.Controllers.Inventory
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StockTransferController(StockTransferUseCases useCases, ISessionStateService currentUser) : ControllerBase
    {
        [HttpGet]
        [RequireFeature("transfers", "read")]
        public async Task<IActionResult> LisStockTransfers([FromQuery] StockTransferQueryDto queryDto)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
            return await useCases.ListStockTransfers.Execute(actorResult.Value, queryDto).ToValueOrProblemDetails();
        }

        [HttpPost]
        [RequireFeature("transfers", "create")]
        public async Task<IActionResult> CreateStockTransfer([FromBody] CreateStockTransferDto createStockTransferDto)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
            return await useCases.CreateStockTransfer.Execute(actorResult.Value, createStockTransferDto).ToValueOrProblemDetails();
        }
        [HttpPost("Resolve/{transferId:guid}")]
        [RequireFeature("transfers", "update")]
        public async Task<IActionResult> ResolveStockTransfer([FromRoute] Guid transferId, [FromBody] ResolveStockTransferDto resolveStockTransferDto)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
            return await useCases.ResolveStockTransfer
                .Execute(actorResult.Value, transferId, resolveStockTransferDto)
                .ToValueOrProblemDetails();
        }
        [HttpGet("{transferId:guid}")]
        [RequireFeature("transfers", "read")]
        public async Task<IActionResult> GetStockTransferDetails([FromRoute] Guid transferId)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
            return await useCases.StockTransferDetails.Execute(actorResult.Value, transferId).ToValueOrProblemDetails();
        }

        [HttpPatch("Cancel/{transferId:guid}")]
        [RequireFeature("transfers", "delete")]
        public async Task<IActionResult> CancelStockTransfer([FromRoute] Guid transferId)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
            return await useCases.CancelStockTransfer.Execute(actorResult.Value, transferId).ToValueOrProblemDetails();
        }

    }
}
