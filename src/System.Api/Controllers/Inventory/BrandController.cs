using System.Api.Result;
using Common.Contracts.authentication;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Inventory.Application.UseCases.Brands;
using Module.Inventory.Application.UseCases.Brands.CreateBrand;
using Module.Inventory.Application.UseCases.Brands.GetBrands;
using Module.Inventory.Application.UseCases.Brands.Update;

namespace System.Api.Controllers.Inventory
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Inventory | Brands")]
    [Authorize]
    public class BrandController(BrandUseCases service, ICurrentUser currentUser) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateBrand([FromBody] CreateBrandRequest dto)
        {
            return await service.CreateBrand.Execute(currentUser.ToActorContext(), dto).ToValueOrProblemDetails();
        }

        [HttpGet]
        public async Task<IActionResult> GetBrands([FromQuery] bool? includeInactive)
        {
            return await  service.GetBrands.Execute(includeInactive).ToValueOrProblemDetails();
        }

        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> ToggleBrandStatus([FromRoute] Guid id)
        {
            return await service.UpdateBrand.ChangeStatus(currentUser.ToActorContext(), id).ToValueOrProblemDetails();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateBrand([FromRoute] Guid id, [FromBody] UpdateBrandDto dto)
        {
            return await service.UpdateBrand.Execute(currentUser.ToActorContext(), id, dto).ToValueOrProblemDetails();
        }
    }
}
