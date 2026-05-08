using System.Api.Filters;
using System.Api.Result;
using Inventory.Contracts.Dtos.Receptions;
using Inventory.UseCases.Receptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace System.Api.Controllers.Inventory
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Inventory | Receptions")]
    [Authorize]
    public class ReceptionController(ReceptionUseCases service) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateReception(CreateStockReceptionDto dto)
        {
            return await service.CreateReceptionUc.Execute(dto).ToValueOrProblemDetails();
        }

        [HttpGet]
        public async Task<IActionResult> ListReceptions([FromQuery] ReceptionQueryDto dto)
        {
            return await service.ListReceptions.Execute(dto).ToValueOrProblemDetails();
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetReception([FromRoute] Guid id)
        {
            return await service.GetReception.Execute(id).ToValueOrProblemDetails();
        }

        [HttpGet("{id:guid}/labels")]
        public async Task<IActionResult> GetLabels([FromRoute] Guid id)
        {
            return await service.ReceptionLabels.Execute(id).ToValueOrProblemDetails();
        }
    }
}
