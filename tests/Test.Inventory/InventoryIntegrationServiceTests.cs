using Common.Contracts.authentication;
using Common.Contracts.inventory;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Module.Inventory.Domain.Inventory;
using Module.Inventory.Domain.Products;
using Module.Inventory.Infrastructure;
using System.Infrastructure.Persistence;

namespace Test.Inventory;

public class InventoryIntegrationServiceTests
{
    private static readonly Guid TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid BranchId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid VariantId = Guid.NewGuid();
    private static readonly Guid ReferenceId = Guid.NewGuid();

    private static TestAppDbContext CreateDbContext()
    {
        var tenantCtx = new TestTenantConnectionContext
        {
            TenantId = TenantId,
            Schema = "test_schema",
            DatabaseName = "test_db"
        };
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"InvTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new TestAppDbContext(options, tenantCtx);
    }

    private static async Task SeedVariant(TestAppDbContext ctx, int stock)
    {
        var brand = new Brand { Id = Guid.NewGuid(), Name = "Nike", Prefix = "NIK", CreatedBy = TenantId, CreatedByName = "Test User" };
        var category = new Category { Id = Guid.NewGuid(), Name = "Zapatillas", CreatedBy = TenantId, CreatedByName = "Test User" };
        var color = new Color { Id = Guid.NewGuid(), Name = "Negro", CreatedBy = TenantId, CreatedByName = "Test User" };
        var size = new Size { Id = Guid.NewGuid(), Name = "42", SortOrder = 1, CreatedBy = TenantId, CreatedByName = "Test User" };
        ctx.Brands.Add(brand);
        ctx.Categories.Add(category);
        ctx.Colors.Add(color);
        ctx.Sizes.Add(size);

        var product = Product.Create("Air Max", "d", category.Id, brand.Id, Gender.Unisex, "NIK-1", TenantId, UserId, "Test User");
        ctx.Products.Add(product);
        await ctx.SaveChangesAsync();

        var variant = ProductVariant.Create(product.Id, color.Id, size.Id, 100m, "NIK-1-001", TenantId, UserId, "Test User");
        variant.Id = VariantId;
        ctx.ProductVariants.Add(variant);
        await ctx.SaveChangesAsync();

        ctx.BranchInventories.Add(new BranchInventory
        {
            ProductVariantId = VariantId,
            BranchId = BranchId,
            Stock = stock,
            CreatedBy = UserId,
            CreatedByName = "Test User"
        });
        await ctx.SaveChangesAsync();
    }

    private static InventoryIntegrationService CreateSut(TestAppDbContext ctx) => new(ctx);

    [Fact]
    public async Task DeductStock_ShouldDeductAndCreateSaleMovements()
    {
        using var ctx = CreateDbContext();
        await SeedVariant(ctx, 10);
        var sut = CreateSut(ctx);

        var result = await sut.DeductStock(
            [new StockDeductionDto(VariantId, 3)],
            BranchId,
            UserId,
            "Test User",
            ReferenceId);

        Assert.True(result.IsSuccess);

        await ctx.SaveChangesAsync();

        var inventory = await ctx.BranchInventories.SingleAsync(bi => bi.ProductVariantId == VariantId);
        Assert.Equal(7, inventory.Stock);

        var movement = await ctx.StockMovements.SingleAsync(m => m.ReferenceId == ReferenceId);
        Assert.Equal(MovementType.Sale, movement.MovementType);
        Assert.Equal(-3m, movement.Quantity);
        Assert.Equal(BranchId, movement.BranchId);
    }

    [Fact]
    public async Task DeductStock_ShouldReturnError_WhenInsufficientStock()
    {
        using var ctx = CreateDbContext();
        await SeedVariant(ctx, 2);
        var sut = CreateSut(ctx);

        var result = await sut.DeductStock(
            [new StockDeductionDto(VariantId, 5)],
            BranchId,
            UserId,
            "Test User",
            ReferenceId);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.InvalidState, result.Error.Code);

        await ctx.SaveChangesAsync();
        Assert.Empty(await ctx.StockMovements.ToListAsync());
        var inventory = await ctx.BranchInventories.SingleAsync(bi => bi.ProductVariantId == VariantId);
        Assert.Equal(2, inventory.Stock);
    }

    [Fact]
    public async Task DeductStock_ShouldReturnError_WhenVariantNotFound()
    {
        using var ctx = CreateDbContext();
        var sut = CreateSut(ctx);

        var result = await sut.DeductStock(
            [new StockDeductionDto(Guid.NewGuid(), 1)],
            BranchId,
            UserId,
            "Test User",
            ReferenceId);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.NotFound, result.Error.Code);
    }
}