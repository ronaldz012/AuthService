using System.Api.Result; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public class CategoryController(CategoryUseCases categoryUseCases) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest dto)
        {
            return await categoryUseCases.CreateCategory.Execute(dto).ToValueOrProblemDetails();
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories([FromQuery] bool? includeInactive)
        {
            return await categoryUseCases.GetCategories.Execute(includeInactive).ToValueOrProblemDetails();
        }

        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> ToggleCategoryStatus([FromRoute] Guid id)
        {
            return await categoryUseCases.UpdateCategory.ChangeStatus(id).ToValueOrProblemDetails();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateCategory([FromRoute] Guid id, [FromBody] UpdateCategoryDto dto)
        {
            return await categoryUseCases.UpdateCategory.Execute(id, dto).ToValueOrProblemDetails();
        }
    }
}
