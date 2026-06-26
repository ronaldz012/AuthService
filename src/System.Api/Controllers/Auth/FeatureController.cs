using System.Api.Filters;
using System.Api.Result;
using Microsoft.AspNetCore.Mvc;
using Module.Auth.Application.UseCases.Features;

namespace System.Api.Controllers.Auth
{
    [Route("api/system/[controller]")]
    [ApiController]
    [Tags("Admin | Features")]
    [ApiKey]
    public class FeatureController(FeatureUseCases featureUseCases) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> AddFeature([FromBody] CreateFeatureDto dto)
        {
            return await featureUseCases.CreateFeature.Execute(dto)
            .ToValueOrProblemDetails();
        }

        [HttpGet("{key}")]
        public async Task<IActionResult> GetFeature(string key)
        {
            return await featureUseCases.GetFeature.Execute(key)
            .ToValueOrProblemDetails();
        }
        [HttpGet]
        public async Task<IActionResult> GetAllFeatures([FromQuery] FeatureQueryDto queryDto)
        {
            return await featureUseCases.ListFeatures.Execute(queryDto)
            .ToValueOrProblemDetails();
        }
    }
}
