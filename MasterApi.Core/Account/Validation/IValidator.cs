using System.ComponentModel.DataAnnotations;
using MasterApi.Core.Data.Infrastructure;
using MasterApi.Core.Services;

namespace MasterApi.Core.Account.Validation
{
    public interface IValidator<T> where T : class, IObjectState
    {
        ValidationResult Validate(IService<T> service, T account, string value);
    }
}
