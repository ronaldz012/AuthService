using Module.Auth.Domain;

namespace Module.Auth.Domain;

    public enum VerificationCodePurpose
    {
        AccountVerification, 
        PasswordReset,       
        EmailChange,        
    }

    public class EmailVerificationCode
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public Guid TenantId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public int Attempts { get; set; } = 0;
        public VerificationCodePurpose Purpose { get; set; }
        public User User { get; set; } = default!;
    }