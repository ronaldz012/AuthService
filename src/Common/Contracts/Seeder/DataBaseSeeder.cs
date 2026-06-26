namespace Common.Contracts.Seeder;

public class DatabaseSeeder(IEnumerable<IDataSeeder> seeders)
{
    public async Task SeedAllAsync()
    {
        foreach (var seeder in seeders.OrderBy(s => s.Order))
            await seeder.SeedAsync();
    }
}