using System.Api.Attributes;
using System.Api.Filters;
using System.Api.Result;
using Inventory.Contracts.Dtos.Products;
using Inventory.UseCases.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace System.Api.Controllers.Inventory
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Inventory | Products")]
    [Authorize]
    public class ProductController(ProductUseCases productUseCases) : ControllerBase
    {
        [HttpPost]
        [RequireFeature("inventory", "read")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto request)
        {
            return await productUseCases.CreateProduct.Execute(request).ToValueOrProblemDetails();
        }

        [HttpGet]
        [RequireBranch]
        public async Task<IActionResult> GetProducts([FromQuery] ProductQueryDto request)
        {
            return await productUseCases.ListProducts.Execute(request).ToValueOrProblemDetails();
        }

        [HttpGet("Search")]
        public async Task<IActionResult> SearchProduct([FromQuery] string request)
        {
            return await productUseCases.SearchProducts.Execute(request).ToValueOrProblemDetails();
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetProduct([FromRoute] int id)
        {
            return await productUseCases.ProductDetails.Execute(id).ToValueOrProblemDetails();
        }

        [HttpGet("productVariant")]
        [Authorize]
        public async Task<IActionResult> GetVariantProductByCode([FromQuery] string request)
        {
            return await productUseCases.GetProductByCode.Execute(request).ToValueOrProblemDetails();
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductDto request, [FromRoute] int id)
        {
            return await productUseCases.UpdateProduct.Execute(request, id).ToValueOrProblemDetails();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct([FromRoute] int id)
        {
            return await productUseCases.DeleteProduct.Execute(id).ToValueOrProblemDetails();
        }
    }
}
