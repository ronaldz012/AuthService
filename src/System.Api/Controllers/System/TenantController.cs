using System.Api.Result;
using Microsoft.AspNetCore.Mvc;
using shared.Contracts.dtos;
using shared.Module.UseCases;

namespace System.Api.Controllers.System;
[ApiController]
[Route("api/system/migrations")]
public class TenantController(TenantService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateNewTenant([FromBody]CreateTenantDto tenant)
    {
        return await service.CreateTenantAsync(tenant).ToValueOrProblemDetails();
    }
}