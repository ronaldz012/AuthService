using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Application.UseCases.Users.CreateTenantAdmin;

public class CreateTenantAdmin(
    IAuthDbContext context,
    ITenantConnectionContext tenantConnectionContext)
{
    public async Task<Result<CreateTenantAdminResponse>> Execute(ActorContext ctx, CreateTenantAdminRequest dto)
    {
        var displayName = await context.Tenants
            .Where(t => t.Id == tenantConnectionContext.TenantId)
            .Select(t => t.DisplayName)
            .FirstAsync();

        var globalUsername = $"{displayName}-{dto.Username}";

        var usernameTaken = await context.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Username == globalUsername);
        if (usernameTaken) return CreateTenantAdminErrors.EmailOrUsernameTaken;

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var emailTaken = await context.Users
                .IgnoreQueryFilters()
                .AnyAsync(u => u.Email == dto.Email);
            if (emailTaken) return CreateTenantAdminErrors.EmailOrUsernameTaken;
        }

        var newUser = User.CreateTenantAdmin(
            dto.Email, globalUsername, dto.FirstName, dto.LastName,
            dto.Ci, dto.Nationality, dto.BirthDate, ctx.UserId, ctx.FullName);

        context.Users.Add(newUser);

        await context.SaveChangesAsync();

        return new CreateTenantAdminResponse(newUser.Id, string.Empty, false);
    }
}
