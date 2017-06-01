using System;

namespace MasterApi.Core.Filters
{
    public class CustomValidationException : Exception
    {
        public CustomValidationException()
        {
        }

        public CustomValidationException(string message)
            : base(message)
        {

        }

        public CustomValidationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}