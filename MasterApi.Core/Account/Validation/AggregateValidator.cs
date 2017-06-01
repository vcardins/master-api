using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MasterApi.Core.Data.Infrastructure;
using MasterApi.Core.Services;

namespace MasterApi.Core.Account.Validation
{
    public class AggregateValidator<T> : List<IValidator<T>>, IValidator<T>
       where T : class, IObjectState
    {
        public ValidationResult Validate(IService<T> service, T account, string value)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (account == null) throw new ArgumentNullException(nameof(account));

            foreach (var item in this)
            {
                var result = item.Validate(service, account, value);
                if (result != null && result != ValidationResult.Success)
                {
                    return result;
                }
            }
            return null;
        }

    }

}
