using System.Api.Attributes;
using System.Api.Result;
using Common.Contracts.authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module.Inventory.Application.UseCases.Products.Create;
using Module.Inventory.Application.UseCases.ProductVariants;
using Module.Inventory.Application.UseCases.StockMovements;
using Module.Inventory.Application.UseCases.StockMovements.Get;
using Module.Inventory.Application.UseCases.ProductVariants.Create;
using Module.Inventory.Application.UseCases.ProductVariants.PatchStock;
using Module.Inventory.Application.UseCases.ProductVariants.Update;

namespace System.Api.Controllers.Inventory
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Inventory | ProductVariants")]
    [Authorize]
    public class ProductVariantController(ProductVariantUseCases useCases, StockMovementUseCases stockMovementUseCases, ICurrentUser currentUser) : ControllerBase
    {
        [HttpPut("{id:guid}")]
        [RequireFeature("products", "update")]
        public async Task<IActionResult> UpdateProductVariant([FromRoute] Guid id,[FromBody]UpdateProductVariantDto dto)
        {
            return await useCases.UpdateProductVariant.Execute(currentUser.ToActorContext(), dto, id).ToValueOrProblemDetails();
        }

        [HttpPatch("{id:guid}")]
        [RequireFeature("products", "update")]
        public async Task<IActionResult> UpdateStock([FromRoute] Guid id, [FromBody] UpdateProductVariantStockDto dto)
        {
            return await useCases.CorrectProductVariantStock.Execute(currentUser.ToActorContext(), dto, id).ToValueOrProblemDetails();
        }
        [HttpGet]
        public async Task<IActionResult> GetVariantProductByCode([FromQuery] string request)
        {
            return await useCases.GetProductVariantByCode.Execute(currentUser.ToActorContext(), request).ToValueOrProblemDetails();
        }

        [HttpGet("{id:guid}/details")]
        [RequireFeature("products", "read")]
        public async Task<IActionResult> GeProductVariantById([FromRoute] Guid id)
        {
            return await useCases.GetProductVariantDetails.Execute(currentUser.ToActorContext(), id).ToValueOrProblemDetails();
        }
        [HttpPost("{productId:guid}")]
        [RequireFeature("products", "update")]
        public async Task<IActionResult> CreateProductVariants([FromRoute]Guid productId, [FromBody] CreateProductVariantsRequest request)
        {
            return await useCases.CreateProductVariantUc.Execute(currentUser.ToActorContext(), productId, request).ToValueOrProblemDetails();
        }

        [HttpGet("{id:guid}/can-delete")]
        [RequireFeature("products", "delete")]
        public async Task<IActionResult> CanDeleteVariant([FromRoute] Guid id)
        {
            return await useCases.DeleteProductVariantUc.Check(currentUser.ToActorContext(), id).ToValueOrProblemDetails();
        }

        [HttpDelete("{id:guid}")]
        [RequireFeature("products", "delete")]
        public async Task<IActionResult> DeleteProductVariant([FromRoute] Guid id)
        {
            return await useCases.DeleteProductVariantUc.Execute(currentUser.ToActorContext(), id).ToValueOrProblemDetails();
        }

        [HttpGet("{id:guid}/movements")]
        [RequireFeature("products", "read")]
        public async Task<IActionResult> GetProductVariantMovements([FromRoute] Guid id, [FromQuery] StockMovementQueryDto query)
        {
            return await stockMovementUseCases.ListStockMovements.Execute(currentUser.ToActorContext(), id, query).ToValueOrProblemDetails();
        }
    }
}
