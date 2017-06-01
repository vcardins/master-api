using MasterApi.Core.Account.Models;

namespace MasterApi.Core.Account.Validation
{
    public class UserAccountValidators
    {
        private readonly AggregateValidator<UserAccount> _usernameValidators = new AggregateValidator<UserAccount>();
        public void RegisterUsernameValidator(params IValidator<UserAccount>[] items)
        {
            _usernameValidators.AddRange(items);
        }
        public IValidator<UserAccount> UsernameValidator => _usernameValidators;

        private readonly AggregateValidator<UserAccount> _passwordValidators = new AggregateValidator<UserAccount>();
        public void RegisterPasswordValidator(params IValidator<UserAccount>[] items)
        {
            _passwordValidators.AddRange(items);
        }
        public IValidator<UserAccount> PasswordValidator => _passwordValidators;

        private readonly AggregateValidator<UserAccount> _emailValidators = new AggregateValidator<UserAccount>();
        public void RegisterEmailValidator(params IValidator<UserAccount>[] items)
        {
            _emailValidators.AddRange(items);
        }
        public IValidator<UserAccount> EmailValidator => _phoneNumberValidators;

        private readonly AggregateValidator<UserAccount> _phoneNumberValidators = new AggregateValidator<UserAccount>();
        public void RegisterPhoneNumberValidator(params IValidator<UserAccount>[] items)
        {
            _phoneNumberValidators.AddRange(items);
        }
        public IValidator<UserAccount> PhoneNumberValidator => _phoneNumberValidators;
    }

}
