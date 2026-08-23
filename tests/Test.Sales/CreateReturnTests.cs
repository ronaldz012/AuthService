using Common.Contracts.authentication;
using Common.Contracts.inventory;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Sales.Application.Abstraction;
using Module.Sales.Application.UseCases.Sales.Return;
using Module.Sales.Domain;
using Moq;

namespace Test.Sales;

public class CreateReturnTests
{
    private static readonly Guid TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid BranchId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid VariantId = Guid.NewGuid();
    private static readonly Guid CashClosureId = Guid.NewGuid();
    private static readonly Guid OriginalSaleId = Guid.NewGuid();
    private static readonly Guid OriginalSaleItemId = Guid.NewGuid();

    [Fact]
    public async Task Execute_ShouldReturnError_WhenOriginalSaleNotFound()
    {
        var tenantCtx = TestSalesDbContextFactory.CreateTenantContext(TenantId);
        using var dbContext = TestSalesDbContextFactory.Create(tenantCtx);

        var sut = CreateSut(dbContext);

        var result = await sut.Execute(CreateActorContext(), new CreateReturnDto
        {
            OriginalSaleId = OriginalSaleId,
            Items = [new CreateReturnItemDto { OriginalSaleItemId = OriginalSaleItemId, Quantity = 1 }]
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ReturnErrors.OriginalSaleNotFound, result.Error);
    }

    [Fact]
    public async Task Execute_ShouldReturnError_WhenSaleIsNotSaleType()
    {
        var tenantCtx = TestSalesDbContextFactory.CreateTenantContext(TenantId);
        using var dbContext = TestSalesDbContextFactory.Create(tenantCtx);
        SeedOriginalSale(dbContext, SaleType.Return);

        var sut = CreateSut(dbContext);

        var result = await sut.Execute(CreateActorContext(), new CreateReturnDto
        {
            OriginalSaleId = OriginalSaleId,
            Items = [new CreateReturnItemDto { OriginalSaleItemId = OriginalSaleItemId, Quantity = 1 }]
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ReturnErrors.OriginalSaleNotEligible, result.Error);
    }

    [Fact]
    public async Task Execute_ShouldReturnError_WhenAlreadyReturned()
    {
        var tenantCtx = TestSalesDbContextFactory.CreateTenantContext(TenantId);
        using var dbContext = TestSalesDbContextFactory.Create(tenantCtx);
        SeedOriginalSale(dbContext, SaleType.Sale);
        SeedExistingReturn(dbContext);

        var sut = CreateSut(dbContext);

        var result = await sut.Execute(CreateActorContext(), new CreateReturnDto
        {
            OriginalSaleId = OriginalSaleId,
            Items = [new CreateReturnItemDto { OriginalSaleItemId = OriginalSaleItemId, Quantity = 1 }]
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ReturnErrors.AlreadyReturned, result.Error);
    }

    [Fact]
    public async Task Execute_ShouldReturnError_WhenItemNotFoundInOriginalSale()
    {
        var tenantCtx = TestSalesDbContextFactory.CreateTenantContext(TenantId);
        using var dbContext = TestSalesDbContextFactory.Create(tenantCtx);
        SeedOriginalSale(dbContext, SaleType.Sale);

        var sut = CreateSut(dbContext);

        var result = await sut.Execute(CreateActorContext(), new CreateReturnDto
        {
            OriginalSaleId = OriginalSaleId,
            Items = [new CreateReturnItemDto { OriginalSaleItemId = Guid.NewGuid(), Quantity = 1 }]
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ReturnErrors.OriginalItemNotFound, result.Error);
    }

    [Fact]
    public async Task Execute_ShouldReturnError_WhenQuantityExceedsSoldQuantity()
    {
        var tenantCtx = TestSalesDbContextFactory.CreateTenantContext(TenantId);
        using var dbContext = TestSalesDbContextFactory.Create(tenantCtx);
        SeedOriginalSale(dbContext, SaleType.Sale);

        var sut = CreateSut(dbContext);

        var result = await sut.Execute(CreateActorContext(), new CreateReturnDto
        {
            OriginalSaleId = OriginalSaleId,
            Items = [new CreateReturnItemDto { OriginalSaleItemId = OriginalSaleItemId, Quantity = 10 }]
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ReturnErrors.ExceedsReturnableQuantity, result.Error);
    }

    [Fact]
    public async Task Execute_ShouldReturnError_WhenNoOpenCashRegister()
    {
        var tenantCtx = TestSalesDbContextFactory.CreateTenantContext(TenantId);
        using var dbContext = TestSalesDbContextFactory.Create(tenantCtx);
        SeedOriginalSale(dbContext, SaleType.Sale);

        var sut = CreateSut(dbContext);

        var result = await sut.Execute(CreateActorContext(), new CreateReturnDto
        {
            OriginalSaleId = OriginalSaleId,
            Items = [new CreateReturnItemDto { OriginalSaleItemId = OriginalSaleItemId, Quantity = 1 }]
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ReturnErrors.NoOpenCashRegister, result.Error);
    }

    [Fact]
    public async Task Execute_ShouldReturnError_WhenReturnStockFails()
    {
        var tenantCtx = TestSalesDbContextFactory.CreateTenantContext(TenantId);
        using var dbContext = TestSalesDbContextFactory.Create(tenantCtx);
        SeedOriginalSale(dbContext, SaleType.Sale);
        SeedOpenCashClosure(dbContext);

        var inventoryMock = new Mock<IInventoryIntegrationService>();
        inventoryMock
            .Setup(s => s.ReturnStock(It.IsAny<List<StockReturnDto>>(), BranchId, UserId, It.IsAny<string>(), It.IsAny<Guid>(), false))
            .ReturnsAsync(new Error(ErrorCode.NotFound, "Product variant not found."));

        var sut = CreateSut(dbContext, inventoryMock.Object);

        var result = await sut.Execute(CreateActorContext(), new CreateReturnDto
        {
            OriginalSaleId = OriginalSaleId,
            Items = [new CreateReturnItemDto { OriginalSaleItemId = OriginalSaleItemId, Quantity = 1 }]
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.NotFound, result.Error.Code);
    }

    [Fact]
    public async Task Execute_ShouldCreateReturn_WhenValidRequest()
    {
        var tenantCtx = TestSalesDbContextFactory.CreateTenantContext(TenantId);
        using var dbContext = TestSalesDbContextFactory.Create(tenantCtx);
        SeedOriginalSale(dbContext, SaleType.Sale);
        SeedOpenCashClosure(dbContext);

        var inventoryMock = new Mock<IInventoryIntegrationService>();
        inventoryMock
            .Setup(s => s.ReturnStock(It.IsAny<List<StockReturnDto>>(), BranchId, UserId, It.IsAny<string>(), It.IsAny<Guid>(), false))
            .ReturnsAsync(true);

        var sut = CreateSut(dbContext, inventoryMock.Object);

        var result = await sut.Execute(CreateActorContext(), new CreateReturnDto
        {
            OriginalSaleId = OriginalSaleId,
            Items = [new CreateReturnItemDto { OriginalSaleItemId = OriginalSaleItemId, Quantity = 2 }]
        });

        Assert.True(result.IsSuccess, $"Expected success but got: {result.Error?.Code} - {result.Error?.Message}");

        var saved = await dbContext.Sales.FirstOrDefaultAsync(s => s.Type == SaleType.Return);
        Assert.NotNull(saved);
        Assert.Equal(BranchId, saved.BranchId);
        Assert.Equal(UserId, saved.SoldById);
        Assert.Equal(OriginalSaleId, saved.OriginalSaleId);
        Assert.Equal(SaleType.Return, saved.Type);
        Assert.Equal(TenantId, saved.TenantId);
        Assert.Single(saved.SaleItems);
        Assert.Equal(-2, saved.SaleItems.First().Quantity);
    }

    [Fact]
    public async Task Execute_ShouldReturnError_WhenSaleBelongsToDifferentTenant()
    {
        var otherTenantId = Guid.Parse("00000000-0000-0000-0000-000000000099");
        var tenantCtx = TestSalesDbContextFactory.CreateTenantContext(otherTenantId);
        using var dbContext = TestSalesDbContextFactory.Create(tenantCtx);
        SeedOriginalSale(dbContext, SaleType.Sale);

        var sut = CreateSut(dbContext);

        var result = await sut.Execute(CreateActorContext(), new CreateReturnDto
        {
            OriginalSaleId = OriginalSaleId,
            Items = [new CreateReturnItemDto { OriginalSaleItemId = OriginalSaleItemId, Quantity = 1 }]
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(ReturnErrors.OriginalSaleNotFound, result.Error);
    }

    private static ActorContext CreateActorContext()
        => new(TenantId, UserId, "Test User", BranchId, [BranchId]);

    private static CreateReturn CreateSut(
        ISalesDbContext dbContext,
        IInventoryIntegrationService? inventoryService = null)
    {
        return new CreateReturn(
            dbContext,
            inventoryService ?? new Mock<IInventoryIntegrationService>().Object,
            new Mock<ILogger<CreateReturn>>().Object);
    }

    private static void SeedOriginalSale(ISalesDbContext ctx, SaleType type)
    {
        ctx.Sales.Add(new Sale
        {
            Id = OriginalSaleId,
            BranchId = BranchId,
            SoldById = UserId,
            SoldByName = "Original Buyer",
            CreatedBy = UserId,
            CreatedByName = "Original Buyer",
            CashRegisterClosureId = CashClosureId,
            PaymentMethod = PaymentMethod.Cash,
            DocumentType = DocumentType.Ticket,
            Type = type,
            TotalAmount = 100m,
            TenantId = TenantId,
            SaleItems =
            [
                new SaleItem
                {
                    Id = OriginalSaleItemId,
                    ProductVariantId = VariantId,
                    ProductSku = "SKU-001",
                    ProductDisplayName = "Test Product - Negro / 42",
                    Quantity = 5,
                    UnitPrice = 100m,
                    UnitCost = 30m,
                    DiscountAmount = 0,
                    FinalPrice = 500m,
                    TenantId = TenantId
                }
            ]
        });
        ctx.SaveChangesAsync().GetAwaiter().GetResult();
    }

    private static void SeedExistingReturn(ISalesDbContext ctx)
    {
        ctx.Sales.Add(new Sale
        {
            Id = Guid.NewGuid(),
            BranchId = BranchId,
            SoldById = UserId,
            SoldByName = "Original Buyer",
            CreatedBy = UserId,
            CreatedByName = "Original Buyer",
            CashRegisterClosureId = CashClosureId,
            PaymentMethod = PaymentMethod.Cash,
            DocumentType = DocumentType.Ticket,
            Type = SaleType.Return,
            OriginalSaleId = OriginalSaleId,
            TotalAmount = -50m,
            TenantId = TenantId
        });
        ctx.SaveChangesAsync().GetAwaiter().GetResult();
    }

    private static void SeedOpenCashClosure(ISalesDbContext ctx)
    {
        ctx.CashRegisterClosures.Add(new CashRegisterClosure
        {
            Id = CashClosureId,
            BranchId = BranchId,
            IsOpen = true,
            OpenAt = DateTime.UtcNow,
            OpeningBalance = 500m,
        });
        ctx.SaveChangesAsync().GetAwaiter().GetResult();
    }
}
