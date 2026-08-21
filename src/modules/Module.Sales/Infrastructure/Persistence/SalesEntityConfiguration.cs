using Microsoft.EntityFrameworkCore;
using Module.Sales.Domain;

namespace Module.Sales.Infrastructure.Persistence;

public static class SalesEntityConfiguration
{
    public static void Apply(ModelBuilder builder)
    {
        builder.Entity<Sale>(entity =>
        {
            entity.HasMany(s => s.SaleItems)
                .WithOne(i => i.Sale)
                .HasForeignKey(i => i.SaleId);
        });
        builder.Entity<SaleItem>(entity =>
        {
        });
        builder.Entity<CashRegisterClosure>(entity =>
        {
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
        });
    }
}
