using System;
using System.ComponentModel.DataAnnotations;
using MasterApi.Core.Account.Models;
using MasterApi.Core.Account.Services;
using MasterApi.Core.Services;

namespace MasterApi.Core.Account.Validation
{
    public class DelegateValidator : IValidator<UserAccount>
    {
        private readonly Func<IUserAccountService, UserAccount, string, ValidationResult> func;
        public DelegateValidator(Func<IUserAccountService, UserAccount, string, ValidationResult> func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));

            this.func = func;
        }

        public ValidationResult Validate(IService<UserAccount> service, UserAccount account, string value)
        {
            return func((IUserAccountService)service, account, value);
        }
    }

}
