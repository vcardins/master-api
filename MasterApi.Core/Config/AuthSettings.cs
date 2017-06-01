using System;

namespace MasterApi.Core.Config
{
    public class AuthSettings
    {
        public string TokenAudience { get; set; }
        public string TokenIssuer { get; set; }

        public bool MultiTenant { get; set; }
        public string DefaultTenant { get; set; }
        public bool EmailIsUsername { get; set; }
        public bool UsernamesUniqueAcrossTenants { get; set; }
        public bool RequireAccountVerification { get; set; }
        public bool AllowLoginAfterAccountCreation { get; set; }
        public int AccountLockoutFailedLoginAttempts { get; set; }
        public TimeSpan AccountLockoutDuration { get; set; }
        public bool AllowAccountDeletion { get; set; }
        public int MinimumPasswordLength { get; set; }
        public int PasswordResetFrequency { get; set; }
        public int PasswordHashingIterationCount { get; set; }
        public int MobileVerificationCodeLength { get; set; }
        public int MobileCodeStaleDurationMinutes { get; set; }
        public int MobileCodeResendDelayMinutes { get; set; }
        public int TwoFactorAuthTokenDurationDays { get; set; }
        
        public bool AllowEmailChangeWhenEmailIsUsername { get; set; }
        public bool AllowMultipleMobileLogin { get; set; }
        public TimeSpan VerificationKeyLifetime { get; set; }
        public bool EmailIsUnique { get; set; }
    }
}
