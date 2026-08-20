using Common.Contracts.Seeder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Infrastructure.Seeder;

public class Auth0Seeder(
    IAuthDbContext context,
    IAuth0ProvisioningService provisioning,
    ILogger<Auth0Seeder> logger) : IDataSeeder
{
    public int Order => 5;

    public async Task SeedAsync()
    {
        const string email = "admin@drivecore.com";
        const string password = "DriveCore@2026";

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
