using Auth.Contracts.Interfaces;
using Inventory.Contracts.Dtos.Sales;
using Inventory.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using sales.Module.Data;
using Shared.Result;

namespace Inventory.UseCases.Sales;



public class GetSaleDetail(SalesDbContext context, ICurrentUser currentUser)
{
    public async Task<Result<SaleDetailDto>> Execute(int saleId)
    {
        var currentBranch = currentUser.BranchIds[0];

        // 1. Buscamos la venta proyectando directamente al DTO
        // Incluimos validación de sucursal por seguridad
        var saleDetail = await context.Sales
            .AsNoTracking()
            .Where(s => s.Id == saleId && s.BranchId == currentBranch)
            .Select(s => new SaleDetailDto
            {
                Id = s.Id,
                BranchId = s.BranchId,
                SoldById = s.SoldById,
                PaymentMethod = s.PaymentMethod,
                TransactionCode = s.TransactionCode,
                TotalAmount = s.TotalAmount,
                InvoiceNumber = s.InvoiceNumber,
                Status = s.Status,
                Notes = s.Notes,
                CreatedAt = s.CreatedAt,
                CancelledAt = s.CancelledAt,
                
                // Proyectamos los ítems y traemos info del ProductVariant
                Items = s.SaleItems.Select(si => new SaleItemDetailDto
                {
                    Id = si.Id,
                    ProductVariantId = si.ProductVariantId,
                    // Concatenamos información útil para la interfaz
                    ProductDisplayName = $"{si.ProductVariant.Product.Name} ({si.ProductVariant.Sku}) - {si.ProductVariant.Color} / {si.ProductVariant.Size}",
                    UnitPrice = si.UnitPrice,
                    DiscountAmount = si.DiscountAmount,
                    FinalPrice = si.FinalPrice
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (saleDetail == null)
        {
            return new Error("NOT_FOUND", "La venta no existe o no tiene permisos para verla.");
        }

        return saleDetail;
    }
}