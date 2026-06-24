using Common.Contracts.authentication;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Infrastructure.persistence;

public class AuthDbContext(DbContextOptions<AuthDbContext> options, ITenantContext tenantContext) : DbContext(options), IAuthDbContext
{
    private readonly Guid? _tenantId = tenantContext.TenantId;
    public DbSet<Feature> Features { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<EmailVerificationCode> EmailVerificationCodes { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Branch>  Branches { get; set; }
    public DbSet<UserBranchRole> UserBranchRoles { get; set; }
    public DbSet<RoleFeaturePermission> RoleFeaturePermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();

            e.HasMany(u => u.EmailVerificationCodes)
                .WithOne(ev => ev.User)
                .HasForeignKey(ev => ev.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(u => u.UserBranchRoles) 
                .WithOne(ubr => ubr.User)
                .HasForeignKey(ubr => ubr.UserId)
                .OnDelete(DeleteBehavior.Restrict); 

            e.HasQueryFilter(u => u.DeletedAt == null);
        });
            
        modelBuilder.Entity<Tenant>(e =>
        {
            e.HasOne(t => t.OwnerUser)
                .WithMany()
                .HasForeignKey(t => t.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserBranchRole>(ubr =>
        {
            ubr.HasKey(ur => new { ur.BranchId, ur.RoleId });
            ubr.HasOne(ur => ur.Branch)
                .WithMany(b => b.UserBranchRoles)
                .HasForeignKey(ur => ur.BranchId);
            ubr.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles);
        });
        
        modelBuilder.Entity<Role>(e =>
        {
            e.HasIndex(r => new { r.TenantId, r.Name }).IsUnique();
            e.HasMany(r => r.RoleFeaturePermissions)
                .WithOne(rfp => rfp.Role)
                .HasForeignKey(rfp => rfp.RoleId)
                .OnDelete(DeleteBehavior.Cascade); 

        });
        modelBuilder.Entity<Branch>(e =>
        {
            e.HasQueryFilter(branch => branch.TenantId == _tenantId);
        });


    }
}