// using System.Api.Filters;
// using System.Api.Result;
// using Common.Services;
// using Microsoft.AspNetCore.Mvc;
// using Module.Auth.dtos;
// using Module.Auth.UseCases;
//
// namespace System.Api.Controllers.System;
// [ApiController]
// [Route("api/system/[controller]")]
// [ApiKey]
// [Tags("Admin | System")]
// public class TenantController(TenantService service) : ControllerBase
// {
//     [HttpPost]
//     public async Task<IActionResult> CreateNewTenant([FromBody]CreateTenantDto tenant)
//     {
//         return await service.CreateTenantAsync(tenant).ToValueOrProblemDetails();
//     }
// }