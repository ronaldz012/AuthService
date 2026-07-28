using Microsoft.EntityFrameworkCore;
using Module.Sales.Domain;

namespace Module.Sales.Infrastructure.Persistence;

public static class SalesEntityConfiguration
{
    public static void Apply(ModelBuilder builder, Guid? tenantId)
    {
        builder.Entity<Sale>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantId);
            entity.HasMany(s => s.SaleItems)
                .WithOne(i => i.Sale)
                .HasForeignKey(i => i.SaleId);
        });
        builder.Entity<SaleItem>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantId);
        });
        builder.Entity<CashRegisterClosure>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantId);
            entity.HasIndex(e => new { e.TenantId, e.BranchId })
                .IsUnique()
                .HasFilter("\"IsOpen\" = true")
                .HasDatabaseName("IX_CashRegisterClosures_Tenant_Branch_OpenOnly");
            entity.HasMany(c => c.Movements)
                .WithOne(m => m.CashRegisterClosure)
                .HasForeignKey(m => m.CashRegisterClosureId);
            entity.HasMany(c => c.Sales)
                .WithOne(s => s.CashRegisterClosure)
                .HasForeignKey(s => s.CashRegisterClosureId);
        });
        builder.Entity<CashRegisterMovement>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantId && x.DeletedAt == null);
        });
    }
}
