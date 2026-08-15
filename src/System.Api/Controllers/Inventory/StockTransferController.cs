using System.Api.Filters;
using System.Api.Result;
using Common.Contracts.authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Inventory.Application.UseCases.Transfers;
using Module.Inventory.Application.UseCases.Transfers.Create;
using Module.Inventory.Application.UseCases.Transfers.Get;
using Module.Inventory.Application.UseCases.Transfers.Resolve;

namespace System.Api.Controllers.Inventory
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StockTransferController(StockTransferUseCases useCases, ICurrentUser currentUser) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> LisStockTransfers([FromQuery] StockTransferQueryDto queryDto)
        {
            return await useCases.ListStockTransfers.Execute(currentUser.ToActorContext(), queryDto).ToValueOrProblemDetails();
        }

        [HttpPost]
        public async Task<IActionResult> CreateStockTransfer([FromBody] CreateStockTransferDto createStockTransferDto)
        {
            return await useCases.CreateStockTransfer.Execute(currentUser.ToActorContext(), createStockTransferDto).ToValueOrProblemDetails();
        }
        [HttpPost("Resolve/{transferId:guid}")]
        public async Task<IActionResult> ResolveStockTransfer([FromRoute] Guid transferId, [FromBody] ResolveStockTransferDto resolveStockTransferDto)
        {
            return await useCases.ResolveStockTransfer
                .Execute(currentUser.ToActorContext(), transferId, resolveStockTransferDto)
                .ToValueOrProblemDetails();
        }
        [HttpGet("{transferId:guid}")]
        public async Task<IActionResult> GetStockTransferDetails([FromRoute] Guid transferId)
        {
            return await useCases.StockTransferDetails.Execute(currentUser.ToActorContext(), transferId).ToValueOrProblemDetails();
        }

        [HttpPatch("Cancel/{transferId:guid}")]
        public async Task<IActionResult> CancelStockTransfer([FromRoute] Guid transferId)
        {
            return await useCases.CancelStockTransfer.Execute(currentUser.ToActorContext(), transferId).ToValueOrProblemDetails();
        }

    }
}
