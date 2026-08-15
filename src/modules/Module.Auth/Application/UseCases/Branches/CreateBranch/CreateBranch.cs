using Common.Contracts.authentication;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Module.Auth.Application.Abstraction;
using Module.Auth.Domain;

namespace Module.Auth.Application.UseCases.Branches.CreateBranch;

public class CreateBranch(IAuthDbContext context)
{
    public async Task<Result<BranchCreatedResponse>> Execute(ActorContext ctx, CreateBranchRequest request)
    {
        var tenantId = ctx.TenantId;

        var planAllowedKeys = await context.Tenants
            .Where(t => t.Id == tenantId)
            .Select(t => t.Plan.AllowedFeatureKeys)
            .FirstOrDefaultAsync();

        if (planAllowedKeys == null)
            return CreateBranchErrors.PlanNotFound;

        var features = await context.Features
            .Select(f => new FeatureModuleInfo(f.Key, f.Module))
            .ToListAsync();

        var branchFeatureKeys = BranchFeatureKeysResolver.Resolve(planAllowedKeys, request.Type, features);

        var newBranch = Branch.Create(
            Guid.NewGuid(),
            request.Name,
            request.Place,
            request.PhoneNumber,
            request.Type,
            ctx.UserId,
            ctx.FullName);
        newBranch.BranchCode = request.BranchCode;
        newBranch.AllowedFeatureKeys = branchFeatureKeys;

        context.Branches.Add(newBranch);
        await context.SaveChangesAsync();

        return new BranchCreatedResponse
        {
            Id = newBranch.Id,
            Name = newBranch.Name,
            Type = newBranch.Type,
            AllowedFeatureKeys = newBranch.AllowedFeatureKeys,
        };
    }
}