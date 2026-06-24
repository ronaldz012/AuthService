namespace Module.Auth.Application.UseCases.Branches;

public class BranchResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Status { get; set; } = true;
    public string BranchCode { get; set; } = string.Empty;
}