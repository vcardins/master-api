using MasterApi.Core.Config;
using MasterApi.Core.Account.Enums;
using MasterApi.Core.Account.Models;
using MasterApi.Core.EventHandling;

namespace MasterApi.Core.Account.Events
{
    internal interface IAllowMultiple { }

    public abstract class UserAccountEvent : IEvent
    {
        public UserAccount Account { get; set; }
        public AppInformation AppInfo { get; set; }
        public AppBaseUrls Urls { get; set; }
    }

    public class AccountCreatedEvent : UserAccountEvent
    {
        // InitialPassword might be null if this is a re-send
        // notification for account created (when user tries to
        // reset password before verifying their account)
        public string InitialPassword { get; set; }
        public string VerificationKey { get; set; }
    }

    public class AccountUnlockedEvent : UserAccountEvent { }

    public class PasswordResetFailedEvent : UserAccountEvent { }

    public class PasswordResetRequestedEvent : UserAccountEvent
    {
        public string VerificationKey { get; set; }
    }
    public class PasswordChangedEvent : UserAccountEvent
    {
        public string NewPassword { get; set; }
    }
    public class PasswordResetSecretAddedEvent : UserAccountEvent
    {
        public PasswordResetSecret Secret { get; set; }
    }
    public class PasswordResetSecretRemovedEvent : UserAccountEvent
    {
        public PasswordResetSecret Secret { get; set; }
    }
    public class UsernameReminderRequestedEvent : UserAccountEvent { }
    public class AccountClosedEvent : UserAccountEvent { }
    public class AccountReopenedEvent : UserAccountEvent
    {
        public string VerificationKey { get; set; }
    }

    public class UsernameChangedEvent : UserAccountEvent
    {
        public string OldUsername { get; set; }
        public string NewUsername { get; set; }
    }

    public class EmailChangeRequestedEvent : UserAccountEvent
    {
        public string OldEmail { get; set; }
        public string NewEmail { get; set; }
        public string VerificationKey { get; set; }
    }
    public class EmailChangedEvent : UserAccountEvent
    {
        public string OldEmail { get; set; }
        public string VerificationKey { get; set; }
    }

    public class EmailVerifiedEvent : UserAccountEvent
    {
        public bool IsNewAccount { get; set; }
    }

    public class MobilePhoneChangeRequestedEvent : UserAccountEvent
    {
        public string NewMobilePhoneNumber { get; set; }
        public string Code { get; set; }
    }

    public class MobilePhoneChangedEvent : UserAccountEvent { }

    public class MobilePhoneRemovedEvent : UserAccountEvent { }

    public class TwoFactorAuthenticationEnabledEvent : UserAccountEvent
    {
        public TwoFactorAuthMode Mode { get; set; }
    }
    public class TwoFactorAuthenticationDisabledEvent : UserAccountEvent { }

    public class MobileVerifiedEvent : UserAccountEvent
    {
        public string PinCode { get; set; }
    }
    public class TwoFactorAuthenticationCodeNotificationEvent : UserAccountEvent
    {
        public string Code { get; set; }
    }
    public class TwoFactorAuthenticationTokenCreatedEvent : UserAccountEvent
    {
        public string Token { get; set; }
    }
    public class ClaimAddedEvent : UserAccountEvent, IAllowMultiple
    {
        public UserClaim Claim { get; set; }
    }
    public class ClaimRemovedEvent : UserAccountEvent, IAllowMultiple
    {
        public UserClaim Claim { get; set; }
    }
    public class ExternalLoginAddedEvent : UserAccountEvent, IAllowMultiple
    {
        public ExternalLogin ExternalLogin { get; set; }
    }
    public class ExternalLoginRemovedEvent : UserAccountEvent, IAllowMultiple
    {
        public ExternalLogin ExternalLogin { get; set; }
    }

    public abstract class SuccessfulLoginEvent : UserAccountEvent { }
    public class SuccessfulPasswordLoginEvent : SuccessfulLoginEvent { }
    public class SuccessfulTwoFactorAuthCodeLoginEvent : SuccessfulLoginEvent { }

    public abstract class FailedLoginEvent : UserAccountEvent { }
    public class AccountNotVerifiedEvent : FailedLoginEvent { }
    public class AccountLockedEvent : FailedLoginEvent { }
    public class InvalidAccountEvent : FailedLoginEvent { }
    public class TooManyRecentPasswordFailuresEvent : FailedLoginEvent { }
    public class InvalidPasswordEvent : FailedLoginEvent { }
}
