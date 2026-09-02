using Common.Contracts.Seeder;
using Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Infrastructure.Seeder;

public class Auth0Seeder(
    IAuthDbContext context,
    IAuth0ProvisioningService provisioning,
    IOptions<SeederSettings> seederSettings,
    ILogger<Auth0Seeder> logger) : IDataSeeder
{
    public int Order => 5;

    public async Task SeedAsync()
    {
        var email = seederSettings.Value.AdminEmail;
        var password = seederSettings.Value.AdminPassword;

        var owner = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email);

        if (owner is null)
            return;

        if (!string.IsNullOrEmpty(owner.ExternalAuthId))
            return;

        var result = await provisioning.EnsureTestUserAsync(email, password);
        if (!result.IsSuccess)
        {
            logger.LogError("Failed to provision Auth0 test user for {Email}: {Error}. Seeding continues without Auth0 link.", email, result.Error.Message);
            return;
        }

        owner.ExternalAuthId = result.Value;
        owner.AuthProvider = AuthProvider.Auth0;
        owner.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
    }
}
