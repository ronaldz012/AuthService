using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;

namespace Module.Auth.Application.UseCases.TenantDatabases.GetById;

public class GetTenantDatabaseDetails(IAuthDbContext context, IDbConnectionTester connectionTester)
{
    public async Task<Result<TenantDatabaseDetailsResponse>> Execute(Guid id)
    {
        var db = await context.TenantDatabases.Select(x => new TenantDatabaseDetailsResponse
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            Schema = x.Schema,
            Tenants = x.Tenants.Select(t => new TenantDatabaseCompanyDetailsResponse
            {
                Id = t.Id,
                DisplayName = t.DisplayName,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt,
                OwnerEmail = t.OwnerUser.Email,
                OwnerName = t.OwnerUser.FirstName + " " + t.OwnerUser.LastName,
                PlaneName = t.Plan.Name
            })
        }).FirstOrDefaultAsync(x => x.Id == id);
        if(db is  null)
            return new Error("NOT_FOUND", "Database not found");
        
        var result =  await connectionTester.TestConnectionAsync(db.Name, db.Schema);
        db.IsOnline = result.IsSuccess;
        return db;
    }
    
}