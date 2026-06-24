using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Module.Auth.Domain;

namespace Module.Auth.Application.Abstraction;

public interface IAuthDbContext
{
    DbSet<Feature> Features { get; }
    DbSet<Tenant> Tenants { get; }
    DbSet<User> Users { get; }
    DbSet<EmailVerificationCode> EmailVerificationCodes { get; }
    DbSet<Role> Roles { get; }
    DbSet<Branch> Branches { get; }
    DbSet<UserBranchRole> UserBranchRoles { get; }
    DbSet<RoleFeaturePermission> RoleFeaturePermissions { get; }

    DbSet<TEntity> Set<TEntity>()
        where TEntity : class;

    EntityEntry<TEntity> Add<TEntity>(TEntity entity)
        where TEntity : class;

    ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(
        TEntity entity,
        CancellationToken cancellationToken = default)
        where TEntity : class;

    EntityEntry<TEntity> Update<TEntity>(TEntity entity)
        where TEntity : class;

    EntityEntry<TEntity> Remove<TEntity>(TEntity entity)
        where TEntity : class;

    EntityEntry Entry(object entity);

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);


}
