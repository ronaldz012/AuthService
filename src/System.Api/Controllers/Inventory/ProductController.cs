using System.Api.Attributes;
using System.Api.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Inventory.Application.UseCases.Products;
using Module.Inventory.Application.UseCases.Products.Create;
using Module.Inventory.Application.UseCases.Products.Get;
using Module.Inventory.Application.UseCases.Products.Update;
using Module.Inventory.Application.UseCases.Products.UpdateStatus;

namespace System.Api.Controllers.Inventory
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Inventory | Products")]
    [Authorize]
    public class ProductController(ProductUseCases productUseCases) : ControllerBase
    {
        [HttpPost]
        [RequireFeature("products", "create")]
        public async Task<IActionResult> CreateProduct([FromBody]  CreateProductRequest request)
        {
            return await productUseCases.CreateProductUc.Execute(request).ToValueOrProblemDetails();
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] ProductQueryDto request)
        {
            return await productUseCases.GetProductsUc.Execute(request).ToValueOrProblemDetails();
        }

        [HttpGet("Search")]
        public async Task<IActionResult> SearchProduct([FromQuery] string request, [FromQuery] bool? includeInactive)
        {
            return await productUseCases.SearchProducts.Execute(request, includeInactive).ToValueOrProblemDetails();
        }

        [HttpGet("{id:guid}")]
        [RequireFeature("products", "read", true)]
        public async Task<IActionResult> GetProduct([FromRoute] Guid id)
        {
            return await productUseCases.ProductDetails.Execute(id).ToValueOrProblemDetails();
        }
        

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductDto request, [FromRoute] Guid id)
        {
            return await productUseCases.UpdateProduct.Execute(request, id).ToValueOrProblemDetails();
        }

        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> UpdateProductStatus([FromRoute] Guid id, [FromBody] UpdateProductStatusDto request)
        {
            return await productUseCases.UpdateProductStatus.Execute(id, request).ToValueOrProblemDetails();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteProduct([FromRoute] Guid id)
        {
            return await productUseCases.DeleteProduct.Execute(id).ToValueOrProblemDetails();
        }
    }
}
