using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MasterApi.Core.Account.Enums;
using MasterApi.Core.Models;

namespace MasterApi.Core.Account.Models
{
    public class UserAccount : BaseObjectState
    {
        public int UserId { get; set; }

        public virtual Guid Guid { get; set; }

        // If email address is being used as the username then this property
        // should adhere to maximim length constraint for valid email addresses.
        // See Dominic Sayers answer at SO: http://stackoverflow.com/a/574698/99240
        public virtual string Username { get; set; }
        public virtual DateTimeOffset LastUpdated { get; set; }
        public virtual bool IsAccountClosed { get; set; }
        public virtual DateTimeOffset? AccountClosed { get; set; }

        public virtual bool IsLoginAllowed { get; set; }
        public virtual DateTimeOffset? LastLogin { get; set; }
        public virtual DateTimeOffset? LastFailedLogin { get; set; }
        public virtual int FailedLoginCount { get; set; }

        public virtual DateTimeOffset? PasswordChanged { get; set; }
        public virtual bool RequiresPasswordReset { get; set; }

        // Maximum length of a valid email address is 254 characters.
        // See Dominic Sayers answer at SO: http://stackoverflow.com/a/574698/99240
        [EmailAddress]
        public virtual string Email { get; set; }
        public virtual bool IsAccountVerified { get; set; }

        public virtual DateTimeOffset? LastFailedPasswordReset { get; set; }
        public virtual int FailedPasswordResetCount { get; set; }
        public virtual string MobileCode { get; set; }
        public virtual DateTimeOffset? MobileCodeSent { get; set; }
        public virtual string MobilePhoneNumber { get; set; }
        public virtual DateTimeOffset? MobilePhoneNumberChanged { get; set; }
        public virtual string VerificationKey { get; set; }
        public virtual VerificationKeyPurpose? VerificationPurpose { get; set; }
        
        public virtual DateTimeOffset? VerificationKeySent { get; set; }
        public virtual string VerificationStorage { get; set; }
        public virtual string HashedPassword { get; set; }

        public virtual TwoFactorAuthMode AccountTwoFactorAuthMode { get; set; }
        public virtual TwoFactorAuthMode CurrentTwoFactorAuthStatus { get; set; }

        public virtual DateTimeOffset Created { get; set; }
        public virtual UserProfile Profile { get; set; }
        public virtual ICollection<UserClaim> ClaimCollection { get; set; }

        //public IEnumerable<UserClaim> UserClaims
        //{
        //    get { return ClaimCollection ?? new List<UserClaim>(); }
        //}

        public void AddClaim(UserClaim item)
        {
            item.UserId = UserId;
            ClaimCollection.Add(item);
        }

        public void RemoveClaim(UserClaim item)
        {
            ClaimCollection.Remove(item);
        }

        public virtual ICollection<ExternalLogin> ExternalLoginCollection { get; set; }

        //public IEnumerable<ExternalLogin> ExternalLoginSecrets
        //{
        //    get { return ExternalLoginCollection ?? new List<ExternalLogin>(); }
        //}

        public void AddExternalLogin(ExternalLogin item)
        {
            item.UserId = UserId;
            ExternalLoginCollection.Add(item);
        }

        public void RemoveExternalLogin(ExternalLogin item)
        {
            ExternalLoginCollection.Remove(item);
        }

        public virtual ICollection<PasswordResetSecret> PasswordResetSecretCollection { get; set; }

        //public IEnumerable<PasswordResetSecret> PasswordResetSecrets
        //{
        //    get { return PasswordResetSecretCollection ?? new List<PasswordResetSecret>(); }
        //}

        public void AddPasswordResetSecret(PasswordResetSecret item)
        {
            item.UserId = UserId;
            PasswordResetSecretCollection.Add(item);
        }

        public void RemovePasswordResetSecret(PasswordResetSecret item)
        {
            PasswordResetSecretCollection.Remove(item);
        }

        public virtual ICollection<TwoFactorAuthToken> TwoFactorAuthTokenCollection { get; set; }

        //public IEnumerable<TwoFactorAuthToken> TwoFactorAuthTokens
        //{
        //    get { return TwoFactorAuthTokenCollection ?? new List<TwoFactorAuthToken>(); }
        //}

        public void AddTwoFactorAuthToken(TwoFactorAuthToken item)
        {
            item.UserId = UserId;
            TwoFactorAuthTokenCollection.Add(item);
        }

        public void RemoveTwoFactorAuthToken(TwoFactorAuthToken item)
        {
            TwoFactorAuthTokenCollection.Remove(item);
        }

        public virtual ICollection<ExceptionLog> ExceptionsRaised { get; set; }
        public virtual ICollection<AuditLog> AuditLogs { get; set; }
        public virtual ICollection<Notification> NotificationsSent { get; set; }
        public virtual ICollection<Note> Notes { get; set; }

        public bool IsNew()
        {
            return !LastLogin.HasValue;
        }

        public UserAccount()
        {
            ExternalLoginCollection = new HashSet<ExternalLogin> ();
            PasswordResetSecretCollection = new HashSet<PasswordResetSecret>();
            ClaimCollection = new HashSet<UserClaim>();
            TwoFactorAuthTokenCollection = new HashSet<TwoFactorAuthToken>();
            ExceptionsRaised = new HashSet<ExceptionLog>();
            AuditLogs = new HashSet<AuditLog>();
            NotificationsSent = new HashSet<Notification>();
        }
    }

}

