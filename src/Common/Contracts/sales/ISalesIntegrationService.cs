namespace Common.Contracts.sales;

public interface ISalesIntegrationService
{
    Task<bool> BranchHasOpenClosures(Guid branchId);
}
