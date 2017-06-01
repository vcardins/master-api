using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using MasterApi.Core.Account.Models;
using System.Linq;
using MasterApi.Core.Account.Events;

namespace MasterApi.Services.Account
{
    public partial class UserAccountService 
    {
        public void AddClaims(int accountId, UserClaimCollection claims)
        {
            _logger.LogInformation(GetLogMessage($"called for accountId: {accountId}"));
            UpdateClaims(accountId, claims);
        }

        public void RemoveClaims(int accountId, UserClaimCollection claims)
        {
            _logger.LogInformation(GetLogMessage($"called for accountId: {accountId}"));
            UpdateClaims(accountId, null, claims);
        }

        public void UpdateClaims(int accountId, UserClaimCollection additions = null, UserClaimCollection deletions = null)
        {
            _logger.LogInformation(GetLogMessage($"called for accountId: {accountId}"));

            if ((additions == null || !additions.Any()) &&
                (deletions == null || !deletions.Any()))
            {
                _logger.LogInformation(GetLogMessage("no additions or deletions -- exiting"));
                return;
            }

            var account = GetById(accountId);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            foreach (var addition in additions ?? UserClaimCollection.Empty)
            {
                AddClaim(account, addition);
            }
            foreach (var deletion in deletions ?? UserClaimCollection.Empty)
            {
                RemoveClaim(account, deletion.Type, deletion.Value);
            }
            Update(account);
        }

        public void AddClaim(int accountId, string type, string value)
        {
            _logger.LogInformation(GetLogMessage($"called for accountId: {accountId}"));

            if (string.IsNullOrWhiteSpace(type))
            {
                _logger.LogError(GetLogMessage("failed -- null type"));
                throw new ArgumentException("type");
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                _logger.LogError(GetLogMessage("failed -- null value"));
                throw new ArgumentException("value");
            }

            var account = GetById(accountId);
            if (account == null) throw new ArgumentException("Invalid AccountID", nameof(accountId));

            AddClaim(account, new UserClaim(type, value));
            Update(account);
        }

        private void AddClaim(UserAccount account, UserClaim claim)
        {
            if (claim == null) throw new ArgumentNullException(nameof(claim));

            if (account.HasClaim(claim.Type, claim.Value)) return;
            account.AddClaim(claim);
            AddEvent(new ClaimAddedEvent { Account = account, Claim = claim });

            _logger.LogTrace(GetLogMessage("claim added"));
        }

        public void RemoveClaim(int accountId, string type)
        {
            _logger.LogInformation(GetLogMessage($"called for accountId: {accountId}"));

            var account = GetById(accountId);
            if (account == null) throw new ArgumentException("Invalid AccountID", nameof(accountId));

            if (string.IsNullOrWhiteSpace(type))
            {
                _logger.LogError(GetLogMessage("failed -- null type"));
                throw new ArgumentException("type");
            }

            var claimsToRemove =
                from claim in account.ClaimCollection
                where claim.Type == type
                select claim;

            foreach (var claim in claimsToRemove.ToArray())
            {
                account.RemoveClaim(claim);
                AddEvent(new ClaimRemovedEvent { Account = account, Claim = claim });
                _logger.LogTrace(GetLogMessage("claim removed"));
            }

            Update(account);
        }

        public void RemoveClaim(int accountId, string type, string value)
        {
            _logger.LogInformation(GetLogMessage($"called for accountId: {accountId}"));

            var account = GetById(accountId);
            if (account == null) throw new ArgumentException("Invalid AccountID", nameof(accountId));

            RemoveClaim(account, type, value);
            Update(account);
        }

        private void RemoveClaim(UserAccount account, string type, string value)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                _logger.LogError(GetLogMessage("failed -- null type"));
                throw new ArgumentException("type");
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                _logger.LogError(GetLogMessage("failed -- null value"));
                throw new ArgumentException("value");
            }

            var claimsToRemove =
                from claim in account.ClaimCollection
                where claim.Type == type && claim.Value == value
                select claim;
            foreach (var claim in claimsToRemove.ToArray())
            {
                account.RemoveClaim(claim);
                AddEvent(new ClaimRemovedEvent { Account = account, Claim = claim });
                _logger.LogDebug(GetLogMessage("claim removed"));
            }
        }

        public IEnumerable<Claim> MapClaims(UserAccount account)
        {
            throw new NotImplementedException();
        }
    }
}
