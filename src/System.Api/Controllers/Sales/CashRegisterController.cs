using System.Api.Attributes;
using System.Api.Result;
using Common.Contracts.authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Auth.Application.Abstraction;
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
    public class CashRegisterController(RegisterUseCases registerUseCases, ISessionStateService currentUser) : ControllerBase
    {
        [HttpPost("Open")]
        [RequireFeature("closures", "create")]
        public async Task<IActionResult> Open([FromBody] OpenCashRegisterDto dto)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
            return await registerUseCases.OpenCashRegister.Execute(actorResult.Value, dto).ToValueOrProblemDetails();
        }

        [HttpPost("Close")]
        [RequireFeature("closures", "update")]
        public async Task<IActionResult> Close([FromBody] CloseCashRegisterDto dto)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
            return await registerUseCases.CloseCashRegister.Execute(actorResult.Value, dto).ToValueOrProblemDetails();
        }

        [HttpGet("Current")]
        [RequireFeature("pos", "read")]
        public async Task<IActionResult> Current()
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
            return await registerUseCases.GetCurrentRegister.Execute(actorResult.Value).ToValueOrProblemDetails();
        }

        [HttpGet("Current/details")]
        [RequireFeature("pos", "read")]
        public async Task<IActionResult> CurrentDetails()
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
            return await registerUseCases.GetClosureDetail.ExecuteCurrent(actorResult.Value).ToValueOrProblemDetails();
        }

        [HttpGet("{id:guid}")]
        [RequireFeature("closures", "read")]
        public async Task<IActionResult> GetClosureDetail([FromRoute] Guid id)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
            return await registerUseCases.GetClosureDetail.Execute(actorResult.Value, id).ToValueOrProblemDetails();
        }

        [HttpGet]
        [RequireFeature("closures", "read")]
        public async Task<IActionResult> ListClosures([FromQuery] ClosuresQueryDto queryDto)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)  return actorResult.ToValueOrProblemDetails();
            return await registerUseCases.ListClosures.Execute(actorResult.Value, queryDto).ToValueOrProblemDetails();
        }
    }
}
