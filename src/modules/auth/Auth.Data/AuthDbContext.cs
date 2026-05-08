using Auth.Data.Entities;
using Common.Data;
using Common.Domain;
using Microsoft.EntityFrameworkCore;

namespace Auth.Data;

public class AuthDbContext(DbContextOptions<AuthDbContext> options, ITenantContext tenantContext) : DbContext(options)
{
    // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserBranchRole> UserBranchRoles { get; set; }
        public DbSet<RoleFeaturePermission> RoleFeaturePermissions { get; set; }
        public DbSet<EmailVerificationCode> EmailVerificationCodes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
       
        if (!string.IsNullOrEmpty(tenantContext.Schema))
        {
            modelBuilder.HasDefaultSchema(tenantContext.Schema);
        }
        
        base.OnModelCreating(modelBuilder);       // luego el base
        
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantContext.TenantId);

            entity.HasMany(u => u.EmailVerificationCodes)
                  .WithOne(evc => evc.User)
                  .HasForeignKey(evc => evc.UserId);
        });
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantContext.TenantId);

        });
        modelBuilder.Entity<UserBranchRole>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId == tenantContext.TenantId);
            entity.HasOne(ur => ur.User)
                  .WithMany(u => u.UserBranchRoles)
                  .HasForeignKey(ur => ur.UserId);

            entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId);
            
            
        });

        modelBuilder.Entity<RoleFeaturePermission>(entity =>
        {
            entity.HasQueryFilter(x => x.TenantId ==  tenantContext.TenantId);
            entity.HasOne(rmp => rmp.Role)
                  .WithMany(r => r.RoleFeaturePermissions)
                  .HasForeignKey(rmp => rmp.RoleId);
        });
        modelBuilder.Entity<EmailVerificationCode>(entity =>
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
