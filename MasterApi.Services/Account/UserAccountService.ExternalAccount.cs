using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using MasterApi.Core.Account.Events;
using MasterApi.Core.Account.Models;

namespace MasterApi.Services.Account
{
    public partial class UserAccountService 
    {
        protected virtual ExternalLogin GetExternalLogin(UserAccount account, string provider, string id)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            _logger.LogInformation(GetLogMessage($"called for account ID: {account.UserId}"));

            return account.ExternalLoginCollection.SingleOrDefault(x => x.LoginProvider == provider && x.ProviderKey == id);
        }


        public virtual void AddOrUpdateExternalLogin(UserAccount account, string provider, string id, IEnumerable<Claim> claims = null)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            _logger.LogInformation(GetLogMessage($"called for accountId: {account.UserId}"));

            if (string.IsNullOrWhiteSpace(provider))
            {
                _logger.LogError(GetLogMessage("failed -- null provider"));
                throw new ArgumentNullException(nameof(provider));
            }
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogError(GetLogMessage("failed -- null id"));
                throw new ArgumentNullException(nameof(id));
            }

            var otherAcct = GetByExternalLogin(provider, id);
            if (otherAcct != null && otherAcct.UserId != account.UserId)
            {
                _logger.LogError(GetLogMessage("failed -- adding linked account that is already associated with another account"));
                throw new ValidationException(GetValidationMessage("ExternalLoginAlreadyInUse"));
            }

            //RemoveExternalLoginClaims(account, provider, id);

            var linked = GetExternalLogin(account, provider, id);
            if (linked == null)
            {
                linked = new ExternalLogin
                {
                    LoginProvider = provider,
                    ProviderKey = id,
                    LastLogin = UtcNow
                };
                account.AddExternalLogin(linked);
                AddEvent(new ExternalLoginAddedEvent { Account = account, ExternalLogin = linked });

                _logger.LogTrace(GetLogMessage("linked account added"));
            }
            else
            {
                linked.LastLogin = UtcNow;
            }

            account.LastLogin = UtcNow;

            Update(account);
        }

        public virtual void RemoveExternalLogin(int accountId, string provider, string id)
        {
            _logger.LogInformation(GetLogMessage("called for account ID: {accountId}"));

            var account = GetById(accountId);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            var linked = GetExternalLogin(account, provider, id);

            if (linked != null && account.ExternalLoginCollection.Count == 1 && !account.HasPassword())
            {
                // can't remove last linked account if no password
                _logger.LogError(GetLogMessage("no password on account -- can't remove last provider"));
                throw new ValidationException("Cant Remove Last External Login If NoPassword");
            }

            if (linked != null)
            {
                account.RemoveExternalLogin(linked);
                AddEvent(new ExternalLoginRemovedEvent { Account = account, ExternalLogin = linked });
                _logger.LogTrace(GetLogMessage("linked account removed"));
            }

            Update(account);
        }       
    }
}
