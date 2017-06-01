using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MasterApi.Core.Models
{
    public abstract class ValidatableModel : IValidatableObject
    {
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Override this method to implement custom validation in your entities
            // This is only for making it compile... and returning null will give an exception.
            if (false)
            {
                yield return new ValidationResult("Well, this should not happend...");
            }
        }
    }
}
