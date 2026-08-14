using Common.Contracts.authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Module.Inventory.Application.UseCases.Brands.GetBrands;
using Module.Inventory.Application.UseCases.Brands.Update;
using Module.Inventory.Application.UseCases.Categories.Get;
using Module.Inventory.Application.UseCases.Categories.Update;
using Module.Inventory.Application.UseCases.Colors.List;
using Module.Inventory.Application.UseCases.Colors.Update;
using Module.Inventory.Application.UseCases.Sizes.List;
using Module.Inventory.Application.UseCases.Sizes.Update;
using Module.Inventory.Domain.Products;
using System.Infrastructure.Persistence;

namespace Test.Inventory;

public class CatalogStatusTests
{
    private static readonly Guid TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid BrandId = Guid.NewGuid();
    private static readonly Guid CategoryId = Guid.NewGuid();
    private static readonly Guid ColorId = Guid.NewGuid();
    private static readonly Guid SizeId = Guid.NewGuid();

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

    private static ICurrentUser CreateCurrentUser()
    {
        var mock = new Mock<ICurrentUser>();
        mock.Setup(u => u.UserId).Returns(UserId);
        mock.Setup(u => u.FullName).Returns("Test User");
        return mock.Object;
    }

    private static async Task SeedCatalog(TestAppDbContext ctx)
    {
        ctx.Brands.Add(new Brand { Id = BrandId, Name = "Nike", Prefix = "NIK", CreatedBy = TenantId, CreatedByName = "Test User" });
        ctx.Categories.Add(new Category { Id = CategoryId, Name = "Zapatillas", CreatedBy = TenantId, CreatedByName = "Test User" });
        ctx.Colors.Add(new Color { Id = ColorId, Name = "Negro", CreatedBy = TenantId, CreatedByName = "Test User" });
        ctx.Sizes.Add(new Size { Id = SizeId, Name = "42", SortOrder = 1, CreatedBy = TenantId, CreatedByName = "Test User" });
        await ctx.SaveChangesAsync();
    }

    [Fact]
    public async Task GetBrands_ShouldExcludeInactive_ByDefault()
    {
        using var ctx = CreateDbContext();
        await SeedCatalog(ctx);
        ctx.Brands.Add(new Brand { Id = Guid.NewGuid(), Name = "Old Brand", Prefix = "OLD", IsActive = false, CreatedBy = TenantId, CreatedByName = "Test User" });
        await ctx.SaveChangesAsync();

        var sut = new GetBrands(ctx);
        var result = await sut.Execute();

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(result.Value, b => !b.IsActive);
    }

    [Fact]
    public async Task GetBrands_ShouldIncludeInactive_WhenRequested()
    {
        using var ctx = CreateDbContext();
        await SeedCatalog(ctx);
        ctx.Brands.Add(new Brand { Id = Guid.NewGuid(), Name = "Old Brand", Prefix = "OLD", IsActive = false, CreatedBy = TenantId, CreatedByName = "Test User" });
        await ctx.SaveChangesAsync();

        var sut = new GetBrands(ctx);
        var result = await sut.Execute(includeInactive: true);

        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value, b => !b.IsActive);
    }

    [Fact]
    public async Task GetCategories_ShouldExcludeInactive_ByDefault()
    {
        using var ctx = CreateDbContext();
        await SeedCatalog(ctx);
        ctx.Categories.Add(new Category { Id = Guid.NewGuid(), Name = "Old Category", IsActive = false, CreatedBy = TenantId, CreatedByName = "Test User" });
        await ctx.SaveChangesAsync();

        var sut = new GetCategories(ctx);
        var result = await sut.Execute();

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(result.Value, c => !c.IsActive);
    }

    [Fact]
    public async Task GetCategories_ShouldIncludeInactive_WhenRequested()
    {
        using var ctx = CreateDbContext();
        await SeedCatalog(ctx);
        ctx.Categories.Add(new Category { Id = Guid.NewGuid(), Name = "Old Category", IsActive = false, CreatedBy = TenantId, CreatedByName = "Test User" });
        await ctx.SaveChangesAsync();

        var sut = new GetCategories(ctx);
        var result = await sut.Execute(includeInactive: true);

        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value, c => !c.IsActive);
    }

    [Fact]
    public async Task GetListColors_ShouldExcludeInactive_ByDefault()
    {
        using var ctx = CreateDbContext();
        await SeedCatalog(ctx);
        ctx.Colors.Add(new Color { Id = Guid.NewGuid(), Name = "Gris", IsActive = false, CreatedBy = TenantId, CreatedByName = "Test User" });
        await ctx.SaveChangesAsync();

        var sut = new GetListColors(ctx);
        var result = await sut.Execute();

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(result.Value, c => !c.IsActive);
    }

    [Fact]
    public async Task GetListColors_ShouldIncludeInactive_WhenRequested()
    {
        using var ctx = CreateDbContext();
        await SeedCatalog(ctx);
        ctx.Colors.Add(new Color { Id = Guid.NewGuid(), Name = "Gris", IsActive = false, CreatedBy = TenantId, CreatedByName = "Test User" });
        await ctx.SaveChangesAsync();

        var sut = new GetListColors(ctx);
        var result = await sut.Execute(includeInactive: true);

        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value, c => !c.IsActive);
    }

    [Fact]
    public async Task GetListSizes_ShouldExcludeInactive_ByDefault()
    {
        using var ctx = CreateDbContext();
        await SeedCatalog(ctx);
        ctx.Sizes.Add(new Size { Id = Guid.NewGuid(), Name = "XXL", SortOrder = 20, IsActive = false, CreatedBy = TenantId, CreatedByName = "Test User" });
        await ctx.SaveChangesAsync();

        var sut = new GetListSizes(ctx);
        var result = await sut.Execute();

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(result.Value, s => !s.IsActive);
    }

    [Fact]
    public async Task GetListSizes_ShouldIncludeInactive_WhenRequested()
    {
        using var ctx = CreateDbContext();
        await SeedCatalog(ctx);
        ctx.Sizes.Add(new Size { Id = Guid.NewGuid(), Name = "XXL", SortOrder = 20, IsActive = false, CreatedBy = TenantId, CreatedByName = "Test User" });
        await ctx.SaveChangesAsync();

        var sut = new GetListSizes(ctx);
        var result = await sut.Execute(includeInactive: true);

        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value, s => !s.IsActive);
    }

    [Fact]
    public async Task UpdateBrandStatus_ShouldToggleNewState()
    {
        using var ctx = CreateDbContext();
        await SeedCatalog(ctx);

        var sut = new UpdateBrand(ctx, CreateCurrentUser());
        var result = await sut.ChangeStatus(BrandId);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value);
        Assert.False((await ctx.Brands.FindAsync(BrandId))!.IsActive);

        var second = await sut.ChangeStatus(BrandId);
        Assert.True(second.IsSuccess);
        Assert.True(second.Value);
    }

    [Fact]
    public async Task UpdateCategoryStatus_ShouldToggleNewState()
    {
        using var ctx = CreateDbContext();
        await SeedCatalog(ctx);

        var sut = new UpdateCategory(ctx, CreateCurrentUser());
        var result = await sut.ChangeStatus(CategoryId);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value);
        Assert.False((await ctx.Categories.FindAsync(CategoryId))!.IsActive);
    }

    [Fact]
    public async Task UpdateColorStatus_ShouldToggleNewState()
    {
        using var ctx = CreateDbContext();
        await SeedCatalog(ctx);

        var sut = new UpdateColor(ctx, CreateCurrentUser());
        var result = await sut.ChangeStatus(ColorId);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value);
        Assert.False((await ctx.Colors.FindAsync(ColorId))!.IsActive);
    }

    [Fact]
    public async Task UpdateSizeStatus_ShouldToggleNewState()
    {
        using var ctx = CreateDbContext();
        await SeedCatalog(ctx);

        var sut = new UpdateSize(ctx, CreateCurrentUser());
        var result = await sut.ChangeStatus(SizeId);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value);
        Assert.False((await ctx.Sizes.FindAsync(SizeId))!.IsActive);
    }

    [Fact]
    public async Task UpdateBrandStatus_ShouldReturnNotFound_WhenMissing()
    {
        using var ctx = CreateDbContext();
        var sut = new UpdateBrand(ctx, CreateCurrentUser());

        var result = await sut.ChangeStatus(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(UpdateBrandErrors.BrandNotFound, result.Error);
    }

    [Fact]
    public async Task UpdateBrand_ShouldUpdateNameAndDescription_ButNotPrefix()
    {
        using var ctx = CreateDbContext();
        await SeedCatalog(ctx);

        var sut = new UpdateBrand(ctx, CreateCurrentUser());
        var result = await sut.Execute(BrandId, new UpdateBrandDto { Name = "Nike Updated", Description = "New desc" });

        Assert.True(result.IsSuccess);
        var brand = await ctx.Brands.FindAsync(BrandId);
        Assert.Equal("Nike Updated", brand!.Name);
        Assert.Equal("New desc", brand.Description);
        Assert.Equal("NIK", brand.Prefix);
    }

    [Fact]
    public async Task UpdateBrand_ShouldReturnNameAlreadyExists()
    {
        using var ctx = CreateDbContext();
        await SeedCatalog(ctx);
        ctx.Brands.Add(new Brand { Id = Guid.NewGuid(), Name = "Adidas", Prefix = "ADI", CreatedBy = TenantId, CreatedByName = "Test User" });
        await ctx.SaveChangesAsync();

        var sut = new UpdateBrand(ctx, CreateCurrentUser());
        var result = await sut.Execute(BrandId, new UpdateBrandDto { Name = "Adidas" });

        Assert.False(result.IsSuccess);
        Assert.Equal(UpdateBrandErrors.BrandNameAlreadyExists, result.Error);
    }

    [Fact]
    public async Task UpdateCategory_ShouldUpdateNameAndDescription()
    {
        using var ctx = CreateDbContext();
        await SeedCatalog(ctx);

        var sut = new UpdateCategory(ctx, CreateCurrentUser());
        var result = await sut.Execute(CategoryId, new UpdateCategoryDto { Name = "Running", Description = "d" });

        Assert.True(result.IsSuccess);
        var category = await ctx.Categories.FindAsync(CategoryId);
        Assert.Equal("Running", category!.Name);
        Assert.Equal("d", category.Description);
    }

    [Fact]
    public async Task UpdateColor_ShouldUpdateName()
    {
        using var ctx = CreateDbContext();
        await SeedCatalog(ctx);

        var sut = new UpdateColor(ctx, CreateCurrentUser());
        var result = await sut.Execute(ColorId, new UpdateColorDto { Name = "Negro Mate" });

        Assert.True(result.IsSuccess);
        var color = await ctx.Colors.FindAsync(ColorId);
        Assert.Equal("Negro Mate", color!.Name);
    }

    [Fact]
    public async Task UpdateSize_ShouldUpdateNameAndSortOrder()
    {
        using var ctx = CreateDbContext();
        await SeedCatalog(ctx);

        var sut = new UpdateSize(ctx, CreateCurrentUser());
        var result = await sut.Execute(SizeId, new UpdateSizeDto { Name = "42.5", SortOrder = 8 });

        Assert.True(result.IsSuccess);
        var size = await ctx.Sizes.FindAsync(SizeId);
        Assert.Equal("42.5", size!.Name);
        Assert.Equal(8, size.SortOrder);
    }
}