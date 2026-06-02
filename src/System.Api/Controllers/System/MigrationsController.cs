using System.Api.Migration;
using Microsoft.AspNetCore.Mvc;
using System.Api.Filters;

namespace System.Api.Controllers.System;

[ApiController]
[Route("api/system/[controller]")]
[ApiKey]
[Tags("Admin | System")]
public class MigrationsController(MigrationService migrationService, TenantMigrationOrchestrator orchestrator) : ControllerBase
{
    // [HttpPost("update-tenant")]
    // public async Task<IActionResult> UpdateTenant([FromQuery] string schema)
    // {
    //     if (string.IsNullOrWhiteSpace(schema)) 
    //         return BadRequest("El nombre del esquema es obligatorio.");
    //
    //     try 
    //     {
    //         await migrationService.MigrateTenantAsync(schema);
    //         return Ok($"Esquema {schema} actualizado con éxito.");
    //     }
    //     catch (Exception ex)
    //     {
    //         return StatusCode(500, $"Error migrando {schema}: {ex.Message}");
    //     }
    // }

    [HttpPost("update-all-tenants")]
    public async Task<IActionResult> UpdateAllTenant([FromQuery] string schema)
    {
        if (string.IsNullOrWhiteSpace(schema))
        {
            return BadRequest("El nombre del esquema es obligatorio.");
        }

        // El orquestador debe estar inyectado vía constructor en el controlador
        var result = await orchestrator.MigrateAllAsync(schema);

        if (result.HasErrors)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}