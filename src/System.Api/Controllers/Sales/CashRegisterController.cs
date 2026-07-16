using System.Api.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Sales.Application.UseCases;
using Module.Sales.Application.UseCases.Registers.Close;
using Module.Sales.Application.UseCases.Registers.Open;

namespace System.Api.Controllers.Sales
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Sales | CashRegisters")]
    [Authorize]
    public class CashRegisterController(RegisterUseCases registerUseCases) : ControllerBase
    {
        [HttpPost("Open")]
        public async Task<IActionResult> Open([FromBody] OpenCashRegisterDto dto)
        {
            return await registerUseCases.OpenCashRegister.Execute(dto).ToValueOrProblemDetails();
        }

        [HttpPost("Close")]
        public async Task<IActionResult> Close([FromBody] CloseCashRegisterDto dto)
        {
            return await registerUseCases.CloseCashRegister.Execute(dto).ToValueOrProblemDetails();
        }

        [HttpGet("Current")]
        public async Task<IActionResult> Current()
        {
            return await registerUseCases.GetCurrentRegister.Execute().ToValueOrProblemDetails();
        }
    }
}
