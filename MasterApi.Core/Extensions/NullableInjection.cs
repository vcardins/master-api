using System;
using System.Reflection;
using Omu.ValueInjecter.Injections;

namespace MasterApi.Core.Extensions
{
    public class NullableInjection : LoopInjection
    {
        public NullableInjection(string[] ignoredProps = null)
            : base(ignoredProps)
        {}

        public NullableInjection() : base(null) {}

        protected override bool MatchTypes(Type source, Type target)
        {
            var matches = source == target ||
                          source == Nullable.GetUnderlyingType(target) ||
                          target == Nullable.GetUnderlyingType(source);
            return matches;
        }

        protected override void SetValue(object source, object target, PropertyInfo sp, PropertyInfo tp)
        {
            var val = sp.GetValue(source);
            if (val == null) return;
            tp.SetValue(target, val);    
        }             
    }

}