using System;
using MasterApi.Core.Enums;

namespace MasterApi.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ModuleAttribute : Attribute
    {
        public ModelType Name { get; set; }
        public bool LiveUpdate { get; set; }
    }
}