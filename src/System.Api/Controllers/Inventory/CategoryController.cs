using System.Api.Attributes;
using System.Api.Result; 
using Common.Contracts.authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Auth.Application.Abstraction;
using Module.Inventory.Application.UseCases.Categories;
using Module.Inventory.Application.UseCases.Categories.Create;
using Module.Inventory.Application.UseCases.Categories.Get;
using Module.Inventory.Application.UseCases.Categories.Update;

namespace System.Api.Controllers.Inventory
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Inventory | Categories")]
    [Authorize]
    public class CategoryController(CategoryUseCases categoryUseCases, ISessionStateService currentUser) : ControllerBase
    {
        [HttpPost]
        [RequireFeature("products", "create")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest dto)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)
                return actorResult.ToValueOrProblemDetails();
            return await categoryUseCases.CreateCategory.Execute(actorResult.Value, dto).ToValueOrProblemDetails();
        }

        [HttpGet]
        [RequireFeature("products", "read")]
        public async Task<IActionResult> GetCategories([FromQuery] bool? includeInactive)
        {
            return await categoryUseCases.GetCategories.Execute(includeInactive).ToValueOrProblemDetails();
        }

        [HttpPatch("{id:guid}/status")]
        [RequireFeature("products", "update")]
        public async Task<IActionResult> ToggleCategoryStatus([FromRoute] Guid id)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)
                return actorResult.ToValueOrProblemDetails();
            return await categoryUseCases.UpdateCategory.ChangeStatus(actorResult.Value, id).ToValueOrProblemDetails();
        }

        [HttpPut("{id:guid}")]
        [RequireFeature("products", "update")]
        public async Task<IActionResult> UpdateCategory([FromRoute] Guid id, [FromBody] UpdateCategoryDto dto)
        {
            var actorResult = currentUser.GetActorContext();
            if (!actorResult.IsSuccess)
                return actorResult.ToValueOrProblemDetails();
            return await categoryUseCases.UpdateCategory.Execute(actorResult.Value, id, dto).ToValueOrProblemDetails();
        }
    }
}
