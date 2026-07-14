using System.Api.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Sales.Application.UseCases.Registers.Open;

namespace System.Api.Controllers.Sales
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Sales | CashRegisters")]
    [Authorize]
    public class CashRegisterController(OpenCashRegister openCashRegister) : ControllerBase
    {
        [HttpPost("Open")]
        public async Task<IActionResult> Open([FromBody] OpenCashRegisterDto dto)
        {
            return await openCashRegister.Execute(dto).ToValueOrProblemDetails();
        }
    }
}
