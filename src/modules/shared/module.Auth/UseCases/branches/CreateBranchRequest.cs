namespace module.Auth.Features.branches;

public class CreateBranchRequest
{
    public string Name { get; set; } = string.Empty;
    public string Place { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
}