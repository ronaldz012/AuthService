using Module.Auth.Application.UseCases.Branches.CreateBranch;

namespace Test.Auth.Branches;

public class CreateBranchTests
{
    [Fact]
    public async Task Execute_ShouldCreateBranch_WithTenantIdFromContext()
    {
        var tenantId = Guid.NewGuid();
        var tenantContext = TestAuthDbContextFactory.CreateTenantContext(tenantId);
        using var dbContext = TestAuthDbContextFactory.Create(tenantContext);

        var sut = new CreateBranch(dbContext);

        var request = new CreateBranchRequest
        {
            Name = "Test Branch",
            Place = "Test Place",
            PhoneNumber = "123456789",
            BranchCode = "BR-001",
        };

        var result = await sut.Execute(request);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        Assert.Equal("Test Branch", result.Value.Name);

        var savedBranch = await dbContext.Branches.FindAsync(result.Value.Id);
        Assert.NotNull(savedBranch);
        Assert.Equal(tenantId, savedBranch.TenantId);
    }
}
