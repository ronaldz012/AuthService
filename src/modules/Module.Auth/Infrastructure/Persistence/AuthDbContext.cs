using Common.Contracts.authentication;
using Common.Domain;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Infrastructure.Persistence;

public class AuthDbContext(DbContextOptions<AuthDbContext> options, ITenantConnectionContext tenantConnectionContext) : DbContext(options), IAuthDbContext
{
    

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var tenantEntries = ChangeTracker.Entries<IMustHaveTenant>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified)
            .ToList();

        if (tenantEntries.Count != 0)
        {
            var currentTenantId = tenantConnectionContext.TenantId;

            foreach (var entry in tenantEntries)
            {
                if (entry.State == EntityState.Added && currentTenantId.HasValue)
                    entry.Entity.TenantId = currentTenantId.Value;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Plan> Plans { get; set; } // <- Agregado para gestionar los planes
    public DbSet<Feature> Features { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<EmailVerificationCode> EmailVerificationCodes { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Branch> Branches { get; set; }
    public DbSet<UserBranchRole> UserBranchRoles { get; set; }
    public DbSet<TenantDataBase> TenantDatabases { get; set; }
    public DbSet<RoleFeaturePermission> RoleFeaturePermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique().HasFilter("\"Email\" IS NOT NULL");

            e.HasMany(u => u.EmailVerificationCodes)
                .WithOne(ev => ev.User)
                .HasForeignKey(ev => ev.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(u => u.UserBranchRoles) 
                .WithOne(ubr => ubr.User)
                .HasForeignKey(ubr => ubr.UserId)
                .OnDelete(DeleteBehavior.Restrict); 

            e.HasQueryFilter(u => u.DeletedAt == null && u.TenantId == tenantConnectionContext.TenantId);
        });
            
        modelBuilder.Entity<Tenant>(e =>
        {
            e.HasOne(t => t.OwnerUser)
                .WithMany()
                .HasForeignKey(t => t.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(t => t.TenantDataBase)
                .WithMany(d => d.Tenants)
                .HasForeignKey(t => t.DataBaseId);
            
            e.HasOne(t => t.Plan)
                .WithMany()
                .HasForeignKey(t => t.PlanId);
            e.HasMany(t => t.Branches)
                .WithOne(b => b.Tenant)
                .HasForeignKey(b => b.TenantId);
        });

        modelBuilder.Entity<Plan>(e =>
        {
            e.OwnsMany(p => p.DefaultRolesTemplate, builder =>
            {
                builder.ToJson();
                builder.OwnsMany(r => r.Permissions);
            });
        });

        modelBuilder.Entity<Feature>(e =>
        {
            e.HasKey(f => f.Key); 
            e.Property(f => f.Key).HasMaxLength(100); 

            e.OwnsMany(f => f.AvailableActions, builder =>
            {
                builder.ToJson();
            });
        });

        // --- CONFIGURACIÓN DE ROLE ---
        modelBuilder.Entity<Role>(e =>
        {
            e.HasIndex(r => new { r.TenantId, r.Name }).IsUnique();
            
            e.HasMany(r => r.RoleFeaturePermissions)
                .WithOne(rfp => rfp.Role)
                .HasForeignKey(rfp => rfp.RoleId)
                .OnDelete(DeleteBehavior.Cascade); 

            // Filtro Global Multitenant (¡Muy recomendado!)
            e.HasQueryFilter(r => r.TenantId == tenantConnectionContext.TenantId);
        });

        // --- CONFIGURACIÓN DE ROLE FEATURE PERMISSION ---
        modelBuilder.Entity<RoleFeaturePermission>(e =>
        {
            
            e.HasIndex(rfp => new { rfp.RoleId, rfp.FeatureKey }).IsUnique();

            e.HasOne(rfp => rfp.Feature)
                .WithMany()
                .HasForeignKey(rfp => rfp.FeatureKey);


            e.HasQueryFilter(rfp => rfp.TenantId == tenantConnectionContext.TenantId);
        });

        modelBuilder.Entity<UserBranchRole>(ubr =>
        {
            ubr.HasKey(ur => new { ur.UserId, ur.BranchId, ur.RoleId });
            
            ubr.HasOne(ur => ur.Branch)
                .WithMany(b => b.UserBranchRoles)
                .HasForeignKey(ur => ur.BranchId);
                
            ubr.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId); // <- Asegúrate de especificar la FK aquí

            ubr.HasQueryFilter(ur => ur.TenantId == tenantConnectionContext.TenantId);
        });

        // --- CONFIGURACIÓN DE BRANCH ---
        modelBuilder.Entity<Branch>(e =>
        {
            e.HasQueryFilter(branch => branch.TenantId == tenantConnectionContext.TenantId);
        });
    }
}