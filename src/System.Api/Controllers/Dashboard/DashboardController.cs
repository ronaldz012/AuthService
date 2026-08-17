using System.Api.Attributes;
using System.Api.Result;
using Common.Contracts.authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Sales.Application.UseCases;
using Module.Sales.Application.UseCases.Registers.TodaySales;

namespace System.Api.Controllers.Dashboard
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Dashboard")]
    [Authorize]
    public class DashboardController(RegisterUseCases registerUseCases, ICurrentUser currentUser) : ControllerBase
    {
        [HttpGet("today-sales")]
        [RequireFeature("sales", "read")]
        public async Task<IActionResult> GetTodaySales()
        {
            return await registerUseCases.GetTodaySales.Execute(currentUser.ToActorContext()).ToValueOrProblemDetails();
        }

        [HttpGet("last-closure")]
        [RequireFeature("closures", "read")]
        public async Task<IActionResult> GetLastClosure()
        {
            return await registerUseCases.GetLastClosureSummary.Execute(currentUser.ToActorContext()).ToValueOrProblemDetails();
        }
    }
}