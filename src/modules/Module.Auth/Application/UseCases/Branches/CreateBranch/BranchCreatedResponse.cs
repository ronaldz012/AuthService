namespace Module.Auth.Application.UseCases.Branches.CreateBranch;

using Module.Auth.Domain;

public class BranchCreatedResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Status { get; set; } = true;
    public string BranchCode { get; set; } = string.Empty;
    public BranchType Type { get; set; } = BranchType.Warehouse;
    public List<string> AllowedFeatureKeys { get; set; } = [];
}