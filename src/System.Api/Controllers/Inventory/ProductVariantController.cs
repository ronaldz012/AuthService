using System.Api.Result;
using Inventory.Contracts.Dtos.ProductVariants;
using Inventory.UseCases.ProductVariants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace System.Api.Controllers.Inventory
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Inventory | ProductVariants")]
    [Authorize]
    public class ProductVariantController(ProductVariantUseCases useCases) : ControllerBase
    {
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateProductVariant([FromRoute] Guid id,[FromBody]UpdateProductVariantDto dto)
        {
            return await useCases.UpdateProductVariant.Execute(dto, id).ToValueOrProblemDetails();
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> UpdateStock([FromRoute] Guid id, [FromBody] UpdateProductVariantStockDto dto)
        {
            return await useCases.CorrectProductVariantStock.Execute(dto, id).ToValueOrProblemDetails();
        }
        [HttpGet]
        public async Task<IActionResult> GetVariantProductByCode([FromQuery] string request)
        {
            return await useCases.GetProductVariantByCode.Execute(request).ToValueOrProblemDetails();
        }
        [HttpPost("{productId:guid}")]
        public async Task<IActionResult> CreateProductVariants([FromRoute]Guid productId, [FromBody] List<CreateProductVariantDto> variants)
        {
            return await useCases.CreateProductVariantUc.Execute(productId, variants).ToValueOrProblemDetails();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteProductVariant([FromRoute] Guid id)
        {
            return await useCases.DeleteProductVariantUc.Execute(id).ToValueOrProblemDetails();
        }

    }
}
