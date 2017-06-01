using System.ComponentModel;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using MasterApi.Core.Models;

namespace MasterApi.Core.Common.Validation
{
    public static class ValidationEngine
    {
        /// <summary>
        ///     Will validate an entity that implements IValidatableObject and DataAnnotations
        /// </summary>
        /// <typeparam name="T">The type that inherits the abstract basetype DomainObject</typeparam>
        /// <param name="entity">The Entity to validate</param>
        /// <returns></returns>
        public static ValidationContainer<T> GetValidationContainer<T>(this T entity) where T : ValidatableModel
        {
            var brokenrules = new Dictionary<string, IList<string>>();

            // IValidatableObject
            var customErrors = entity.Validate(new ValidationContext(entity, null, null));
            if (customErrors != null)
            {
                foreach (var customError in customErrors)
                {
                    if (customError == null)
                    {
                        continue;
                    }

                    foreach (var memberName in customError.MemberNames)
                    {
                        if (!brokenrules.ContainsKey(memberName))
                        {
                            brokenrules.Add(memberName, new List<string>());
                        }

                        brokenrules[memberName].Add(customError.ErrorMessage);
                    }
                }
            }

            var results = ValidateAnnotations(entity);

            foreach (var result in results)
            {
                brokenrules.Add(result.Key, result.Value);
            }

            return new ValidationContainer<T>(brokenrules, entity);
        }

        /// <summary>
        ///     Will validate recursively DataAnnotations
        /// </summary>
        /// <typeparam name="T">The type that inherits the abstract basetype DomainObject</typeparam>
        /// <param name="entity">The Entity to validate</param>
        /// <returns>The dictionary that contain validation results</returns>
        private static Dictionary<string, IList<string>> ValidateAnnotations<T>(T entity) where T : ValidatableModel
        {
            var brokenrules = new Dictionary<string, IList<string>>();

            // DataAnnotations
            foreach (var pi in entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if(pi.PropertyType.DeclaringType == typeof(ValidatableModel))
                {
                    var obj = (ValidatableModel)pi.GetValue(entity);

                    if (obj == null) continue;

                    var results = ValidateAnnotations(obj);

                    foreach (var result in results)
                    {
                        brokenrules.Add(result.Key, result.Value);
                    }
                }


                foreach (
                    var attribute in (ValidationAttribute[])pi.GetCustomAttributes(typeof(ValidationAttribute), false))
                {
                    if (attribute.IsValid(pi.GetValue(entity, null)))
                    {
                        continue;
                    }

                    var description = (DescriptionAttribute)pi.GetCustomAttribute(typeof (DescriptionAttribute), false);

                    var name = description?.ToString() ?? pi.Name;

                    if (!brokenrules.ContainsKey(name))
                    {
                        brokenrules.Add(name, new List<string>());
                    }

                    brokenrules[name].Add(attribute.FormatErrorMessage(pi.Name));
                }
            }

            return brokenrules;
        }
    }

}