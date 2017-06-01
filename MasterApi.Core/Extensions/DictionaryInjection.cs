using System;
using System.Reflection;
using Omu.ValueInjecter.Injections;
using System.Collections.Generic;
using System.Linq;

namespace MasterApi.Core.Extensions
{
    public class DictionaryInjection : KnownSourceInjection<IDictionary<string, object>>
    {
        private readonly string[] _ignoredProps;
        private readonly bool _allowNullables;

        public DictionaryInjection(string[] ignoredProps, bool allowNullables = false)
        {
            _ignoredProps = ignoredProps;
            _allowNullables = allowNullables;
        }

        public DictionaryInjection() { }

        protected override void Inject(IDictionary<string, object> source, object target)
        {
            if (target == null) return;
            var props = target.GetType().GetProperties();

            foreach (var o in source)
            {
                var tp = props.SingleOrDefault(x => x.Name.ToLower() == o.Key.ToLower());
                if (tp == null) continue;
                if (_ignoredProps.Contains(tp.Name)) continue;
                if (o.Value == null && !_allowNullables) continue;

                object newValue;
                if (o.Value == null && _allowNullables)
                {
                    newValue = null;
                }
                else
                {
                    var t = Nullable.GetUnderlyingType(tp.PropertyType) ?? tp.PropertyType;
                    newValue = !t.GetTypeInfo().IsEnum
                        ? Convert.ChangeType(o.Value, t)
                        : Enum.Parse(t, o.Value.ToString());
                }

                tp.SetValue(target, newValue);
            }
        }

    }

}