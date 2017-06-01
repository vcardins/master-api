using System;
using MasterApi.Core.Constants;

namespace MasterApi.Core.Filters
{
    public class ForbiddenException : Exception
    {
        public ForbiddenException()
            : this(AppConstants.InformationMessages.AccessNotAllowed)
        {
        }

        public ForbiddenException(string message)
            : base(message)
        {

        }

        public ForbiddenException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}