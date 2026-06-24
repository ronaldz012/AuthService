using Common.Contracts.authentication;
using Common.Utilities;
using Inventory.Contracts.interfaces;
using sales.Contracts.dtos;
using sales.Module.Data;
using sales.Module.Entities;

namespace sales.UseCases.UseCases;

public class CreateSale(SalesDbContext context, ICurrentUser currentUser, IInventoryIntegrationService inventoryService)
{
    public async Task<Result<bool>> Execute(CreateSaleDto dto)
    {
        var branchId = currentUser.BranchIds[0];
        var variantIds = dto.Items.Select(i => i.ProductVariantId).Distinct().ToList();

        var stockResult = await inventoryService.GetVariantsWithStock(variantIds, branchId);
        if (!stockResult.IsSuccess) return stockResult.Error;

        var cashClosure = await context.CashRegisterClosures.FindAsync(dto.CashRegisterClosureId);
        if (cashClosure == null || cashClosure.BranchId != branchId)
            return new Error("NOT_FOUND", "Cash closure not found or does not belong to this branch.");

        if (cashClosure.Status != CashRegisterClosureStatus.Open)
            return new Error("VALIDATION_ERROR", "The cash closure must be open to register sales.");

        var variants = stockResult.Value;
        if (variants.Count != variantIds.Count)
            return new Error("NOT_FOUND", "One or more products do not exist.");

        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var factoryItems = dto.Items.Select(itemDto =>
            {
                var variant = variants.First(v => v.Id == itemDto.ProductVariantId);
                return (itemDto.ProductVariantId, variant.Price, itemDto.Quantity, itemDto.DiscountAmount);
            }).ToList();

            Sale sale;
            if (dto.DocumentType == DocumentType.Ticket)
            {
                sale = Sale.CreateSaleWithTicket(
                    branchId,
                    currentUser.UserId,
                    dto.CashRegisterClosureId,
                    dto.PaymentMethod,
                    dto.TransactionCode,
                    dto.Notes,
                    factoryItems
                );
            }
            else
            {
                sale = Sale.CreateSaleWithInvoice(
                    branchId,
                    currentUser.UserId,
                    dto.CashRegisterClosureId,
                    dto.PaymentMethod,
                    dto.DocumentType, 
                    dto.InvoiceNumber,
                    dto.TransactionCode,
                    dto.Notes,
                    factoryItems
                );
            }

            var deductions = dto.Items
                .Select(i => new StockDeductionDto(i.ProductVariantId, i.Quantity))
                .ToList();

            var deductResult = await inventoryService.DeductStock(deductions, branchId, currentUser.UserId, sale.Id);
            if (!deductResult.IsSuccess) return deductResult.Error;

            context.Sales.Add(sale);
            await context.SaveChangesAsync(); 

            await transaction.CommitAsync();
            return true;
        }
        catch (InvalidOperationException ex)
        {
            await transaction.RollbackAsync();
            return new Error("VALIDATION_ERROR", ex.Message);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}