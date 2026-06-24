using System.Api.Result;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Inventory.Application.UseCases.Brands;
using Module.Inventory.Application.UseCases.Brands.CreateBrand;
using Module.Inventory.Application.UseCases.Brands.GetBrands;

namespace System.Api.Controllers.Inventory
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Inventory | Brands")]
    [Authorize]
    public class BrandController(BrandUseCases service) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateBrand([FromBody] CreateBrandRequest dto)
        {
            return await service.CreateBrand.Execute(dto).ToValueOrProblemDetails();
        }

        [HttpGet]
        public async Task<IActionResult> GetBrands([FromQuery]  QueryBrandDto query)
        {
            return await  service.GetBrands.Execute(query).ToValueOrProblemDetails();
        }
    }
}
