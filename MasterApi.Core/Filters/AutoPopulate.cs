using System;

namespace MasterApi.Core.Filters
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AutoPopulateAttribute : Attribute
    {
    }
}