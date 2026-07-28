namespace Module.Auth.Domain;

public class RefreshToken
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid UserId { get; init; }
    public User User { get; set; } = null!;
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }

    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAt;
}