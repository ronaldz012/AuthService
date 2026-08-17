using System.Api.Attributes;
using System.Api.Result;
using Common.Contracts.authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Sales.Application.UseCases;
using Module.Sales.Application.UseCases.Registers.Close;
using Module.Sales.Application.UseCases.Registers.List;
using Module.Sales.Application.UseCases.Registers.Open;

namespace System.Api.Controllers.Sales
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Sales | CashRegisters")]
    [Authorize]
    public class CashRegisterController(RegisterUseCases registerUseCases, ICurrentUser currentUser) : ControllerBase
    {
        [HttpPost("Open")]
        [RequireFeature("closures", "create")]
        public async Task<IActionResult> Open([FromBody] OpenCashRegisterDto dto)
        {
            return await registerUseCases.OpenCashRegister.Execute(currentUser.ToActorContext(), dto).ToValueOrProblemDetails();
        }

        [HttpPost("Close")]
        [RequireFeature("closures", "update")]
        public async Task<IActionResult> Close([FromBody] CloseCashRegisterDto dto)
        {
            return await registerUseCases.CloseCashRegister.Execute(currentUser.ToActorContext(), dto).ToValueOrProblemDetails();
        }

        [HttpGet("Current")]
        [RequireFeature("pos", "read")]
        public async Task<IActionResult> Current()
        {
            return await registerUseCases.GetCurrentRegister.Execute(currentUser.ToActorContext()).ToValueOrProblemDetails();
        }

        [HttpGet("{id:guid}")]
        [RequireFeature("closures", "read")]
        public async Task<IActionResult> GetClosureDetail([FromRoute] Guid id, [FromQuery] bool includeStock = false)
        {
            return await registerUseCases.GetClosureDetail.Execute(currentUser.ToActorContext(), id, includeStock).ToValueOrProblemDetails();
        }

        [HttpGet]
        [RequireFeature("closures", "read")]
        public async Task<IActionResult> ListClosures([FromQuery] ClosuresQueryDto queryDto)
        {
            return await registerUseCases.ListClosures.Execute(currentUser.ToActorContext(), queryDto).ToValueOrProblemDetails();
        }
    }
}
