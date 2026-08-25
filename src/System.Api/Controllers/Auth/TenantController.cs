using System.Api.Filters;
using System.Api.Result;
using Microsoft.AspNetCore.Mvc;
using Module.Auth.Application.UseCases.Tenant;
using Module.Auth.Application.UseCases.Tenant.Create;
using Module.Auth.Application.UseCases.TenantDatabases;

namespace System.Api.Controllers.Auth;
[ApiController]
[Route("api/system/[controller]")]
[Tags("Admin | System")]
[ApiKey]
public class TenantController(TenantDatabaseUseCases tenantDatabaseUseCases, TenantUseCases tenantUseCases) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTenantDatabases()
    {
        return await tenantDatabaseUseCases.GetTenantDatabases.Execute().ToValueOrProblemDetails();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTenantDatabasesDetails([FromRoute] Guid id)
    {
        return await  tenantDatabaseUseCases.GetTenantDatabaseDetails.Execute(id).ToValueOrProblemDetails();
    }


    [HttpPost]
    public async Task<IActionResult> CreateTenant(
        [FromBody] CreateTenantRequest request)
    {
        return await tenantUseCases.CreateTenant.ExecuteAsync(request).ToValueOrProblemDetails();
    }
    
}
