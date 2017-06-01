using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using MasterApi.Core.Account.Models;

namespace MasterApi.Services.Account
{
    public static class UserAccountExtensions
    {
        public static bool HasClaim(this UserAccount account, string type)
        {
            if (account == null) throw new ArgumentException("account");
            if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");

            return account.ClaimCollection.Any(x => x.Type == type);
        }

        public static bool HasClaim(this UserAccount account, string type, string value)
        {
            if (account == null) throw new ArgumentException("account");
            if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("value");

            return account.ClaimCollection.Any(x => x.Type == type && x.Value == value);
        }

        public static IEnumerable<string> GetClaimValues(this UserAccount account, string type)
        {
            if (account == null) throw new ArgumentException("account");
            if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");

            var query =
                from claim in account.ClaimCollection
                where claim.Type == type
                select claim.Value;
            return query.ToArray();
        }

        public static string GetClaimValue(this UserAccount account, string type)
        {
            if (account == null) throw new ArgumentException("account");
            if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");

            var query =
                from claim in account.ClaimCollection
                where claim.Type == type
                select claim.Value;
            return query.SingleOrDefault();
        }

        //public static bool RequiresTwoFactorAuthToSignIn(this UserAccount account)
        //{
        //    if (account == null) throw new ArgumentException("account");
        //    return account.CurrentTwoFactorAuthStatus != TwoFactorAuthMode.None;
        //}

        //public static bool RequiresTwoFactorCertificateToSignIn(this UserAccount account)
        //{
        //    if (account == null) throw new ArgumentException("account");
        //    return
        //        account.AccountTwoFactorAuthMode == TwoFactorAuthMode.Certificate &&
        //        account.CurrentTwoFactorAuthStatus == TwoFactorAuthMode.Certificate;
        //}

        //public static bool RequiresTwoFactorAuthCodeToSignIn(this UserAccount account)
        //{
        //    if (account == null) throw new ArgumentException("account");
        //    return
        //        account.AccountTwoFactorAuthMode == TwoFactorAuthMode.Mobile &&
        //        account.CurrentTwoFactorAuthStatus == TwoFactorAuthMode.Mobile;
        //}

        public static bool HasPassword(this UserAccount account)
        {
            if (account == null) throw new ArgumentException("account");
            return !string.IsNullOrWhiteSpace(account.HashedPassword);
        }

        public static bool IsNew(this UserAccount account)
        {
            if (account == null) throw new ArgumentException("account");
            return account.LastLogin == null;
        }

        public static IEnumerable<Claim> GetIdentificationClaims(this UserAccount account)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, account.UserId.ToString("D")),
                new Claim(ClaimTypes.Name, account.Username)
            };

            return claims;
        }

        public static IEnumerable<Claim> GetAllClaims(this UserAccount account)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            var claims = new List<Claim>();
            claims.AddRange(account.GetIdentificationClaims());

            if (!string.IsNullOrWhiteSpace(account.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, account.Email));
            }
            if (!string.IsNullOrWhiteSpace(account.MobilePhoneNumber))
            {
                claims.Add(new Claim(ClaimTypes.MobilePhone, account.MobilePhoneNumber));
            }

            var otherClaims =
                (from uc in account.ClaimCollection
                 select new Claim(uc.Type, uc.Value)).ToList();
            claims.AddRange(otherClaims);

            return claims;
        }
    }
}
