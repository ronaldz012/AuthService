using Common.Contracts.authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Module.Inventory.Application.UseCases.Providers.GetProviders;
using Module.Inventory.Application.UseCases.Providers.Update;
using Module.Inventory.Domain.Organization;
using System.Infrastructure.Persistence;

namespace Test.Inventory;

public class ProviderTests
{
    private static readonly Guid TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ProviderId = Guid.NewGuid();

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

    private static ActorContext CreateActorContext()
        => new(TenantId, UserId, "Test User", Guid.Empty, []);

    private static async Task SeedProvider(TestAppDbContext ctx, Guid id, string name, bool isActive)
    {
        ctx.Providers.Add(new Provider
        {
            Id = id,
            Name = name,
            IsActive = isActive,
            CreatedBy = UserId,
            CreatedByName = "Test User"
        });
        await ctx.SaveChangesAsync();
    }

    [Fact]
    public async Task GetProviders_ShouldExcludeInactive_ByDefault()
    {
        using var ctx = CreateDbContext();
        await SeedProvider(ctx, ProviderId, "Active Provider", true);
        await SeedProvider(ctx, Guid.NewGuid(), "Inactive Provider", false);

        var sut = new GetProviders(ctx);
        var result = await sut.Execute();

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(result.Value, p => !p.IsActive);
    }

    [Fact]
    public async Task GetProviders_ShouldIncludeInactive_WhenRequested()
    {
        using var ctx = CreateDbContext();
        await SeedProvider(ctx, ProviderId, "Active Provider", true);
        await SeedProvider(ctx, Guid.NewGuid(), "Inactive Provider", false);

        var sut = new GetProviders(ctx);
        var result = await sut.Execute(includeInactive: true);

        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value, p => !p.IsActive);
    }

    [Fact]
    public async Task UpdateProvider_ShouldPersistFields_AndSetAudit()
    {
        using var ctx = CreateDbContext();
        await SeedProvider(ctx, ProviderId, "Old Name", true);

        var sut = new UpdateProvider(ctx);
        var result = await sut.Execute(CreateActorContext(), ProviderId, new UpdateProviderRequest
        {
            Name = "New Name",
            ContactName = "Carlos",
            Email = "carlos@mail.com",
            PhoneNumber = "999",
            Address = "Street 1"
        });
        Assert.True(result.IsSuccess);
        var provider = await ctx.Providers.FindAsync(ProviderId);
        Assert.Equal("New Name", provider!.Name);
        Assert.Equal("Carlos", provider.ContactName);
        Assert.Equal("carlos@mail.com", provider.Email);
        Assert.Equal("999", provider.PhoneNumber);
        Assert.Equal("Street 1", provider.Address);
        Assert.Equal(UserId, provider.UpdatedBy);
        Assert.Equal("Test User", provider.UpdatedByName);
        Assert.NotNull(provider.UpdatedAt);
    }

    [Fact]
    public async Task UpdateProvider_ShouldReturnNameAlreadyExists()
    {
        using var ctx = CreateDbContext();
        await SeedProvider(ctx, ProviderId, "Original", true);
        await SeedProvider(ctx, Guid.NewGuid(), "Taken Name", true);

        var sut = new UpdateProvider(ctx);
        var result = await sut.Execute(CreateActorContext(), ProviderId, new UpdateProviderRequest { Name = "Taken Name" });

        Assert.False(result.IsSuccess);
        Assert.Equal(UpdateProviderErrors.ProviderNameAlreadyExists, result.Error);
    }

    [Fact]
    public async Task ToggleProvider_ShouldFlipAndSetAudit()
    {
        using var ctx = CreateDbContext();
        await SeedProvider(ctx, ProviderId, "Provider", true);

        var sut = new UpdateProvider(ctx);
        var result = await sut.ChangeStatus(CreateActorContext(), ProviderId);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value);

        var provider = await ctx.Providers.FindAsync(ProviderId);
        Assert.False(provider!.IsActive);
        Assert.Equal(UserId, provider.UpdatedBy);
        Assert.NotNull(provider.UpdatedAt);
    }

    [Fact]
    public async Task ToggleProvider_ShouldReturnNotFound_WhenMissing()
    {
        using var ctx = CreateDbContext();
        var sut = new UpdateProvider(ctx);

        var result = await sut.ChangeStatus(CreateActorContext(), Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(UpdateProviderErrors.ProviderNotFound, result.Error);
    }
}