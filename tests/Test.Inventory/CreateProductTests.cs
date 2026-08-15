using System.Data.Common;
using System.Transactions;
using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Module.Inventory.Application.Abstraction;
using Module.Inventory.Application.UseCases.Products.Create;
using Module.Inventory.Domain.Products;
using System.Infrastructure.Persistence;

namespace Test.Inventory;

public class CreateProductTests
{
    private static readonly Guid TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid BrandId = Guid.NewGuid();
    private static readonly Guid CategoryId = Guid.NewGuid();
    private static readonly Guid ColorId = Guid.NewGuid();
    private static readonly Guid SizeId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    private static ActorContext CreateActorContext()
        => new(TenantId, UserId, "Test User", Guid.Empty, []);

    private static IProductCodeService CreateCodeService(string internalCode, string sku)
    {
        var mock = new Mock<IProductCodeService>();
        mock.Setup(s => s.ReserveBrandCounter(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(internalCode);
        mock.Setup(s => s.ReserveVariantCounter(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(sku);
        return mock.Object;
    }

    [Fact]
    public async Task Execute_ShouldReturnError_WhenBrandNotFound()
    {
        using var ctx = CreateDbContext();
        SeedCategory(ctx);
        var sut = new CreateProductUc(ctx, Mock.Of<IProductCodeService>());

        var result = await sut.Execute(CreateActorContext(), new CreateProductRequest
        {
            Name = "Test Product",
            BrandId = Guid.NewGuid(),
            CategoryId = CategoryId,
            Variants = []
        });

        Assert.Equal(CreateProductErrors.BrandNotFound, result.Error);
    }

    [Fact]
    public async Task Execute_ShouldReturnError_WhenColorsNotFound()
    {
        using var ctx = CreateDbContext();
        SeedBrand(ctx);
        SeedCategory(ctx);
        var sut = new CreateProductUc(ctx, Mock.Of<IProductCodeService>());

        var result = await sut.Execute(CreateActorContext(), new CreateProductRequest
        {
            Name = "Test Product",
            BrandId = BrandId,
            CategoryId = CategoryId,
            Variants =
            [
                new CreateProductVariantForProductDto { ColorId = Guid.NewGuid(), SizeId = SizeId, Price = 100 }
            ]
        });

        Assert.Equal(CreateProductErrors.ColorsNotFound, result.Error);
    }

    [Fact]
    public async Task Execute_ShouldCreateProductWithVariants_WhenValidRequest()
    {
        using var ctx = CreateDbContext();
        SeedBrand(ctx);
        SeedCategory(ctx);
        SeedColor(ctx);
        SeedSize(ctx);
        var codeService = CreateCodeService("BRD-1", "BRD-1-001");
        var sut = new CreateProductUc(ctx, codeService);

        var result = await sut.Execute(CreateActorContext(), new CreateProductRequest
        {
            Name = "Test Product",
            Description = "A test product",
            BrandId = BrandId,
            CategoryId = CategoryId,
            Gender = Gender.Unisex,
            Variants =
            [
                new CreateProductVariantForProductDto
                {
                    ColorId = ColorId,
                    SizeId = SizeId,
                    Price = 99.99m,
                    Description = "Medium variant"
                }
            ]
        });

        Assert.True(result.IsSuccess, $"Expected success but got: {result.Error?.Code} - {result.Error?.Message}");
        Assert.NotNull(result.Value);
        Assert.Equal("Test Product", result.Value.Name);
        Assert.Equal("BRD-1", result.Value.InternalCode);
        Assert.Single(result.Value.Variants);
        Assert.Equal("BRD-1-001", result.Value.Variants[0].Sku);
    }

    [Fact]
    public async Task Execute_ShouldReturnError_WhenProductNameAlreadyExistsForCategoryAndBrand()
    {
        using var ctx = CreateDbContext();
        SeedBrand(ctx);
        SeedCategory(ctx);
        SeedColor(ctx);
        SeedSize(ctx);
        var codeService = CreateCodeService("BRD-1", "BRD-1-001");
        var sut = new CreateProductUc(ctx, codeService);

        var first = await sut.Execute(CreateActorContext(), new CreateProductRequest
        {
            Name = "Test Product",
            BrandId = BrandId,
            CategoryId = CategoryId,
            Variants =
            [
                new CreateProductVariantForProductDto { ColorId = ColorId, SizeId = SizeId, Price = 100 }
            ]
        });
        Assert.True(first.IsSuccess, $"Expected success but got: {first.Error?.Code} - {first.Error?.Message}");

        var second = await sut.Execute(CreateActorContext(), new CreateProductRequest
        {
            Name = "test product",
            BrandId = BrandId,
            CategoryId = CategoryId,
            Variants =
            [
                new CreateProductVariantForProductDto { ColorId = ColorId, SizeId = SizeId, Price = 100 }
            ]
        });

        Assert.False(second.IsSuccess);
        Assert.Equal(CreateProductErrors.ProductNameAlreadyExists, second.Error);
    }

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

    private static void SeedBrand(TestAppDbContext ctx)
    {
        ctx.Brands.Add(new Brand { Id = BrandId, Name = "Test Brand", Prefix = "BRD", ProductCounter = 0, CreatedBy = TenantId, CreatedByName = "Test User" });
        ctx.SaveChangesAsync().GetAwaiter().GetResult();
    }

    private static void SeedCategory(TestAppDbContext ctx)
    {
        ctx.Categories.Add(new Category { Id = CategoryId, Name = "Test Category", CreatedBy = TenantId, CreatedByName = "Test User" });
        ctx.SaveChangesAsync().GetAwaiter().GetResult();
    }

    private static void SeedColor(TestAppDbContext ctx)
    {
        ctx.Colors.Add(new Color { Id = ColorId, Name = "Red", CreatedBy = TenantId, CreatedByName = "Test User" });
        ctx.SaveChangesAsync().GetAwaiter().GetResult();
    }

    private static void SeedSize(TestAppDbContext ctx)
    {
        ctx.Sizes.Add(new Size { Id = SizeId, Name = "M", SortOrder = 1, CreatedBy = TenantId, CreatedByName = "Test User" });
        ctx.SaveChangesAsync().GetAwaiter().GetResult();
    }
}

public class TestTenantConnectionContext : ITenantConnectionContext
{
    public string? Schema { get; set; }
    public Guid? TenantId { get; set; }
    public string? DatabaseName { get; set; }
    public DbConnection Connection => throw new NotSupportedException("InMemory tests do not support Connection.");
    public Task EnsureOpenAsync() => Task.CompletedTask;
    public Task<TransactionScope> BeginTransactionScopeAsync() =>
        Task.FromResult(new TransactionScope(TransactionScopeOption.Suppress));
}

public class TestAppDbContext(DbContextOptions<AppDbContext> options, ITenantConnectionContext tenant)
    : AppDbContext(options, tenant);
