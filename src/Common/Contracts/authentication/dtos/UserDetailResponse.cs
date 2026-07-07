namespace Common.Contracts.authentication.dtos;

public class UserDetailResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public int UserType { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
