using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using MasterApi.Core.Data.UnitOfWork;
using MasterApi.Core.Infrastructure.Crypto;
using MasterApi.Core.Account.Enums;
using MasterApi.Core.Account.Models;
using MasterApi.Core.Account.Services;
using MasterApi.Core.Config;
using Microsoft.Extensions.Options;
using MasterApi.Core.Account.Validation;
using MasterApi.Services.Account.Messaging;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MasterApi.Services.Account
{
    public partial class UserAccountService : Service<UserAccount>, IUserAccountService
    {
        public AuthSettings Settings { get; set; }

        private static ICrypto _crypto;
        private static AppSettings _settings;
        private readonly ILogger<UserAccountService> _logger;
        private UserAccountMessages? _accountStatus;

        private readonly Lazy<AggregateValidator<UserAccount>> _usernameValidator;
        private readonly Lazy<AggregateValidator<UserAccount>> _emailValidator;
        private readonly Lazy<AggregateValidator<UserAccount>> _phoneNumberValidator;
        private readonly Lazy<AggregateValidator<UserAccount>> _passwordValidator;

        public UserAccountService(IServiceProvider serviceProvider, IUnitOfWorkAsync unitOfWork) : base(unitOfWork)
        {
            _crypto = (ICrypto)serviceProvider.GetService(typeof(ICrypto));
            var settings = (IOptions<AppSettings>)serviceProvider.GetService(typeof(IOptions<AppSettings>));
            _settings = settings.Value;
            _logger = (ILogger<UserAccountService>)serviceProvider.GetService(typeof(ILogger<UserAccountService>));

            Settings = settings.Value.Auth;

            AddEventHandler(new EmailUserAccountEventsHandler(serviceProvider));

            var accountvalidators = new UserAccountValidators();

            _usernameValidator = new Lazy<AggregateValidator<UserAccount>>(() =>
            {
                var val = new AggregateValidator<UserAccount>();
                if (!_settings.Auth.EmailIsUsername)
                {
                    val.Add(UserAccountValidation.UsernameDoesNotContainAtSign);
                    val.Add(UserAccountValidation.UsernameCanOnlyStartOrEndWithLetterOrDigit);
                    val.Add(UserAccountValidation.UsernameOnlyContainsValidCharacters);
                    val.Add(UserAccountValidation.UsernameOnlySingleInstanceOfSpecialCharacters);
                }
                val.Add(UserAccountValidation.UsernameMustNotAlreadyExist);
                val.Add(accountvalidators.UsernameValidator);
                return val;
            });

            _emailValidator = new Lazy<AggregateValidator<UserAccount>>(() =>
            {
                var val = new AggregateValidator<UserAccount>
                {
                    UserAccountValidation.EmailIsRequiredIfRequireAccountVerificationEnabled,
                    UserAccountValidation.EmailIsValidFormat,
                    UserAccountValidation.EmailMustNotAlreadyExist,
                    accountvalidators.EmailValidator
                };
                return val;
            });

            _phoneNumberValidator = new Lazy<AggregateValidator<UserAccount>>(() =>
            {
                var val = new AggregateValidator<UserAccount>
                {
                    UserAccountValidation.PhoneNumberIsRequiredIfRequireAccountVerificationEnabled,
                    UserAccountValidation.PhoneNumberMustNotAlreadyExist,
                    accountvalidators.PhoneNumberValidator
                };
                return val;
            });

            _passwordValidator = new Lazy<AggregateValidator<UserAccount>>(() =>
            {
                var val = new AggregateValidator<UserAccount>
                {
                    UserAccountValidation.PasswordMustBeDifferentThanCurrent,
                    accountvalidators.PasswordValidator
                };
                return val;
            });
        }

        private static string GetLogMessage(string message, [CallerMemberName] string callerName = null)
        {
            return $"[UserAccountService.{callerName}] - {message}";
        }

        protected void ValidateUsername(UserAccount account, string value)
        {
            var result = _usernameValidator.Value.Validate(this, account, value);
            if (result == null || result == ValidationResult.Success) return;
            var error = GetLogMessage(result.ErrorMessage);
            _logger.LogError(error);
            throw new ValidationException(result.ErrorMessage);
        }

        protected void ValidatePassword(UserAccount account, string value)
        {
            // null is allowed (e.g. for external providers)
            if (value == null) return;

            var result = _passwordValidator.Value.Validate(this, account, value);
            if (result == null || result == ValidationResult.Success) return;
            var error = GetLogMessage(result.ErrorMessage);
            _logger.LogError(error);
            throw new ValidationException(result.ErrorMessage);
        }

        protected void ValidateEmail(UserAccount account, string value)
        {
            var result = _emailValidator.Value.Validate(this, account, value);
            if (result == null || result == ValidationResult.Success) return;
            var error = GetLogMessage(result.ErrorMessage);
            _logger.LogError(error);
            throw new ValidationException(result.ErrorMessage);
        }

        protected void ValidatePhoneNumber(UserAccount account, string value)
        {
            var result = _phoneNumberValidator.Value.Validate(this, account, value);
            if (result == null || result == ValidationResult.Success) return;
            var error = GetLogMessage(result.ErrorMessage);
            _logger.LogError(error);
            throw new ValidationException(result.ErrorMessage);
        }
	}
}