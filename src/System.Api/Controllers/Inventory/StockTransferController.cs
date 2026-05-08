using System.Api.Filters;
using System.Api.Result;
using Inventory.Contracts.Dtos.Transfers;
using Inventory.UseCases.Transfers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace System.Api.Controllers.Inventory
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StockTransferController(StockTransferUseCases useCases) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> LisStockTransfers([FromQuery] StockTransferQueryDto queryDto)
        {
            return await useCases.ListStockTransfers.Execute(queryDto).ToValueOrProblemDetails();
        }

        [HttpPost]
        public async Task<IActionResult> CreateStockTransfer([FromBody] CreateStockTransferDto createStockTransferDto)
        {
            return await useCases.CreateStockTransfer.Execute(createStockTransferDto).ToValueOrProblemDetails();
        }
        [HttpPost("Resolve/{transferId:guid}")]
        public async Task<IActionResult> ResolveStockTransfer([FromRoute] Guid transferId, [FromBody] ResolveStockTransferDto resolveStockTransferDto)
        {
            return await useCases.ResolveStockTransfer
                .Execute(transferId, resolveStockTransferDto)
                .ToValueOrProblemDetails();
        }
        [HttpGet("{transferId:guid}")]
        public async Task<IActionResult> GetStockTransferDetails([FromRoute] Guid transferId)
        {
            return await useCases.StockTransferDetails.Execute(transferId).ToValueOrProblemDetails();
        }

        [HttpPatch("Cancel/{transferId:int}")]
        public async Task<IActionResult> CancelStockTransfer([FromRoute] int transferId)
        {
            return await useCases.CancelStockTransfer.Execute(transferId).ToValueOrProblemDetails();
        }

    }
}
