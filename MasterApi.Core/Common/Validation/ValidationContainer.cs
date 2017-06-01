#region credits
// ***********************************************************************
// Assembly	: Flext.Core
// Author	: Victor Cardins
// Created	: 03-19-2013
// 
// Last Modified By : Victor Cardins
// Last Modified On : 03-21-2013
// ***********************************************************************
#endregion

using System.Collections.Generic;

namespace MasterApi.Core.Common.Validation
{
    #region

    

    #endregion

    public class ValidationContainer<T> : IValidationContainer<T>
    {
        public IDictionary<string, IList<string>> Errors { get; }
        public T Entity { get; }

        public bool IsValid => Errors.Count == 0;

        public ValidationContainer(IDictionary<string, IList<string>> errors, T entity)
        {
            Errors = errors;
            Entity = entity;
        }
    }
}
