using System.Api.Attributes;
using System.Api.Result;
using Common.Contracts.authentication;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Auth.Application.Abstraction;
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
    public class BrandController(BrandUseCases service, ISessionStateService currentUser) : ControllerBase
    {
        [HttpPost]
        [RequireFeature("products", "create")]
        public async Task<IActionResult> CreateBrand([FromBody] CreateBrandRequest dto)
        
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)
                return actorResult.ToValueOrProblemDetails();
            return await service.CreateBrand.Execute(actorResult.Value, dto).ToValueOrProblemDetails();
        }

        [HttpGet]
        [RequireFeature("products", "read")]
        public async Task<IActionResult> GetBrands([FromQuery] bool? includeInactive)
        {
            return await  service.GetBrands.Execute(includeInactive).ToValueOrProblemDetails();
        }

        [HttpPatch("{id:guid}/status")]
        [RequireFeature("products", "update")]
        public async Task<IActionResult> ToggleBrandStatus([FromRoute] Guid id)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)
                return actorResult.ToValueOrProblemDetails();
            return await service.UpdateBrand.ChangeStatus(actorResult.Value, id).ToValueOrProblemDetails();
        }

        [HttpPut("{id:guid}")]
        [RequireFeature("products", "update")]
        public async Task<IActionResult> UpdateBrand([FromRoute] Guid id, [FromBody] UpdateBrandDto dto)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)
                return actorResult.ToValueOrProblemDetails();
            return await service.UpdateBrand.Execute(actorResult.Value, id, dto).ToValueOrProblemDetails();
        }
    }
}
