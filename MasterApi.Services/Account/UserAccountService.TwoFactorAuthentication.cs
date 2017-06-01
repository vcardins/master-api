using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MasterApi.Core.Account.Models;
using MasterApi.Core.Account.Enums;
using MasterApi.Core.Account.Events;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using MasterApi.Core.Account;

namespace MasterApi.Services.Account
{
    public partial class UserAccountService 
    {
        public async Task ConfigureTwoFactorAuthenticationAsync(int accountId, TwoFactorAuthMode mode)
        {
            _logger.LogInformation(GetLogMessage($"called: {accountId}"));

            var account = await GetByIdAsync(accountId, x => x.TwoFactorAuthTokenCollection);
            if (account == null) throw new ArgumentException("Invalid Account Id");

            ConfigureTwoFactorAuthentication(account, mode);
            Update(account);

            _logger.LogInformation(GetLogMessage("Success"));
        }

        public async Task SendTwoFactorAuthenticationCodeAsync(int accountId)
        {
            _logger.LogInformation(GetLogMessage($"called: {accountId}"));

            var account = await GetByIdAsync(accountId, x => x.TwoFactorAuthTokenCollection);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            RequestTwoFactorAuthCode(account, true);

            Update(account, true);
        }

        protected virtual void ConfigureTwoFactorAuthentication(UserAccount account, TwoFactorAuthMode mode)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            _logger.LogInformation(GetLogMessage($"called for accountID: {account.UserId}, mode: {mode}"));

            if (account.AccountTwoFactorAuthMode == mode)
            {
                _logger.LogWarning(GetLogMessage("Nothing to do -- mode is same as current value"));
                return;
            }

            if (mode == TwoFactorAuthMode.Mobile &&
                string.IsNullOrWhiteSpace(account.MobilePhoneNumber))
            {
                var error = GetValidationMessage(UserAccountConstants.ValidationMessages.RegisterMobileForTwoFactor);
                _logger.LogError(GetLogMessage(error));
                throw new ValidationException(error);
            }

            ClearMobileAuthCode(account);

            account.AccountTwoFactorAuthMode = mode;
            account.CurrentTwoFactorAuthStatus = TwoFactorAuthMode.None;

            if (mode == TwoFactorAuthMode.None)
            {
                RemoveTwoFactorAuthTokens(account);

                _logger.LogTrace(GetLogMessage("success -- two factor auth disabled"));
                AddEvent(new TwoFactorAuthenticationDisabledEvent { Account = account });
            }
            else
            {
                _logger.LogTrace(GetLogMessage($"success -- two factor auth enabled, mode: {mode}"));
                AddEvent(new TwoFactorAuthenticationEnabledEvent { Account = account, Mode = mode });
            }
        }

        protected virtual void ClearMobileAuthCode(UserAccount account)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            _logger.LogInformation(GetLogMessage($"Called for account id: {account.UserId}"));

            account.MobileCode = null;
            account.MobileCodeSent = null;
            if (account.CurrentTwoFactorAuthStatus == TwoFactorAuthMode.Mobile)
            {
                account.CurrentTwoFactorAuthStatus = TwoFactorAuthMode.None;
            }
            if (account.VerificationPurpose == VerificationKeyPurpose.ChangeMobile)
            {
                ClearVerificationKey(account);
            }
        }

        protected virtual bool IsMobileCodeOlderThan(UserAccount account, int duration)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            if (account.MobileCodeSent == null || string.IsNullOrWhiteSpace(account.MobileCode))
            {
                return true;
            }

            return account.MobileCodeSent < UtcNow.AddMinutes(-duration);
        }

        protected virtual bool IsMobileCodeExpired(UserAccount account)
        {
            return IsMobileCodeOlderThan(account, Settings.MobileCodeStaleDurationMinutes);
        }

        protected virtual bool CanResendMobileCode(UserAccount account)
        {
            return IsMobileCodeOlderThan(account, Settings.MobileCodeResendDelayMinutes);
        }

        protected virtual bool RequestTwoFactorAuthCode(UserAccount account, bool force = false)
        {
            if (account == null) throw new ArgumentException("Invalid AccountID");
            _logger.LogInformation(GetLogMessage($"called: {account.UserId}"));

            if (account.IsAccountClosed)
            {
                _logger.LogError(GetLogMessage("Account closed"));
                return false;
            }

            if (!account.IsLoginAllowed)
            {
                _logger.LogError(GetLogMessage("Login not allowed"));
                return false;
            }

            if (account.AccountTwoFactorAuthMode != TwoFactorAuthMode.Mobile)
            {
                _logger.LogError(GetLogMessage("Account Two Factor Auth Mode not mobile"));
                return false;
            }

            if (string.IsNullOrWhiteSpace(account.MobilePhoneNumber))
            {
                _logger.LogError(GetLogMessage("Mobile Phone Number is missing"));
                return false;
            }

            if (CanResendMobileCode(account) ||
                account.CurrentTwoFactorAuthStatus != TwoFactorAuthMode.Mobile)
            {
                ClearMobileAuthCode(account);

                _logger.LogInformation(GetLogMessage("New mobile code issued"));

                var code = IssueMobileCode(account);
                account.CurrentTwoFactorAuthStatus = TwoFactorAuthMode.Mobile;

                _logger.LogInformation(GetLogMessage("Success"));

                AddEvent(new TwoFactorAuthenticationCodeNotificationEvent { Account = account, Code = code });
            }
            else
            {
                _logger.LogInformation(GetLogMessage("Success, but not issuing a new code"));
            }

            return true;
        }


        protected virtual void CreateTwoFactorAuthToken(UserAccount account)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            _logger.LogInformation(GetLogMessage($"called for accountID: {account.UserId}"));

            if (account.AccountTwoFactorAuthMode != TwoFactorAuthMode.Mobile)
            {
                _logger.LogError(GetLogMessage("AccountTwoFactorAuthMode is not mobile"));
                throw new Exception("AccountTwoFactorAuthMode is not Mobile");
            }

            var value = _crypto.GenerateSalt();

            //var cmd = new IssueTwoFactorAuthToken { Account = account, Token = value };
            //ExecuteCommand(cmd);
            //if (cmd.Success)
            //{
            //    var item = new TwoFactorAuthToken();
            //    item.Token = _crypto.Hash(value);
            //    item.Issued = UtcNow.DateTime;
            //    account.AddTwoFactorAuthToken(item);

            //    AddEvent(new TwoFactorAuthenticationTokenCreatedEvent { Account = account, Token = value });

            //    _logger.LogInformation(GetLogMessage("TwoFactorAuthToken issued"));
            //}
            //else
            //{
            //    _logger.LogInformation(GetLogMessage("TwoFactorAuthToken not issued"));
            //}
        }

        protected virtual bool VerifyTwoFactorAuthToken(UserAccount account, string token)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            _logger.LogInformation(GetLogMessage($"called for accountID: {account.UserId}"));

            if (account.AccountTwoFactorAuthMode != TwoFactorAuthMode.Mobile)
            {
                _logger.LogError(GetLogMessage("AccountTwoFactorAuthMode is not mobile"));
                return false;
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogError(GetLogMessage("failed -- no token"));
                return false;
            }

            token = _crypto.Hash(token);

            var expiration = UtcNow.AddDays(Settings.TwoFactorAuthTokenDurationDays);
            var removequery =
                from t in account.TwoFactorAuthTokenCollection
                where
                    t.Issued < account.PasswordChanged ||
                    t.Issued < account.MobilePhoneNumberChanged ||
                    t.Issued < expiration
                select t;
            var itemsToRemove = removequery.ToArray();

            _logger.LogInformation(GetLogMessage($"number of stale tokens being removed: {itemsToRemove.Length}"));

            foreach (var item in itemsToRemove)
            {
                account.RemoveTwoFactorAuthToken(item);
            }

            var matchquery =
                from t in account.TwoFactorAuthTokenCollection.ToArray()
                where _crypto.VerifyHash(token, t.Token)
                select t;

            var result = matchquery.Any();

            _logger.LogInformation(GetLogMessage($"result was token verified: {result}"));

            return result;
        }

        protected virtual void RemoveTwoFactorAuthTokens(UserAccount account)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            _logger.LogInformation(GetLogMessage($"called for accountID: {account.UserId}"));

            var tokens = account.TwoFactorAuthTokenCollection.ToArray();
            foreach (var item in tokens)
            {
                account.RemoveTwoFactorAuthToken(item);
            }

            //var cmd = new ClearTwoFactorAuthToken { Account = account };
            //ExecuteCommand(cmd);

            _logger.LogInformation(GetLogMessage($"tokens removed: {tokens.Length}"));
        }
    }
}
