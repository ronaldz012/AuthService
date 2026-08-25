using Common.Contracts.authentication;
using Common.Contracts.inventory;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Sales.Application.Abstraction;
using Module.Sales.Application.UseCases.Sales.Get;
using Module.Sales.Domain;
using Moq;

namespace Test.Sales;

public class GetListSalesTests
{
    private static readonly Guid TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid BranchId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid CashClosureId = Guid.NewGuid();
    private static readonly Guid SaleId = Guid.NewGuid();
    private static readonly Guid ReturnId = Guid.NewGuid();

    [Fact]
    public async Task Execute_ShouldReturnSales_WhenNoFilter()
    {
        var tenantCtx = TestSalesDbContextFactory.CreateTenantContext(TenantId);
        using var dbContext = TestSalesDbContextFactory.Create(tenantCtx);
        SeedSaleWithReturn(dbContext);

        var sut = new GetListSales(dbContext);

        var result = await sut.Execute(CreateActorContext(), new SalesQueryDto { Type = SaleType.Sale });

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal(SaleType.Sale, result.Value.Items.First().Type);
    }

    [Fact]
    public async Task Execute_ShouldFilterSalesWithReturn_WhenHasReturnTrue()
    {
        var tenantCtx = TestSalesDbContextFactory.CreateTenantContext(TenantId);
        using var dbContext = TestSalesDbContextFactory.Create(tenantCtx);
        SeedSaleWithReturn(dbContext);

        var sut = new GetListSales(dbContext);

        var result = await sut.Execute(CreateActorContext(), new SalesQueryDto { Type = SaleType.Sale, HasReturn = true });

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.True(result.Value.Items.First().HasReturn);
        Assert.True(result.Value.Items.First().ReturnedAmount < 0);
    }

    [Fact]
    public async Task Execute_ShouldFilterSalesWithoutReturn_WhenHasReturnFalse()
    {
        var tenantCtx = TestSalesDbContextFactory.CreateTenantContext(TenantId);
        using var dbContext = TestSalesDbContextFactory.Create(tenantCtx);
        SeedSaleWithoutReturn(dbContext);

        var sut = new GetListSales(dbContext);

        var result = await sut.Execute(CreateActorContext(), new SalesQueryDto { HasReturn = false });

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.False(result.Value.Items.First().HasReturn);
    }

    [Fact]
    public async Task Execute_ShouldExcludeReturns_WhenQueryingSales()
    {
        var tenantCtx = TestSalesDbContextFactory.CreateTenantContext(TenantId);
        using var dbContext = TestSalesDbContextFactory.Create(tenantCtx);
        SeedSaleWithReturn(dbContext);

        var sut = new GetListSales(dbContext);

        var result = await sut.Execute(CreateActorContext(), new SalesQueryDto { Type = SaleType.Sale });

        Assert.True(result.IsSuccess);
        // Only the Sale should be listed, not the Return
        Assert.Single(result.Value.Items);
        Assert.Equal(SaleId, result.Value.Items.First().Id);
    }

    private static ActorContext CreateActorContext()
        => new(TenantId, UserId, "Test User", BranchId, [BranchId]);

    private static void SeedSaleWithReturn(ISalesDbContext ctx)
    {
        ctx.Sales.Add(new Sale
        {
            Id = SaleId,
            BranchId = BranchId,
            SoldById = UserId,
            SoldByName = "Test User",
            CreatedBy = UserId,
            CreatedByName = "Test User",
            CashRegisterClosureId = CashClosureId,
            PaymentMethod = PaymentMethod.Cash,
            DocumentType = DocumentType.Ticket,
            Type = SaleType.Sale,
            TotalAmount = 100m,
            TenantId = TenantId,
            SaleItems =
            [
                new SaleItem
                {
                    ProductVariantId = Guid.NewGuid(),
                    ProductSku = "SKU-001",
                    ProductDisplayName = "Test Product",
                    Quantity = 2,
                    UnitPrice = 50m,
                    UnitCost = 30m,
                    DiscountAmount = 0,
                    FinalPrice = 100m,
                    TenantId = TenantId
                }
            ]
        });

        ctx.Sales.Add(new Sale
        {
            Id = ReturnId,
            BranchId = BranchId,
            SoldById = UserId,
            SoldByName = "Test User",
            CreatedBy = UserId,
            CreatedByName = "Test User",
            CashRegisterClosureId = CashClosureId,
            PaymentMethod = PaymentMethod.Cash,
            DocumentType = DocumentType.Ticket,
            Type = SaleType.Return,
            OriginalSaleId = SaleId,
            TotalAmount = -50m,
            TenantId = TenantId,
            SaleItems =
            [
                new SaleItem
                {
                    ProductVariantId = Guid.NewGuid(),
                    ProductSku = "SKU-001",
                    ProductDisplayName = "Test Product",
                    Quantity = -1,
                    UnitPrice = 50m,
                    UnitCost = 30m,
                    DiscountAmount = 0,
                    FinalPrice = -50m,
                    OriginalSaleItemId = null,
                    TenantId = TenantId
                }
            ]
        });

        ctx.SaveChangesAsync().GetAwaiter().GetResult();
    }

    private static void SeedSaleWithoutReturn(ISalesDbContext ctx)
    {
        ctx.Sales.Add(new Sale
        {
            Id = SaleId,
            BranchId = BranchId,
            SoldById = UserId,
            SoldByName = "Test User",
            CreatedBy = UserId,
            CreatedByName = "Test User",
            CashRegisterClosureId = CashClosureId,
            PaymentMethod = PaymentMethod.Cash,
            DocumentType = DocumentType.Ticket,
            Type = SaleType.Sale,
            TotalAmount = 100m,
            TenantId = TenantId,
            SaleItems =
            [
                new SaleItem
                {
                    ProductVariantId = Guid.NewGuid(),
                    ProductSku = "SKU-001",
                    ProductDisplayName = "Test Product",
                    Quantity = 2,
                    UnitPrice = 50m,
                    UnitCost = 30m,
                    DiscountAmount = 0,
                    FinalPrice = 100m,
                    TenantId = TenantId
                }
            ]
        });

        ctx.SaveChangesAsync().GetAwaiter().GetResult();
    }
}
