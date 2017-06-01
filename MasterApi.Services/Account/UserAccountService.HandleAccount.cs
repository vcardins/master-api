using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MasterApi.Core.Account.Models;
using MasterApi.Core.Account.ViewModels;
using MasterApi.Core.Data.Infrastructure;
using MasterApi.Core.Models;
using Omu.ValueInjecter;
using MasterApi.Core.Account.Enums;
using MasterApi.Core.Account.Events;
using MasterApi.Core.Account;

namespace MasterApi.Services.Account
{
    public partial class UserAccountService 
    {
        public async Task CreateAccountAsync(RegisterInput model)
        {
            if (_settings.Auth.EmailIsUsername)
            {
                _logger.LogTrace(GetLogMessage("applying email is username"));
                model.Username = model.Email;
            }

            if (!_settings.Auth.MultiTenant)
            {
                _logger.LogTrace(GetLogMessage("applying default tenant"));
            }

            _logger.LogInformation(GetLogMessage($"called: {model.Username}, {model.Email}"));

            var account = new UserAccount
            {
                ClaimCollection = new UserClaimCollection
                {
                    new UserClaim(ClaimTypes.Role, UserAccessLevel.User.ToString()) { ObjectState = ObjectState.Added }
                },
                Profile = new UserProfile
                {
                    ObjectState = ObjectState.Added,
                    Iso2 = model.Country
                },
                ObjectState = ObjectState.Added
            };

            account.InjectFrom(model);
            account.Profile.InjectFrom(model);

            Init(account, model.Username, model.Password, model.Email);

            ValidateEmail(account, model.Email);
            ValidateUsername(account, model.Username);
            ValidatePassword(account, model.Password);

            _logger.LogTrace(GetLogMessage("Success"));

            await Repository.InsertAsync(account, true);

            FireEvents();
        }

        public async Task CreateAccountAsync(string username, string password, string email)
        {
            await CreateAccountAsync(new RegisterInput
            {
                Username = username,
                Email = email,
                Password = password
            });
        }

        public async Task SendUsernameReminder(string email)
        {
            if (Settings.EmailIsUnique == false)
            {
                throw new InvalidOperationException("SendUsernameReminder can't be used when EmailIsUnique is false");
            }

            _logger.LogInformation(GetLogMessage($"called: {email}"));
           
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogError(GetLogMessage("failed -- null email"));
                throw new ValidationException(GetValidationMessage(UserAccountConstants.ValidationMessages.InvalidEmail));
            }

            var account = await GetByEmailAsync(email);
            if (account == null) throw new ValidationException(GetValidationMessage(UserAccountConstants.ValidationMessages.InvalidEmail));

            if (!account.IsAccountVerified)
            {
                _logger.LogError(GetLogMessage("failed -- account not verified"));
                throw new ValidationException(GetValidationMessage(UserAccountConstants.ValidationMessages.AccountNotVerified));
            }

            _logger.LogTrace(GetLogMessage("success"));

            AddEvent(new UsernameReminderRequestedEvent { Account = account });

            Update(account, true);
        }

        public async Task ChangeUsernameAsync(int accountId, string newUsername)
        {
            _logger.LogInformation(GetLogMessage(" called: {accountId}, {newUsername}"));

            if (!_settings.Auth.EmailIsUsername)
            {
                const string error = "SecuritySettings - EmailIsUsername is true, use ChangeEmail API instead";
                _logger.LogTrace(GetLogMessage(error));
                throw new ValidationException(error);
            }

            if (string.IsNullOrWhiteSpace(newUsername))
            {
                var error = "Null username";
                _logger.LogTrace(GetLogMessage(error));
                throw new ValidationException(error);
            }

            var account = await GetByIdAsync(accountId);
            if (account == null) throw new ArgumentException("Invalid Account Id");

            ValidateUsername(account, newUsername);

            var oldUsername = account.Username;

            account.Username = newUsername;
            AddEvent(new UsernameChangedEvent { Account = account, OldUsername = oldUsername, NewUsername = oldUsername });
            Update(account);
            
            _logger.LogTrace(GetLogMessage("Success"));
        }

        public async Task ChangeEmailRequestAsync(int accountId, string newEmail)
        {
            _logger.LogInformation(GetLogMessage($" called: {accountId}, {newEmail}"));

            var account = await GetByIdAsync(accountId);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            ValidateEmail(account, newEmail);

            var oldEmail = account.Email;

            _logger.LogTrace(GetLogMessage("creating a new reset key"));
            var key = SetVerificationKey(account, VerificationKeyPurpose.ChangeEmail, state: newEmail);

            if (!_settings.Auth.RequireAccountVerification)
            {
                _logger.LogTrace(GetLogMessage(" RequireAccountVerification false, changing email"));
                account.IsAccountVerified = false;
                account.Email = newEmail;
                AddEvent(new EmailChangedEvent { Account = account, OldEmail = oldEmail, VerificationKey = key });
            }
            else
            {
                _logger.LogTrace(GetLogMessage("RequireAccountVerification true, sending changing email"));
                AddEvent(new EmailChangeRequestedEvent { Account = account, OldEmail = oldEmail, NewEmail = newEmail, VerificationKey = key });
            }

            Update(account, true);

            _logger.LogTrace(GetLogMessage("Success"));
        }

        public async Task ChangeMobilePhoneRequestAsync(int accountId, string newMobilePhoneNumber)
        {
            _logger.LogInformation(GetLogMessage($" called: {accountId}, {newMobilePhoneNumber}"));
          
            if (string.IsNullOrWhiteSpace(newMobilePhoneNumber))
            {
                var error = "Invalid Mobile Phone Number. It can't be null";
                _logger.LogError(GetLogMessage(error));
                throw new ValidationException(error);
            }

            var account = await GetByIdAsync(accountId);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            ValidatePhoneNumber(account, newMobilePhoneNumber);

            var oldPhoneNumber = account.MobilePhoneNumber;

            if (!IsVerificationPurposeValid(account, VerificationKeyPurpose.ChangeMobile) ||
              CanResendMobileCode(account) ||
              newMobilePhoneNumber != account.VerificationStorage ||
              account.CurrentTwoFactorAuthStatus == TwoFactorAuthMode.Mobile)
            {
                ClearMobileAuthCode(account);

                SetVerificationKey(account, VerificationKeyPurpose.ChangeMobile, state: newMobilePhoneNumber);
                var code = IssueMobileCode(account);

                _logger.LogTrace(GetLogMessage("success"));

                AddEvent(new MobilePhoneChangeRequestedEvent { Account = account, NewMobilePhoneNumber = newMobilePhoneNumber, Code = code });
            }
            else
            {
                _logger.LogTrace(GetLogMessage("complete, but not issuing a new code"));
            }

            Update(account, true);

            _logger.LogTrace(GetLogMessage("success"));
        }

        public async Task ChangePasswordAsync(int accountId, string oldPassword, string newPassword)
        {
            _logger.LogInformation(GetLogMessage($" called - {accountId}"));

            var account = await GetByIdAsync(accountId);
            if (account == null) throw new ArgumentException("Invalid Account");

            if (!VerifyPassword(account, oldPassword))
            {
                const string error = "Invalid Old Password";
                _logger.LogError(GetLogMessage($"failed - {error}"));
                throw new ValidationException(error);
                //GetValidationMessage(UserAccountConstants.ValidationMessages.InvalidOldPassword
            }

            ValidatePassword(account, newPassword);

            SetPassword(account, newPassword);

            _logger.LogTrace(GetLogMessage("success"));

            Update(account);
            
        }

        public async Task<string> RequestVerificationAsync(string email)
        {
            string error;
            _logger.LogInformation(GetLogMessage($" called for : {email}"));

            if (string.IsNullOrWhiteSpace(email))
            {
                error = "No email to use for account verification request";
                _logger.LogError(GetLogMessage(error));
                return error;
                //throw new ValidationException(error);
                //GetValidationMessage(UserAccountConstants.ValidationMessages.PasswordResetErrorNoEmail)
            }

            var account = await GetByEmailAsync(email);
            if (account == null)
            {
                error = "Invalid Account email";
                _logger.LogError(GetLogMessage(error));
                //throw new ValidationException(error);
                return error;
            }

            if (account.IsAccountVerified)
            {
                error = "Account already verified";
                // instead request an initial account verification
                _logger.LogError(GetLogMessage(error));
                return error;
            }

            _logger.LogInformation(GetLogMessage("Creating a new verification key"));

            var key = SetVerificationKey(account, VerificationKeyPurpose.ChangeEmail, state: account.Email);
            AddEvent(new EmailChangeRequestedEvent { Account = account, VerificationKey = key });

            Update(account, true);

            return string.Empty;
        }

        public async Task<string> VerifyEmailFromKeyAsync(string key)
        {
            return await VerifyEmailFromKeyAsync(key, null);
        }

        public async Task<string> VerifyEmailFromKeyAsync(string key, string password)
        {
            string error;

            if (string.IsNullOrWhiteSpace(key))
            {
                error = "Verification key is missing";
                _logger.LogError(GetLogMessage($"failed - {error}"));
                return error;
                throw new ValidationException(""); //GetValidationMessage(UserAccountConstants.ValidationMessages.InvalidKey)
            }

            var account = await GetByVerificationKeyAsync(key);
            if (account == null)
            {
                error = "Invalid verification key";
                _logger.LogError(GetLogMessage($"failed - {error}"));
                return error;
                throw new ValidationException(""); //GetValidationMessage(UserAccountConstants.ValidationMessages.InvalidKey)
            }

            _logger.LogInformation(GetLogMessage($" Account located: id: {account.UserId}"));

            if (!IsVerificationKeyValid(account, VerificationKeyPurpose.ChangeEmail, key))
            {
                error = "Key verification failed";
                _logger.LogError(GetLogMessage($"failed - {error}"));
                return error;
                throw new ValidationException(""); //GetValidationMessage(UserAccountConstants.ValidationMessages.InvalidKey)
            }

            if (string.IsNullOrWhiteSpace(account.VerificationStorage))
            {
                error = "Verification storage empty";
                _logger.LogError(GetLogMessage($"failed - {error}"));
                return error;
                //throw new ValidationException(GetValidationMessage(UserAccountConstants.ValidationMessages.InvalidKey));
            }

            // one last check
            ValidateEmail(account, account.VerificationStorage);

            var isNewAccount = account.IsNew();
            account.Email = account.VerificationStorage;
            account.IsAccountVerified = true;
            account.LastLogin = UtcNow;

            ClearVerificationKey(account);

            AddEvent(new EmailVerifiedEvent { Account = account, IsNewAccount = isNewAccount });

            Update(account);

            _logger.LogTrace(GetLogMessage("success"));

            return string.Empty;
        }

        public async Task<string> CancelVerificationAsync(string key)
        {
            string error;
            _logger.LogInformation(GetLogMessage($" called for : {key}"));

            if (string.IsNullOrWhiteSpace(key))
            {
                error = "No email to use for account verification request";
                _logger.LogError(GetLogMessage(error));
                return error;
                //throw new ValidationException(error);
                //GetValidationMessage(UserAccountConstants.ValidationMessages.PasswordResetErrorNoEmail)
            }

            var account = await GetByVerificationKeyAsync(key);
            if (account == null)
            {
                error = "Account not found from key";
                _logger.LogError(GetLogMessage(error));
                //throw new ValidationException(error);
                return error;
            }

            if (!IsVerificationKeyValid(account, VerificationKeyPurpose.ChangeEmail, key))
            {
                error = "Key verification failed";
                _logger.LogError(GetLogMessage($"failed - {error}"));
                return error;
                throw new ValidationException(""); //GetValidationMessage(UserAccountConstants.ValidationMessages.InvalidKey)
            }

            if (account.VerificationPurpose == VerificationKeyPurpose.ChangeEmail &&
               account.IsNew())
            {
                _logger.LogInformation(GetLogMessage("Account is new (deleting account)"));
                // if last login is null then they've never logged in so we can delete the account
                await DeleteAccount(account);
            }
            else
            {
                _logger.LogInformation(GetLogMessage("Account is not new (canceling clearing verification key)"));
                ClearVerificationKey(account);
                Update(account, true);
            }

            _logger.LogInformation(GetLogMessage("Succeeded"));

            return string.Empty;
        }

        public async Task ResetPasswordAsync(string email)
        {
            _logger.LogInformation(GetLogMessage($" called - {email}"));

            if (string.IsNullOrWhiteSpace(email))
            {
                const string error = "Null email";
                //GetValidationMessage(UserAccountConstants.ValidationMessages.InvalidEmail)
                _logger.LogError(GetLogMessage($"failed - {error}"));
                throw new ValidationException(error);
            }

            var account = await GetByEmailAsync(email);
            if (account == null) throw new ArgumentException("Invalid Email");

            ResetPassword(account);

            Update(account, true);
        }

        public async Task ResetPasswordAsync(Guid accountId)
        {
            _logger.LogInformation(GetLogMessage($" called - {accountId}"));          

            var account = await GetByGuidAsync(accountId);
            if (account == null) throw new ArgumentException("Invalid Email");

            ResetPassword(account);
            Update(account, true);
        }

        public async Task ResetFailedLoginCount(Guid accountId)
        {
            _logger.LogInformation(GetLogMessage($" called - {accountId}"));

            var account = await GetByGuidAsync(accountId);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            account.FailedLoginCount = 0;

            Update(account, true);

            _logger.LogTrace(GetLogMessage("Success"));
        }

        public async Task<UserAccount> ChangePasswordFromResetKeyAsync(string key, string newPassword)
        {
            _logger.LogInformation(GetLogMessage($" called - {key}"));

            var returnError = "Error changing password. The key might be invalid";

            var error = string.Empty;
            if (string.IsNullOrWhiteSpace(key))
            {
                error = GetValidationMessage(UserAccountConstants.ValidationMessages.InvalidEmail);
                _logger.LogError(GetLogMessage($" Failed - {error}"));
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                error = GetValidationMessage(UserAccountConstants.ValidationMessages.InvalidEmail);
                _logger.LogError(GetLogMessage($" Failed - {error}"));
            }

            if (!string.IsNullOrEmpty(error))
            {
                throw new ValidationException(returnError);
            }

            var account = await GetByVerificationKeyAsync(key);
            if (account == null) throw new ArgumentException("Invalid Verification Key");

            ValidatePassword(account, newPassword);

            if (!account.IsAccountVerified)
            {
                _logger.LogError(GetLogMessage("Failed -- Account not verified"));
                throw new ValidationException(returnError);
            }

            if (!IsVerificationKeyValid(account, VerificationKeyPurpose.ResetPassword, key))
            {
                _logger.LogError(GetLogMessage("Failed -- Key verification failed"));
                throw new ValidationException(returnError);
            }

            ClearVerificationKey(account);
            SetPassword(account, newPassword);
            await UpdateAsync(account);

            return account;
        }

        private async Task DeleteAccount(UserAccount account)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));
            _logger.LogInformation(GetLogMessage($" marking account as closed - {account.UserId}"));

            if (Settings.AllowAccountDeletion || account.IsNew())
            {
                _logger.LogInformation(GetLogMessage($" removing account record - {account.UserId}"));
                await Repository.DeleteAsync(account, true);
            }
            else
            {
                CloseAccount(account);
                Update(account);
            }
        }

        private void CloseAccount(UserAccount account)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            _logger.LogInformation(GetLogMessage($" called for accountId - {account.UserId}"));

            ClearVerificationKey(account);
            ClearMobileAuthCode(account);
            ConfigureTwoFactorAuthentication(account, TwoFactorAuthMode.None);

            if (!account.IsAccountClosed)
            {
                _logger.LogInformation(GetLogMessage(" success"));

                account.IsAccountClosed = true;
                account.AccountClosed = UtcNow;

                AddEvent(new AccountClosedEvent { Account = account });
            }
            else
            {
                _logger.LogWarning(GetLogMessage(" account already closed"));
            }
        }

        public async Task DeleteAccountAsync(Guid accountId)
        {
            _logger.LogInformation(GetLogMessage($" called - {accountId}"));

            var account = await GetByGuidAsync(accountId);
            if (account == null) throw new ArgumentException("Invalid AccountId");

            await DeleteAccount(account);
        }

        public async Task CloseAccountAsync(Guid accountId)
        {
            _logger.LogInformation(GetLogMessage($" called - {accountId}"));

            var account = await GetByGuidAsync(accountId);
            if (account == null) throw new ArgumentException("Invalid AccountId");

            CloseAccount(account);
        }

        public async Task ReopenAccountAsync(string username, string password)
        {
            _logger.LogInformation(GetLogMessage($" called - {username}"));

            var account = await GetByUsernameAsync(username);
            if (account == null) throw new ArgumentException("Invalid username");

            if (!VerifyPassword(account, password))
            {
                var error = "Invalid password";
                _logger.LogError(GetLogMessage(error));
                throw new ValidationException(error);
                //GetValidationMessage(AppConstants.ValidationMessages.InvalidPassword)
            }
            await ReopenAccount(account);
        }


        public async Task ReopenAccountAsync(int accountId)
        {
            _logger.LogInformation(GetLogMessage($" called - {accountId}"));

            var account = await GetByIdAsync(accountId);
            if (account == null) throw new ArgumentException("Invalid username");

            await ReopenAccount(account);
        }

        private async Task ReopenAccount(UserAccount account)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            _logger.LogInformation(GetLogMessage($" called - {account.UserId}"));

            if (!account.IsAccountClosed)
            {
                var error = "Account is not closed";
                _logger.LogError(GetLogMessage(error));
                throw new ValidationException(error);
            }

            if (string.IsNullOrWhiteSpace(account.Email))
            {
                var error = "No email to confirm reopen request";
                _logger.LogError(GetLogMessage(error));
                throw new ValidationException(error);
                //GetValidationMessage(AppConstants.ValidationMessages.ReopenErrorNoEmail)
            }

            // this will require the user to confirm via email before logging in
            account.IsAccountVerified = false;
            ClearVerificationKey(account);
            var key = SetVerificationKey(account, VerificationKeyPurpose.ChangeEmail, state: account.Email);
            AddEvent(new AccountReopenedEvent { Account = account, VerificationKey = key });

            account.IsAccountClosed = false;
            account.AccountClosed = null;

            await UpdateAsync(account);

            _logger.LogInformation(GetLogMessage("Success"));

        }

        protected virtual bool IsVerificationKeyValid(UserAccount account, VerificationKeyPurpose purpose, string key)
        {
            if (!IsVerificationPurposeValid(account, purpose))
            {
                return false;
            }

            var result = _crypto.VerifyHash(key, account.VerificationKey);
            if (!result)
            {
                _logger.LogWarning(GetLogMessage("failed -verification key doesn\'t match"));
                return false;
            }

            _logger.LogTrace(GetLogMessage("Success -- verification key valid"));
            return true;
        }

        protected virtual bool IsVerificationPurposeValid(UserAccount account, VerificationKeyPurpose purpose)
        {
            if (account.VerificationPurpose != purpose)
            {
                _logger.LogWarning(GetLogMessage("failed - verification purpose invalid"));
                return false;
            }

            if (IsVerificationKeyStale(account))
            {
                _logger.LogWarning(GetLogMessage("failed - verification key stale"));
                return false;
            }

            _logger.LogTrace(GetLogMessage(" success - verification purpose valid"));

            return true;
        }

        protected virtual bool IsVerificationKeyStale(UserAccount account)
        {
            if (account.VerificationKeySent == null)
            {
                return true;
            }
            return account.VerificationKeySent < UtcNow.Subtract(_settings.Auth.VerificationKeyLifetime);
        }

        protected virtual void ClearVerificationKey(UserAccount account)
        {
            account.VerificationKey = null;
            account.VerificationPurpose = null;
            account.VerificationKeySent = null;
            account.VerificationStorage = null;
        }

        protected virtual void SetPassword(UserAccount account, string password)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            _logger.LogInformation(GetLogMessage($" called for accountId: {account.UserId}"));

            if (string.IsNullOrWhiteSpace(password))
            {
                _logger.LogError(GetLogMessage("failed -- no password provided"));
                throw new ValidationException(); //GetValidationMessage(UserAccountConstants.ValidationMessages.InvalidPassword)
            }

            _logger.LogTrace(GetLogMessage(" Setting new password hash"));

            account.HashedPassword = _crypto.HashPassword(password, _settings.Auth.PasswordHashingIterationCount);
            account.PasswordChanged = UtcNow;
            account.RequiresPasswordReset = false;

            AddEvent(new PasswordChangedEvent { Account = account, NewPassword = password });
        }

        protected virtual void ResetPassword(UserAccount account)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            _logger.LogInformation(GetLogMessage($" called for accountId: {account.UserId}"));

            if (string.IsNullOrWhiteSpace(account.Email))
            {
                const string error = "No email to use for password reset";
                _logger.LogError(GetLogMessage(error));
                throw new ValidationException(error);
                //GetValidationMessage(UserAccountConstants.ValidationMessages.PasswordResetErrorNoEmail)
            }

            if (!account.IsAccountVerified)
            {
                // if they've not yet verified then don't allow password reset
                if (account.IsNew())
                {
                    // instead request an initial account verification
                    _logger.LogWarning(GetLogMessage($" account not verified - raising account created email event to resend initial email: {account.Email}"));
                    var key = SetVerificationKey(account, VerificationKeyPurpose.ChangeEmail, state: account.Email);
                    AddEvent(new AccountCreatedEvent { Account = account, VerificationKey = key });
                }
                else
                {
                    _logger.LogWarning(GetLogMessage($" account not verified - raising change email event to resend email verification: {account.Email}"));
                    var key = SetVerificationKey(account, VerificationKeyPurpose.ChangeEmail, state: account.Email);
                    AddEvent(new EmailChangeRequestedEvent { Account = account, NewEmail = account.Email, VerificationKey = key });
                }
            }
            else
            {
                _logger.LogTrace(GetLogMessage("creating new verification keys"));
                var key = SetVerificationKey(account, VerificationKeyPurpose.ResetPassword);

                _logger.LogTrace(GetLogMessage("raising event to send reset notification"));
                AddEvent(new PasswordResetRequestedEvent { Account = account, VerificationKey = key });
            }
        }

        protected virtual string IssueMobileCode(UserAccount account)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            var code = _crypto.GenerateNumericCode(_settings.Auth.MobileVerificationCodeLength);
            account.MobileCode = _crypto.HashPassword(code, _settings.Auth.PasswordHashingIterationCount);
            account.MobileCodeSent = UtcNow;

            return code;
        }

        protected void Init(UserAccount account, string username, string password, string email)
        {
            _logger.LogInformation(GetLogMessage("called"));

            if (account == null)
            {
                _logger.LogError(GetLogMessage("failed -- null account"));
                throw new ArgumentNullException(nameof(account));
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                _logger.LogError(GetLogMessage("failed -- no username"));
                throw new ValidationException("UsernameRequired");
            }

            if (password != null && string.IsNullOrWhiteSpace(password.Trim()))
            {
                _logger.LogError(GetLogMessage("failed -- no password"));
                throw new ValidationException("PasswordRequired");
            }

            account.Guid = Guid.NewGuid();
            account.Username = username;
            account.Email = email;
            account.HashedPassword = password != null ?
                _crypto.HashPassword(password, _settings.Auth.PasswordHashingIterationCount) : null;
            account.PasswordChanged = password != null ? UtcNow : (DateTimeOffset?)null;
            account.IsAccountVerified = false;
            account.AccountTwoFactorAuthMode = TwoFactorAuthMode.None;
            account.CurrentTwoFactorAuthStatus = TwoFactorAuthMode.None;

            account.IsLoginAllowed = _settings.Auth.AllowLoginAfterAccountCreation;
            _logger.LogTrace(GetLogMessage($" SecuritySettings.AllowLoginAfterAccountCreation is set to: {account.IsLoginAllowed}"));

            string key = null;
            if (!string.IsNullOrWhiteSpace(account.Email))
            {
                _logger.LogTrace(GetLogMessage("Email was provided, so creating email verification request"));
                key = SetVerificationKey(account, VerificationKeyPurpose.ChangeEmail, state: account.Email);
            }

            AddEvent(new AccountCreatedEvent
            {
                Account = account,
                InitialPassword = password,
                VerificationKey = key
            });
        }

        protected virtual string SetVerificationKey(UserAccount account, VerificationKeyPurpose purpose, string key = null, string state = null)
        {
            if (key == null) key = StripUglyBase64(_crypto.GenerateSalt());

            account.VerificationKey = _crypto.Hash(key);
            account.VerificationPurpose = purpose;
            account.VerificationKeySent = UtcNow;
            account.VerificationStorage = state;

            return key;
        }


        public async Task<bool> ChangeMobilePhoneFromCodeAsync(int accountId, string code)
        {
            _logger.LogInformation(GetLogMessage($"called: {accountId}"));

            if (string.IsNullOrWhiteSpace(code))
            {
                _logger.LogError(GetLogMessage("failed -- null code"));
                throw new ValidationException(GetValidationMessage(UserAccountConstants.ValidationMessages.CodeRequired));
            }

            var account = await GetByIdAsync(accountId);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            _logger.LogInformation(GetLogMessage($"called for accountId: {account.UserId}"));

            if (account.VerificationPurpose != VerificationKeyPurpose.ChangeMobile)
            {
                _logger.LogError(GetLogMessage("failed -- invalid verification key purpose"));
                return false;
            }

            if (!VerifyMobileCode(account, code))
            {
                _logger.LogError(GetLogMessage("failed -- mobile code failed to verify"));
                return false;
            }

            var newMobile = account.VerificationStorage;
            if (MobilePhoneExistsOtherThan(account, newMobile))
            {
                _logger.LogTrace(GetLogMessage("failed -- number already in use"));
                throw new ValidationException(GetValidationMessage(UserAccountConstants.ValidationMessages.MobilePhoneAlreadyInUse));
            }

            _logger.LogTrace(GetLogMessage("success"));

            account.MobilePhoneNumber = newMobile;
            account.MobilePhoneNumberChanged = UtcNow;

            ClearVerificationKey(account);
            ClearMobileAuthCode(account);

            AddEvent(new MobilePhoneChangedEvent { Account = account });

            Update(account);

            return true;
        }

        public async Task SetConfirmedMobilePhoneAsync(int accountId, string phone)
        {
            _logger.LogInformation(GetLogMessage($"called: {accountId}, {phone}"));

            if (string.IsNullOrWhiteSpace(phone))
            {
                _logger.LogError(GetLogMessage("failed -- null phone"));
                throw new ValidationException(GetValidationMessage(UserAccountConstants.ValidationMessages.MobilePhoneRequired));
            }

            var account = await GetByIdAsync(accountId);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            if (account.MobilePhoneNumber == phone)
            {
                _logger.LogError(GetLogMessage("mobile phone same as current"));
                throw new ValidationException(GetValidationMessage(UserAccountConstants.ValidationMessages.MobilePhoneMustBeDifferent));
            }

            if (MobilePhoneExistsOtherThan(account, phone))
            {
                _logger.LogTrace(GetLogMessage("failed -- number already in use"));
                throw new ValidationException(GetValidationMessage(UserAccountConstants.ValidationMessages.MobilePhoneAlreadyInUse));
            }

            account.MobilePhoneNumber = phone;
            account.MobilePhoneNumberChanged = UtcNow;

            ClearVerificationKey(account);
            ClearMobileAuthCode(account);

            AddEvent(new MobilePhoneChangedEvent { Account = account });

            Update(account);

            _logger.LogTrace(GetLogMessage("success"));
        }

        public async Task SetConfirmedEmailAsync(int accountId, string email)
        {
            _logger.LogInformation(GetLogMessage($"called: {accountId}, {email}"));

            var account = await GetByIdAsync(accountId);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            ValidateEmail(account, email);

            account.IsAccountVerified = true;
            account.Email = email;

            ClearVerificationKey(account);

            AddEvent(new EmailVerifiedEvent { Account = account });

            if (Settings.EmailIsUsername)
            {
                _logger.LogTrace(GetLogMessage($"security setting EmailIsUsername is true, so changing username: {account.Username}, to: {account.Email}"));
                account.Username = account.Email;
            }

            Update(account);

            _logger.LogTrace(GetLogMessage("success"));
        }

        public async Task<bool> IsPasswordExpiredAsync(int accountId)
        {
            var account = await GetByIdAsync(accountId);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            return IsPasswordExpired(account);
        }

        public bool IsPasswordExpired(UserAccount account)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            _logger.LogInformation(GetLogMessage($"called: {account.UserId}"));

            if (Settings.PasswordResetFrequency <= 0)
            {
                _logger.LogTrace(GetLogMessage("PasswordResetFrequency not set, returning false"));
                return false;
            }

            if (!account.HasPassword())
            {
                _logger.LogTrace(GetLogMessage("HashedPassword is null, returning false"));
                return false;
            }

            if (account.PasswordChanged == null)
            {
                _logger.LogWarning(GetLogMessage("PasswordChanged is null, returning false"));
                return false;
            }

            var now = UtcNow;
            var last = account.PasswordChanged.Value;
            var result = last.AddDays(Settings.PasswordResetFrequency) <= now;

            _logger.LogTrace(GetLogMessage($"result: {result}"));

            return result;
        }

        public async Task RemoveMobilePhoneAsync(int accountId)
        {
            _logger.LogInformation(GetLogMessage($"called: {accountId}"));

            var account = await GetByIdAsync(accountId);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            _logger.LogInformation(GetLogMessage($"called for accountId: {account.UserId}"));

            if (account.AccountTwoFactorAuthMode == TwoFactorAuthMode.Mobile)
            {
                _logger.LogTrace(GetLogMessage("disabling two factor auth"));
                ConfigureTwoFactorAuthentication(account, TwoFactorAuthMode.None);
            }

            if (string.IsNullOrWhiteSpace(account.MobilePhoneNumber))
            {
                _logger.LogWarning(GetLogMessage("nothing to do -- no mobile associated with account"));
                return;
            }

            _logger.LogTrace(GetLogMessage("success"));

            ClearMobileAuthCode(account);

            account.MobilePhoneNumber = null;
            account.MobilePhoneNumberChanged = UtcNow;

            AddEvent(new MobilePhoneRemovedEvent { Account = account });

            Update(account);
        }

        protected virtual bool VerifyMobileCode(UserAccount account, string code)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));
            if (string.IsNullOrWhiteSpace(code)) return false;

            if (IsMobileCodeExpired(account))
            {
                _logger.LogError(GetLogMessage("failed -- mobile code stale"));
                return false;
            }

            try
            {
                if (CheckHasTooManyRecentPasswordFailures(account))
                {
                    _logger.LogError(GetLogMessage("failed -- TooManyRecentPasswordFailures"));
                    return false;
                }

                var result = _crypto.VerifyHashedPassword(account.MobileCode, code);
                if (!result)
                {
                    RecordInvalidLoginAttempt(account);
                    _logger.LogError(GetLogMessage("failed -- mobile code invalid"));
                    return false;
                }

                account.FailedLoginCount = 0;

                _logger.LogTrace(GetLogMessage("success -- mobile code valid"));
                return true;
            }
            finally
            {
                Update(account, true);
            }
        }

        protected internal bool MobilePhoneExistsOtherThan(UserAccount account, string phone)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            _logger.LogInformation(GetLogMessage($"called for account id: {account.UserId}, phone; {phone}"));

            if (string.IsNullOrWhiteSpace(phone)) return false;

            var acct2 = GetByMobilePhone(phone);
            if (acct2 != null)
            {
                return account.UserId != acct2.UserId;
            }
            return false;
        }

        public async Task SetIsLoginAllowed(Guid accountId, bool isLoginAllowed)
        {
            _logger.LogInformation(GetLogMessage($"called: {accountId}"));

            var account = await GetByGuidAsync(accountId);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            var originalIsLoginAllowed = account.IsLoginAllowed;
            account.IsLoginAllowed = isLoginAllowed;

            _logger.LogTrace(GetLogMessage("success"));

            if (!originalIsLoginAllowed && isLoginAllowed)
            {
                AddEvent(new AccountUnlockedEvent { Account = account });
            }

            Update(account);
        }

        public async Task SetRequiresPasswordReset(Guid accountId, bool requiresPasswordReset)
        {
            _logger.LogInformation(GetLogMessage($"called: {accountId}"));

            var account = await GetByGuidAsync(accountId);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            account.RequiresPasswordReset = requiresPasswordReset;

            _logger.LogTrace(GetLogMessage("success"));

            Update(account);
        }

        public async Task SetPassword(Guid accountId, string newPassword)
        {
            _logger.LogInformation(GetLogMessage($"called: {accountId}"));

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                _logger.LogError(GetLogMessage("failed -- null newPassword"));
                throw new ValidationException(GetValidationMessage(UserAccountConstants.ValidationMessages.InvalidNewPassword));
            }

            var account = await GetByGuidAsync(accountId);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            ValidatePassword(account, newPassword);
            SetPassword(account, newPassword);
            Update(account);

            _logger.LogTrace(GetLogMessage("success"));
        }

        private static readonly string[] UglyBase64 = { "+", "/", "=" };

        protected virtual string StripUglyBase64(string s)
        {
            if (s == null) return null;
            foreach (var ugly in UglyBase64)
            {
                s = s.Replace(ugly, "");
            }
            return s;
        }
    }
}
