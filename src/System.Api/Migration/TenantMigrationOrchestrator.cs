namespace System.Api.Migration;

public class TenantMigrationOrchestrator(MigrationService migrationService)
{
    public async Task<MigrationResult> MigrateAllAsync(string schema)
    {
        var result = new MigrationResult { Schema = schema };

        foreach (var (name, migrate) in Modules())
        {
            try
            {
                await migrate(schema);
                result.Modules[name] = "ok";
            }
            catch (Exception ex)
            {
                result.Modules[name] = ex.Message;
                result.HasErrors = true;
            }
        }

        return result;
    }

    private IEnumerable<(string, Func<string, Task>)> Modules() =>
    [
        ("auth",      migrationService.MigrateAuthTenantAsync),
        ("branches",  migrationService.MigrateBranchTenantAsync),
        ("inventory", migrationService.MigrateInvTenantAsync),
        ("sales",     migrationService.MigrateSalesTenantAsync),
    ];
}

public class MigrationResult
{
    public string Schema { get; set; } = "";
    public bool HasErrors { get; set; }
    public Dictionary<string, string> Modules { get; set; } = [];
}