
namespace Common.Contracts.Seeder;

public interface IDataSeeder
{
    int Order { get; } 
    Task SeedAsync();
}