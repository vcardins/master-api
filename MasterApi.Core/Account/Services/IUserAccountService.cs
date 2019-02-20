using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using MasterApi.Core.Account.Models;
using MasterApi.Core.Account.ViewModels;
using MasterApi.Core.Config;
using MasterApi.Core.Services;
using MasterApi.Core.Account.Enums;
using System.Linq.Expressions;

namespace MasterApi.Core.Account.Services
{
    public interface IUserAccountService : IService<UserAccount>
    {
        #region General 

        AuthSettings Settings { get; set; }

        void Update(UserAccount account, bool isInternal);

        Task UpdateAsync(UserAccount account, bool isInternal);
        
        #endregion

        #region Retrieve Account

        UserAccount GetByUsername(string username, Expression<Func<UserAccount, object>> includes = null);
        Task<UserAccount> GetByUsernameAsync(string username, Expression<Func<UserAccount, object>> includes = null);

        UserAccount GetByEmail(string email, Expression<Func<UserAccount, object>> includes = null);
        Task<UserAccount> GetByEmailAsync(string email, Expression<Func<UserAccount, object>> includes = null);

        UserAccount GetByMobilePhone(string phone, Expression<Func<UserAccount, object>> includes = null);
        Task<UserAccount> GetByMobilePhoneAsync(string phone, Expression<Func<UserAccount, object>> includes = null);

        UserAccount GetById(int id, Expression<Func<UserAccount, object>> includes = null);
        Task<UserAccount> GetByIdAsync(int id, Expression<Func<UserAccount, object>> includes = null);

        UserAccount GetByGuid(Guid guid, Expression<Func<UserAccount, object>> includes = null);
        Task<UserAccount> GetByGuidAsync(Guid guid, Expression<Func<UserAccount, object>> includes = null);

        UserAccount GetByExternalLogin(string provider, string id);
        Task<UserAccount> GetByExternalLoginAsync(string provider, string id);

        UserAccount GetByVerificationKey(string key, Expression<Func<UserAccount, object>> includes = null);
        Task<UserAccount> GetByVerificationKeyAsync(string key, Expression<Func<UserAccount, object>> includes = null);

        #endregion

        #region Account Checks

        bool UsernameExists(string username);
        Task<bool> UsernameExistsAsync(string username);

        bool EmailExists(string username);
        Task<bool> EmailExistsAsync(string username);

        bool EmailExistsOtherThan(UserAccount account, string value);

        bool PhoneNumberExistsOtherThan(UserAccount account, string value);

        bool VerifyHashedPassword(UserAccount account, string value);

        #endregion

        #region Create Update Account

        Task CreateAccountAsync(RegisterInput model);
        Task CreateAccountAsync(string username, string password, string email);

        Task<string> RequestVerificationAsync(string email);

        Task<string> CancelVerificationAsync(string key);

        Task DeleteAccountAsync(Guid accountId);

        Task CloseAccountAsync(Guid accountId);

        Task ReopenAccountAsync(string username, string password);

        Task ReopenAccountAsync(int accountId);

        Task SendUsernameReminder(string email);

        Task ChangeUsernameAsync(int accountId, string newUsername);

        Task ChangeEmailRequestAsync(int accountId, string newEmail);

        Task ChangeMobilePhoneRequestAsync(int accountId, string newEmail);

        Task<bool> ChangeMobilePhoneFromCodeAsync(int accountId, string code);

        Task SetConfirmedMobilePhoneAsync(int accountIdD, string phone);
     
        Task SetIsLoginAllowed(Guid accountId, bool isLoginAllowed);

        Task SetRequiresPasswordReset(Guid accountId, bool requiresPasswordReset);

        Task SetPassword(Guid accountId, string newPassword);

        Task ChangePasswordAsync(int accountId, string oldPassword, string newPassword);

        Task SetConfirmedEmailAsync(int accountId, string email);

        Task<bool> IsPasswordExpiredAsync(int accountId);

        bool IsPasswordExpired(UserAccount account);

        Task RemoveMobilePhoneAsync(int accountId);

        Task<string> VerifyEmailFromKeyAsync(string key);

        Task<string> VerifyEmailFromKeyAsync(string key, string password);

        Task ResetPasswordAsync(string email);

        Task ResetPasswordAsync(Guid accountId);

        Task ResetFailedLoginCount(Guid accountId);

        Task<IEnumerable<PasswordResetSecretOutput>> GetSecretQuestionsAsync(int accountId);

        Task<UserAccount> ChangePasswordFromResetKeyAsync(string key, string password);

        Task AddPasswordResetSecretAsync(int accountId, string question, string answer);

        Task RemovePasswordResetSecretAsync(int accountId, Guid questionId);

        Task ResetPasswordFromSecretQuestionAndAnswerAsync(Guid accountId, PasswordResetQuestionAnswer[] answers);

        Task ConfigureTwoFactorAuthenticationAsync(int accountId, TwoFactorAuthMode mode);

        Task SendTwoFactorAuthenticationCodeAsync(int accountId);

        #endregion


        #region Authenticate Account

        bool Authenticate(string username, string password);
        bool Authenticate(string username, string password, out UserAccount account);
        Task<bool> AuthenticateAsync(string username, string password, MobileInfo mobileInfo, out UserAccountAuthentication account);

		Task<ClaimsIdentity> AuthenticateAsync(string username, string password);
		bool AuthenticateWithEmail(string email, string password);
        bool AuthenticateWithEmail(string email, string password, out UserAccount account);
        Task<bool> AuthenticateWithEmailAsync(string email, string password, MobileInfo mobileInfo, out UserAccountAuthentication account);

        bool AuthenticateWithUsernameOrEmail(string userNameOrEmail, string password);
        bool AuthenticateWithUsernameOrEmail(string userNameOrEmail, string password, out UserAccount account);
        Task<bool> AuthenticateWithUsernameOrEmailAsync(string userNameOrEmail, string password, MobileInfo mobileInfo, out UserAccountAuthentication account);

        #endregion

        #region HandleAccountClaims

        void AddClaims(int accountId, UserClaimCollection claims);

        void RemoveClaims(int accountId, UserClaimCollection claims);

        void UpdateClaims(
            int accountId,
            UserClaimCollection additions = null,
            UserClaimCollection deletions = null);

        void AddClaim(int accountId, string type, string value);
        void RemoveClaim(int accountId, string type);
        void RemoveClaim(int accountId, string type, string value);

        IEnumerable<Claim> MapClaims(UserAccount account);

        void AddOrUpdateExternalLogin(UserAccount account, string provider, string id, IEnumerable<Claim> claims = null);
        void RemoveExternalLogin(int accountId, string provider, string id);

        #endregion
            
    }
}