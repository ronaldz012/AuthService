using System.Api.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using shared.Contracts.dtos.Modules;
using shared.Module.UseCases.Modules;


namespace System.Api.Controllers.Auth;

[Route("api/[controller]")]
[ApiController]
[Tags("Authentication | Modules")]

public class ModuleController(ModuleUseCases service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateModule([FromBody] CreateModuleDto createModuleDto)
    {
        return await service.CreateModuleUseCase.Execute(createModuleDto).ToValueOrProblemDetails();
    }

    [HttpGet]
    public async Task<IActionResult> GetModules()
    {
        return await service.ListModules.Execute().ToValueOrProblemDetails();
    }
}
