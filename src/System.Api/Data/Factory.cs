using Auth.Data.Persistence;
using Branches.module.Data;
using Microsoft.EntityFrameworkCore;
using sales.Module.Data;
using Common.Data;
using Inventory.Data;

namespace System.Api.Data;

public class AuthDbContextFactory()
    : DesignTimeDbContextFactory<AuthDbContext>("__EFMigrationsHistory_auth")
{
    protected override AuthDbContext CreateInstance(
        DbContextOptions<AuthDbContext> options, ITenantContext tenant)
        => new(options, tenant);
    
}

public class BranchDbContextFactory()
    : DesignTimeDbContextFactory<BranchDbContext>("__EFMigrationsHistory_branches")
{
    protected override BranchDbContext CreateInstance(
        DbContextOptions<BranchDbContext> options, ITenantContext tenant)
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