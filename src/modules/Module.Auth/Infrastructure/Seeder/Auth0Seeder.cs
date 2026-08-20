using Common.Contracts.Seeder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;
using Module.Auth.Infrastructure.Authentication;

namespace Module.Auth.Infrastructure.Seeder;

public class Auth0Seeder(
    IAuthDbContext context,
    IAuth0ProvisioningService provisioning,
    IConfiguration configuration,
    ILogger<Auth0Seeder> logger) : IDataSeeder
{
    public int Order => 5;

    public async Task SeedAsync()
    {
        if (string.IsNullOrWhiteSpace(configuration["Auth0:Domain"]))
        {
            logger.LogWarning("Auth0:Domain not configured. Skipping Auth0 provisioning.");
            return;
        }

        const string email = "admin@drivecore.com";
        const string password = "DriveCore@2026";

        var owner = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email);

        if (owner is null)
            return;

        if (!string.IsNullOrEmpty(owner.ExternalAuthId))
            return;

        try
        {
            var auth0Id = await provisioning.EnsureTestUserAsync(email, password);

            owner.ExternalAuthId = auth0Id;
            owner.AuthProvider = AuthProvider.Auth0;
            owner.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to provision Auth0 test user for {Email}. Seeding continues without Auth0 link.", email);
        }
    }
}
