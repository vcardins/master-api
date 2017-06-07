using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Omu.ValueInjecter;
using MasterApi.Core.Account.Enums;
using MasterApi.Core.Account.Models;
using MasterApi.Core.Account.Events;
using MasterApi.Core.Account;
using Microsoft.Extensions.Logging;

namespace MasterApi.Services.Account
{
    public partial class UserAccountService 
    {
        protected internal virtual DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

        protected virtual bool CheckHasTooManyRecentPasswordFailures(UserAccount account)
        {
            var result = false;
            if (_settings.Auth.AccountLockoutFailedLoginAttempts <= account.FailedLoginCount)
            {
                result = account.LastFailedLogin >= UtcNow.Subtract(_settings.Auth.AccountLockoutDuration);
                if (!result)
                {
                    // if we're past the lockout window, then reset to zero
                    account.FailedLoginCount = 0;
                }
            }

            if (result)
            {
                account.FailedLoginCount++;
            }

            return result;
        }

        protected virtual void RecordInvalidLoginAttempt(UserAccount account)
        {
            account.LastFailedLogin = UtcNow;
            if (account.FailedLoginCount <= 0)
            {
                account.FailedLoginCount = 1;
            }
            else
            {
                account.FailedLoginCount++;
            }
        }

        public void Update(UserAccount account, bool isInternal = false)
        {
            if (account == null)
            {
                _logger.LogError(GetLogMessage("Failed null account"));
                throw new ArgumentNullException(nameof(account));
            }

            _logger.LogInformation(GetLogMessage($"called for account: {account.UserId}"));

            if (!isInternal)
            {
                account.LastUpdated = UtcNow;
            }
            
            Repository.Update(account, true);

            FireEvents();
        }

        public async Task UpdateAsync(UserAccount account, bool isInternal = true)
        {
            if (account == null)
            {
                _logger.LogError(GetLogMessage("failed null account"));
                throw new ArgumentNullException(nameof(account));
            }

            _logger.LogInformation(GetLogMessage($"called for account: {account.UserId}"));

            if (!isInternal)
            {
                account.LastUpdated = UtcNow;
            }

            await Repository.UpdateAsync(account, true);

            FireEvents();
        }

        protected bool VerifyPassword(UserAccount account, string password)
        {
            _logger.LogInformation(GetLogMessage($"called for account: {account.UserId}"));

            if (string.IsNullOrWhiteSpace(password))
            {
                _logger.LogError(GetLogMessage("called for account: failed -- no password"));
                _accountStatus = UserAccountMessages.MissingPassword;
                return false;
            }

            if (!account.HasPassword())
            {
                _logger.LogError(GetLogMessage("called for account: failed -- account does not have a password"));
                return false;
            }

            if (CheckHasTooManyRecentPasswordFailures(account))
            {
                _logger.LogError(GetLogMessage("called for account: failed -- account in lockout due to failed login attempts"));
                AddEvent(new TooManyRecentPasswordFailuresEvent { Account = account });
                _accountStatus = UserAccountMessages.FailedLoginAttemptsExceeded;
                return false;
            }

            var valid = VerifyHashedPassword(account, password);
            if (valid)
            {
                _logger.LogTrace(GetLogMessage("success"));
                account.FailedLoginCount = 0;
            }
            else
            {
                _logger.LogError(GetLogMessage("failed -- invalid password"));
                _accountStatus = UserAccountMessages.InvalidCredentials;
                RecordInvalidLoginAttempt(account);
                AddEvent(new InvalidPasswordEvent { Account = account });
            }
            return valid;
            
        }

        public virtual bool UsernameExists(string username)
        {
            _logger.LogInformation(GetLogMessage($"called for username; {username}"));
            if (string.IsNullOrWhiteSpace(username)) return false;
            var exists = Repository.Count(x => x.Username == username);
            return exists > 0;
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            _logger.LogInformation(GetLogMessage($"called for username; {username}"));
            if (string.IsNullOrWhiteSpace(username)) return false;
            var exists = await Repository.CountAsync(x => x.Username == username);
            return exists > 0;
        }

        public virtual bool EmailExists(string email)
        {
            _logger.LogInformation(GetLogMessage($"called for email; {email}"));
            if (string.IsNullOrWhiteSpace(email)) return false;
            var exists = Repository.Count(x => x.Email == email);
            return exists > 0;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            _logger.LogInformation(GetLogMessage($"called for username; {email}"));
            if (string.IsNullOrWhiteSpace(email)) return false;
            var exists = await Repository.CountAsync(x => x.Email == email);
            return exists > 0; 
        }

        public bool EmailExistsOtherThan(UserAccount account, string email)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            _logger.LogInformation(GetLogMessage($"called for account id: {account.Guid}, {nameof(email)}; {email}"));

            if (string.IsNullOrWhiteSpace(email)) return false;

            var acct2 = GetByEmail(email);
            if (acct2 != null)
            {
                return account.Guid != acct2.Guid;
            }
            return false;
        }

        public bool PhoneNumberExistsOtherThan(UserAccount account, string phone)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            _logger.LogInformation(GetLogMessage($"called for account id: {account.Guid}, {nameof(phone)} {phone}"));

            if (string.IsNullOrWhiteSpace(phone)) return false;

            var acct2 = GetByMobilePhone(phone);
            if (acct2 != null)
            {
                return account.Guid != acct2.Guid;
            }
            return false;
        }

        public bool VerifyHashedPassword(UserAccount account, string password)
        {
            return account.HasPassword() && _crypto.VerifyHashedPassword(account.HashedPassword, password);
        }

        private bool Authenticate(UserAccount account, string password)
        {
            _logger.LogTrace(GetLogMessage($"for account: {account.UserId}"));
            var isVerified = VerifyPassword(account, password);
            if (!isVerified)
            {
                _accountStatus = UserAccountMessages.InvalidCredentials;
                return false;
            }

            try
            {
                if (!account.IsLoginAllowed)
                {
                    _logger.LogError(GetLogMessage("failed -- account not allowed to login"));
                    AddEvent(new AccountLockedEvent { Account = account });
                    _accountStatus = UserAccountMessages.LoginNotAllowed;
                    return false;
                }

                if (account.IsAccountClosed)
                {
                    _logger.LogError(GetLogMessage("failed -- account closed"));
                    AddEvent(new InvalidAccountEvent { Account = account });
                    _accountStatus = UserAccountMessages.AccountClosed;
                    return false;
                }

                if (_settings.Auth.RequireAccountVerification && !account.IsAccountVerified)
                {
                    _logger.LogError(GetLogMessage("failed -- account not verified"));
                    AddEvent(new AccountNotVerifiedEvent { Account = account });
                    _accountStatus = UserAccountMessages.AccountNotVerified;
                    return false;
                }

                _logger.LogInformation(GetLogMessage("authentication success"));
                account.LastLogin = UtcNow;
                AddEvent(new SuccessfulPasswordLoginEvent { Account = account });

                return true;
            }
            finally
            {
                Update(account, true);                
            }

        }

        public bool Authenticate(string username, string password)
        {
            UserAccount account;
            return Authenticate(username, password, out account);
        }

        public bool Authenticate(string username, string password, out UserAccount account)
        {
            account = null;
            if (string.IsNullOrWhiteSpace(username)) return false;
            if (string.IsNullOrWhiteSpace(password))
            {
                _logger.LogError(GetLogMessage("failed -- empty password"));
                _accountStatus = UserAccountMessages.MissingPassword;
                return false;
            }
            account = GetByUsername(username);
            if (account != null) return Authenticate(account, password);
            _accountStatus = UserAccountMessages.InvalidCredentials;
            return false;
        }

        public Task<bool> AuthenticateAsync(string username, string password, MobileInfo mobileInfo, out UserAccountAuthentication accountAuthentication)
        {
            accountAuthentication = new UserAccountAuthentication { Status = _accountStatus };
            if (!Authenticate(username, password, out UserAccount account))
            {
                return Task.FromResult(false);
            }

            accountAuthentication.InjectFrom(account);
            accountAuthentication.Profile.InjectFrom(account.Profile);
            accountAuthentication.Claims = account.ClaimCollection.Select(uc => new Claim(uc.Type, uc.Value)).ToList();

            if (mobileInfo == null)
            {
                return Task.FromResult(true);
            }

            var isAuthenticated = RegisterMobile(account.UserId, account.Username, mobileInfo);

            return Task.FromResult(isAuthenticated);
        }

        public Task<bool> AuthenticateAsync(string username, string password, out Task<ClaimsIdentity> claimsIdentity, out UserAccountMessages failure)
        {
            claimsIdentity = null;
            failure = UserAccountMessages.None;

            var isAuthenticated = Authenticate(username, password, out UserAccount account);
            if (!isAuthenticated)
            {
                if (_accountStatus != null) failure = _accountStatus.Value;
                return Task.FromResult(false);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, account.Username),
                new Claim(ClaimTypes.NameIdentifier, account.UserId.ToString(), ClaimValueTypes.Integer32),
                new Claim(ClaimTypes.GivenName, account.Profile.FirstName),
                new Claim(ClaimTypes.Surname, account.Profile.LastName),
                new Claim(ClaimTypes.Email, account.Email),
                new Claim("Guid", account.Guid.ToString()),
                new Claim(ClaimTypes.AuthenticationInstant, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.DateTime)
            };

            claims.AddRange(account.ClaimCollection.Select(uc => new Claim(uc.Type, uc.Value)).ToList());

            var roles = claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);

            if (!roles.Any())
            {
                claims.Add(new Claim(ClaimTypes.Role, UserAccessLevel.User.ToString()));
            }

            claimsIdentity = Task.FromResult(new ClaimsIdentity(claims, "Bearer"));

            return Task.FromResult(true);
        }

        public bool AuthenticateWithEmail(string email, string password)
        {
            UserAccount account;
            return AuthenticateWithEmail(email, password, out account);
        }

        public bool AuthenticateWithEmail(string email, string password, out UserAccount account)
        {
            account = null;
            if (string.IsNullOrWhiteSpace(email)) return false;
            if (string.IsNullOrWhiteSpace(password))
            {
                _logger.LogError(GetLogMessage("failed -- empty password"));
                return false;
            }
            account = GetByEmail(email);
            return account != null && Authenticate(account, password);
        }

        public Task<bool> AuthenticateWithEmailAsync(string email, string password, MobileInfo mobileInfo, out UserAccountAuthentication accountAuthentication)
        {
            UserAccount account;
            var isAuthenticate = AuthenticateWithEmail(email, password, out account);
            accountAuthentication = new UserAccountAuthentication { Status = _accountStatus };
            if (!isAuthenticate)
            {
                return Task.FromResult(false);
            }

            accountAuthentication.InjectFrom(account);
            accountAuthentication.Profile.InjectFrom(account.Profile);
            accountAuthentication.Claims = account.ClaimCollection.Select(uc => new Claim(uc.Type, uc.Value)).ToList();

            if (mobileInfo == null)
            {
                return Task.FromResult(true);
            }

            isAuthenticate = RegisterMobile(account.UserId, account.Username, mobileInfo);

            return Task.FromResult(isAuthenticate);
        }

        public bool AuthenticateWithUsernameOrEmail(string userNameOrEmail, string password)
        {
            UserAccount account;
            return AuthenticateWithUsernameOrEmail(userNameOrEmail, password, out account);
        }

        public bool AuthenticateWithUsernameOrEmail(string userNameOrEmail, string password, out UserAccount account)
        {
            account = null;
            if (string.IsNullOrWhiteSpace(userNameOrEmail)) return false;
            if (string.IsNullOrWhiteSpace(password))
            {
                _logger.LogError(GetLogMessage("failed -- empty password"));
                return false;
            }

            if (!_settings.Auth.EmailIsUsername && userNameOrEmail.Contains("@"))
            {
                _logger.LogInformation(GetLogMessage("email detected"));
                return AuthenticateWithEmail(userNameOrEmail, password, out account);
            }

            _logger.LogInformation(GetLogMessage("username detected"));

            return Authenticate(userNameOrEmail, password, out account);
        }

        public Task<bool> AuthenticateWithUsernameOrEmailAsync(string userNameOrEmail, string password, MobileInfo mobileInfo, out UserAccountAuthentication accountAuthentication)
        {
            UserAccount account;
            var isAuthenticate = AuthenticateWithUsernameOrEmail(userNameOrEmail, password, out account);
            accountAuthentication = new UserAccountAuthentication { Status = _accountStatus };
            if (!isAuthenticate)
            {
                return Task.FromResult(false);
            }

            accountAuthentication.InjectFrom(account);
            accountAuthentication.Profile.InjectFrom(account.Profile);
            accountAuthentication.Claims = account.ClaimCollection.Select(uc => new Claim(uc.Type, uc.Value)).ToList();

            if (mobileInfo == null)
            {
                return Task.FromResult(true);
            }

            isAuthenticate = RegisterMobile(account.UserId, account.Username, mobileInfo);

            return Task.FromResult(isAuthenticate);
        }

        private bool RegisterMobile(int userId, string username, MobileInfo currentMobile)
        {
            if (currentMobile.Os == MobileOS.iOS && string.IsNullOrEmpty(currentMobile.Token))
            {
                _accountStatus = UserAccountMessages.MissingDeviceToken;
                return false;
            }

            if (currentMobile.Os == MobileOS.Android && string.IsNullOrEmpty(currentMobile.InstallationId))
            {
                _accountStatus = UserAccountMessages.MissingInstallationId;
                return false;
            }

            if (currentMobile.Os == MobileOS.Android)
            {
                return true;
            }

            var mobile = new UserMobileDevice
            {
                UserId = userId,
                Active = true,
                Token = currentMobile.Token,
                InstallationId = currentMobile.InstallationId,
                OS = currentMobile.Os.ToString(),
                Version = currentMobile.Version,
                Device = currentMobile.Device,
                Created = UtcNow
            };

            Console.Write($"Subscribe to a push notification service {username}");
            //TODO: Subscribe to a push notification service
            //_parseService.SubscribeAsync(mobile, username);
            return true;
        }

        private UserAccount GetAccount(Expression<Func<UserAccount, bool>> query, string value = null, Expression<Func<UserAccount, object>> extraInclude = null)
        {
            const string keyName = nameof(value);

            if (string.IsNullOrWhiteSpace(value))
            {
                var error = GetLogMessage($"Missing {keyName}");
                _logger.LogError(error);
                throw new ValidationException(error);
            }

            _logger.LogInformation(GetLogMessage($"Called {keyName} : {value}"));

            var includes = new Expression<Func<UserAccount, object>>[]
            {
                u => u.ClaimCollection,
                p => p.Profile
            };

            if (extraInclude != null)
            {
                includes[includes.Length] = extraInclude;
            }

            var account = Repository.FirstOrDefault(query, includes);
            if (account == null)
            {
                _logger.LogWarning(GetLogMessage($"failed to locate account by {keyName}: {value}"));
            }
            return account;
        }

        private async Task<UserAccount> GetAccountAsync(Expression<Func<UserAccount, bool>> query, string value = null, Expression<Func<UserAccount, object>> extraInclude = null)
        {
            const string keyName = nameof(value);

            if (string.IsNullOrWhiteSpace(value))
            {
                var error = GetLogMessage($"Missing {keyName}");
                _logger.LogError(error);
                throw new ValidationException(error);
            }

            _logger.LogInformation(GetLogMessage($"Called {nameof(value)} : {value}"));

            var includes = new List<Expression<Func<UserAccount, object>>>
            {
                u => u.ClaimCollection,
                p => p.Profile
            };

            if (extraInclude != null)
            {
                includes.Add(extraInclude);
            }

            var account = await Repository.FirstOrDefaultAsync(query, includes);

            if (account == null)
            {
                _logger.LogWarning(GetLogMessage($"failed to locate account by {keyName}: {value}"));
            }

            return account;
        }

        public UserAccount GetByUsername(string username, Expression<Func<UserAccount, object>> includes = null)
        {
            return GetAccount(x => x.Username == username, username, includes);
        }

        public async Task<UserAccount> GetByUsernameAsync(string username, Expression<Func<UserAccount, object>> includes = null)
        {
            return await GetAccountAsync(x => x.Username == username, username, includes);
        }

        public UserAccount GetByEmail(string email, Expression<Func<UserAccount, object>> includes = null)
        {
            return GetAccount(x => x.Email == email, email, includes);
        }

        public async Task<UserAccount> GetByEmailAsync(string email, Expression<Func<UserAccount, object>> includes = null)
        {
            return await GetAccountAsync(x => x.Email == email, email, includes);
        }

        public UserAccount GetByMobilePhone(string phone, Expression<Func<UserAccount, object>> includes = null)
        {
            return GetAccount(x => x.MobilePhoneNumber == phone, phone, includes);
        }

        public async Task<UserAccount> GetByMobilePhoneAsync(string phone, Expression<Func<UserAccount, object>> includes = null)
        {
            return await GetAccountAsync(x => x.MobilePhoneNumber == phone, phone, includes);
        }

        public UserAccount GetById(int id, Expression<Func<UserAccount, object>> includes = null)
        {
            return GetAccount(x => x.UserId == id, id.ToString(), includes);
        }

        public async Task<UserAccount> GetByIdAsync(int id, Expression<Func<UserAccount, object>> includes = null)
        {
            return await GetAccountAsync(x => x.UserId == id, id.ToString(), includes);
        }

        public UserAccount GetByGuid(Guid guid, Expression<Func<UserAccount, object>> includes = null)
        {
            return GetAccount(x => x.Guid == guid, guid.ToString(), includes);
        }

        public async Task<UserAccount> GetByGuidAsync(Guid guid, Expression<Func<UserAccount, object>> includes = null)
        {
            return await GetAccountAsync(x => x.Guid == guid, guid.ToString(), includes);
        }

        public UserAccount GetByExternalLogin(string provider, string id)
        {
            if (!string.IsNullOrWhiteSpace(provider) && !string.IsNullOrWhiteSpace(id)) { 
                return
                    GetAccount(
                        x => x.ExternalLoginCollection.Any(y => y.LoginProvider == provider && y.ProviderKey == id),
                        provider + id, u => u.ExternalLoginCollection);
            }
            var error = GetLogMessage(UserAccountConstants.ValidationMessages.MissingParameters);
            _logger.LogError(error);
            throw new ValidationException(error);
        }

        public async Task<UserAccount> GetByExternalLoginAsync(string provider, string id)
        {
            if (!string.IsNullOrWhiteSpace(provider) && !string.IsNullOrWhiteSpace(id)) { 
                return
                    await GetAccountAsync(
                        x => x.ExternalLoginCollection.Any(y => y.LoginProvider == provider && y.ProviderKey == id),
                        provider + id, u => u.ExternalLoginCollection);
            }
            var error = GetLogMessage(UserAccountConstants.ValidationMessages.MissingParameters);
            _logger.LogError(error);
            throw new ValidationException(error);
        }

        public UserAccount GetByVerificationKey(string key, Expression<Func<UserAccount, object>> includes = null)
        {
            key = _crypto.Hash(key);
            return GetAccount(x => x.VerificationKey == key, key, includes);
        }

        public async Task<UserAccount> GetByVerificationKeyAsync(string key, Expression<Func<UserAccount, object>> includes = null)
        {
            var key2 = _crypto.Hash(key);
            return await GetAccountAsync(x => x.VerificationKey == key2, key, includes);
        }
    }
}
