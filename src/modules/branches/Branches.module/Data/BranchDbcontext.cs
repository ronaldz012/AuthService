using Branches.module.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Data;

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

 }
}