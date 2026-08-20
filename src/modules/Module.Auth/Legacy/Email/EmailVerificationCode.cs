using Common.Domain;
using Module.Auth.Domain;

namespace Module.Auth.Domain;

    public enum VerificationCodePurpose
    {
        AccountVerification, 
        PasswordReset,       
        EmailChange,        
    }

    public class EmailVerificationCode : IMustHaveTenant, ICreatedAt
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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = default!;

        public bool IsExpired => ExpiresAt < DateTime.UtcNow;

        public static EmailVerificationCode CreateForAccountSetup(string email, Guid? userId = null)
        {
            return new EmailVerificationCode
            {
                Email = email,
                Code = Guid.NewGuid().ToString("N"),
                SentAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(48),
                Purpose = VerificationCodePurpose.AccountVerification,
                IsUsed = false,
                UserId = userId ?? Guid.Empty,
                CreatedAt = DateTime.UtcNow,
            };
        }

        public void MarkAsUsed()
        {
            IsUsed = true;
        }

        public void IncrementAttempts()
        {
            Attempts++;
        }
    }