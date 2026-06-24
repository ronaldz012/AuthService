using Common.Contracts.authentication;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Infrastructure.persistence;
using Module.Inventory.Infrastructure;
using Module.Sales.Infrastructure.Persistence;

namespace System.Api.Data;

public class AuthDbContextFactory()
    : DesignTimeDbContextFactory<AuthDbContext>("__EFMigrationsHistory_auth")
{
    protected override AuthDbContext CreateInstance(
        DbContextOptions<AuthDbContext> options, ITenantContext tenant)
        => new(options, tenant);
    
}



public class InvDbContextFactory()
    : DesignTimeDbContextFactory<InvDbContext>("__EFMigrationsHistory_inventory")
{
    protected override InvDbContext CreateInstance(
        DbContextOptions<InvDbContext> options, ITenantContext tenant)
        => new(options, tenant);
}

public class SalesDbContextFactory()
    : DesignTimeDbContextFactory<SalesDbContext>("__EFMigrationsHistory_sales")
{
    protected override SalesDbContext CreateInstance(
        DbContextOptions<SalesDbContext> options, ITenantContext tenant)
        => new(options, tenant);
}