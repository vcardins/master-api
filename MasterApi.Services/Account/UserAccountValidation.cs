using System.ComponentModel.DataAnnotations;
using System.Linq;
using MasterApi.Core.Account.Models;
using MasterApi.Core.Account.Validation;

namespace MasterApi.Services.Account
{
    internal class UserAccountValidation
    {
        public static readonly IValidator<UserAccount> UsernameDoesNotContainAtSign =
            new DelegateValidator((service, account, value) => !value.Contains("@") ? null : new ValidationResult("UsernameCannotContainAtSign"));

        private static readonly char[] SpecialChars = { '.', ' ', '_', '-', '\'' };

        public static bool IsValidUsernameChar(char c)
        {
            return
                char.IsLetterOrDigit(c) ||
                SpecialChars.Contains(c);
        }

        public static readonly IValidator<UserAccount> UsernameOnlySingleInstanceOfSpecialCharacters =
                   new DelegateValidator((service, account, value) =>
                   {
                       foreach (var specialChar in SpecialChars)
                       {
                           var doubleChar = specialChar.ToString() + specialChar.ToString();
                           if (value.Contains(doubleChar))
                           {
                               //_logger.LogTrace("[UserAccountValidation.UsernameOnlySingleInstanceOfSpecialCharacters] validation failed: {0}, {1}, {2}", account.Username, value);
                               return new ValidationResult("UsernameCannotRepeatSpecialCharacters");
                           }
                       }

                       return null;
                   });

        public static readonly IValidator<UserAccount> UsernameOnlyContainsValidCharacters =
            new DelegateValidator((service, account, value) =>
            {
                if (!value.All(IsValidUsernameChar))
                {
                    //_logger.LogTrace("[UserAccountValidation.UsernameOnlyContainsValidCharacters] validation failed: {0}, {1}", account.Username, value);
                    return new ValidationResult("UsernameOnlyContainsValidCharacters");
                }
                return null;
            });

        public static readonly IValidator<UserAccount> UsernameCanOnlyStartOrEndWithLetterOrDigit =
                   new DelegateValidator((service, account, value) =>
                   {
                       if (!char.IsLetterOrDigit(value.First()) || !char.IsLetterOrDigit(value.Last()))
                       {
                           //_logger.LogTrace("[UserAccountValidation.UsernameCanOnlyStartOrEndWithLetterOrDigit] validation failed: {0}, {1}", account.Username, value);
                           return new ValidationResult("UsernameCanOnlyStartOrEndWithLetterOrDigit");
                       }
                       return null;
                   });

        public static readonly IValidator<UserAccount> UsernameMustNotAlreadyExist =
            new DelegateValidator((service, account, value) =>
            {
                if (service.UsernameExists(value))
                {
                    //_logger.LogTrace("[UserAccountValidation.EmailMustNotAlreadyExist] validation failed: {0}, {1}", account.Username, value);
                    return new ValidationResult("UsernameAlreadyInUse");
                }
                return null;
            });

        public static readonly IValidator<UserAccount> EmailRequired =
            new DelegateValidator((service, account, value) =>
            {
                if (service.Settings.RequireAccountVerification &&
                    string.IsNullOrWhiteSpace(value))
                {
                    //_logger.LogTrace("[UserAccountValidation.EmailRequired] validation failed: {0}", account.Username);

                    return new ValidationResult("EmailRequired");
                }
                return null;
            });

        public static readonly IValidator<UserAccount> EmailIsValidFormat =
            new DelegateValidator((service, account, value) =>
            {
                if (string.IsNullOrWhiteSpace(value)) return null;
                var validator = new EmailAddressAttribute();
                return !validator.IsValid(value) ? new ValidationResult("InvalidEmail") : null;
            });

        public static readonly IValidator<UserAccount> EmailIsRequiredIfRequireAccountVerificationEnabled =
            new DelegateValidator((service, account, value) =>
            {
                if (service.Settings.RequireAccountVerification && string.IsNullOrWhiteSpace(value))
                {
                    return new ValidationResult("EmailRequired");
                }
                return null;
            });

        public static readonly IValidator<UserAccount> EmailMustNotAlreadyExist =
            new DelegateValidator((service, account, value) =>
            {
                if (!string.IsNullOrWhiteSpace(value) && service.EmailExistsOtherThan(account, value))
                {
                    //_logger.LogTrace("[UserAccountValidation.EmailMustNotAlreadyExist] validation failed: {0}, {1}", account.Username, value);

                    return new ValidationResult("EmailAlreadyInUse");
                }
                return null;
            });

        public static readonly IValidator<UserAccount> PhoneNumberRequired =
            new DelegateValidator((service, account, value) =>
            {
                if (!service.Settings.RequireAccountVerification || !string.IsNullOrWhiteSpace(value)) return null;
                //_logger.LogTrace("[UserAccountValidation.PhoneNumberRequired] validation failed: {0}", account.Username);

                return new ValidationResult("MobilePhoneRequired");
            });

        public static readonly IValidator<UserAccount> PhoneNumberIsRequiredIfRequireAccountVerificationEnabled =
            new DelegateValidator((service, account, value) =>
            {
                if (service.Settings.RequireAccountVerification && string.IsNullOrWhiteSpace(value))
                {
                    return new ValidationResult("MobilePhoneMustBeDifferent");
                }
                return null;
            });

        public static readonly IValidator<UserAccount> PhoneNumberMustNotAlreadyExist =
            new DelegateValidator((service, account, value) =>
            {
                if (!string.IsNullOrWhiteSpace(value) && service.PhoneNumberExistsOtherThan(account, value))
                {
                    //_logger.LogTrace("[UserAccountValidation.PhoneNumberMustNotAlreadyExist] validation failed: {0}, {1}", account.Username, value);

                    return new ValidationResult("MobilePhoneAlreadyInUse");
                }
                return null;
            });

        public static readonly IValidator<UserAccount> PasswordMustBeDifferentThanCurrent =
            new DelegateValidator((service, account, value) =>
            {
                // Use LastLogin null-check to see if it's a new account
                // we don't want to run this logic if it's a new account
                if (account.IsNew()) return null;
                //_logger.LogTrace("[UserAccountValidation.PasswordMustBeDifferentThanCurrent] validation failed: {0}", account.Username);
                return !service.VerifyHashedPassword(account, value) ? new ValidationResult("NewPasswordMustBeDifferent") : null;
            });
    }
}
