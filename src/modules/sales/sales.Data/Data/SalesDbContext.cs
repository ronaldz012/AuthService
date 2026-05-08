
using Microsoft.EntityFrameworkCore;
using sales.use.Entities;
using Common.Data;
using Common.Services;

namespace sales.Module.Data;

public class SalesDbContext(DbContextOptions<SalesDbContext> options, ITenantContext tenantContext) : DbContext(options)
{
    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleItem> SaleItems { get; set; }
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (!string.IsNullOrEmpty(tenantContext.Schema))
        {
            Console.Write("desde el tenant estamos cambiendo el schem: "+tenantContext.Schema);
            modelBuilder.HasDefaultSchema(tenantContext.Schema);
        }


        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasMany(s => s.SaleItems)
                .WithOne(i => i.Sale)
                .HasForeignKey(i => i.SaleId);
        });
    }

    public override int SaveChanges()
    {
        // Detectar si estamos en modo de diseño (migraciones)
        bool isMigration = EF.IsDesignTime; 

        if (isMigration)
        {
            // Comportamiento especial: por ejemplo, saltar una validación
            return base.SaveChanges();
        }

        return base.SaveChanges();
    }
}



