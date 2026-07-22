using System.Api.Result; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Inventory.Application.UseCases.Categories;
using Module.Inventory.Application.UseCases.Categories.Create;
using Module.Inventory.Application.UseCases.Categories.Get;

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
        public async Task<IActionResult> GetCategories()
        {
            return await categoryUseCases.GetCategories.Execute().ToValueOrProblemDetails();
        }
    }
}
