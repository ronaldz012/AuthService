using Common.Domain;
using Module.Auth.Domain;

namespace Module.Auth.Domain;

public class Branch: IMustHaveTenant

{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Place { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool Status { get; set; } = true;
    public string BranchCode { get; set; } = string.Empty;
    
    public Guid TenantId { get; set; }
    public ICollection<UserBranchRole> UserBranchRoles { get; set; } = new List<UserBranchRole>();
}