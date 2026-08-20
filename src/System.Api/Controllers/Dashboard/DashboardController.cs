using System.Api.Attributes;
using System.Api.Result;
using Common.Contracts.authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Auth.Application.Abstraction;
using Module.Sales.Application.UseCases;
using Module.Sales.Application.UseCases.Registers.TodaySales;

namespace System.Api.Controllers.Dashboard
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Dashboard")]
    [Authorize]
    public class DashboardController(RegisterUseCases registerUseCases, ISessionStateService currentUser) : ControllerBase
    {
        [HttpGet("today-sales")]
        [RequireFeature("sales", "read")]
        public async Task<IActionResult> GetTodaySales()
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)
                return actorResult.ToValueOrProblemDetails();


            return await registerUseCases.GetTodaySales.Execute(currentUser.GetActorContext().Value!).ToValueOrProblemDetails();
        }

        [HttpGet("last-closure")]
        [RequireFeature("closures", "read")]
        public async Task<IActionResult> GetLastClosure()
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)
                return actorResult.ToValueOrProblemDetails();
            return await registerUseCases.GetLastClosureSummary.Execute(currentUser.GetActorContext().Value!).ToValueOrProblemDetails();
        }
    }
}