namespace Common.Contracts.authentication.dtos;

public class PermissionsByBranchDto
{
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<SessionFeatureDto> Features { get; set; } = [];
}

public class SessionFeatureDto
{
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public bool IsMenu { get; set; }
    public List<string> Permissions { get; set; } = [];
}