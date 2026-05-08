using Branches.module.Entities;
using Common.Data;
using Common.Domain;
using Microsoft.EntityFrameworkCore;

namespace Branches.module.Data;

public class BranchDbContext (DbContextOptions<BranchDbContext> options, ITenantContext tenantContext) : DbContext(options)
{
 public DbSet<Branch> Branches { get; set; }

 protected override void OnModelCreating(ModelBuilder modelBuilder)
 {
  if (!string.IsNullOrEmpty(tenantContext.Schema))
  {
   modelBuilder.HasDefaultSchema(tenantContext.Schema);
  }
  base.OnModelCreating(modelBuilder);       // luego el base

  modelBuilder.Entity<Branch>(entity =>
  {
   entity.HasQueryFilter(x => x.TenantId == tenantContext.TenantId);
  });

 }
 public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
 {
  // Obtenemos el TenantId actual desde el servicio de contexto
  var currentTenantId = tenantContext.TenantId ?? throw new InvalidOperationException("Tenant is not set") ;

  // Buscamos todas las entidades que:
  // 1. Están siendo agregadas (Added) o modificadas (Modified)
  // 2. Implementan la interfaz ITenantEntity
  var entries = ChangeTracker.Entries<IMustHaveTenant>()
   .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

  foreach (var entry in entries)
  {
   if (entry.State == EntityState.Added)
   {
    // Asignamos el TenantId automáticamente al crear
    entry.Entity.TenantId = currentTenantId;
   }
   else if (entry.State == EntityState.Modified)
   {
    // Opcional: Evitar que se cambie el TenantId en ediciones
    entry.Property(x => x.TenantId).IsModified = false;
   }
  }

  return base.SaveChangesAsync(cancellationToken);
 }
}