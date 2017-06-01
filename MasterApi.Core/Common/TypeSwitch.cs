using System;

namespace MasterApi.Core.Common
{
    public static class TypeSwitch
    {
        public static Switch<TSource> On<TSource>(TSource value)
        {
            return new Switch<TSource>(value);
        }

        public class Switch<TSource>
        {
            private readonly TSource _value;
            private bool _handled;

            internal Switch(TSource value)
            {
                _value = value;
            }

            public Switch<TSource> Case<TTarget>(Action<TTarget> action)
                where TTarget : TSource
            {
                if (_handled) return this;
                var sourceType = _value.GetType();
                var targetType = typeof(TTarget);
                if (targetType.GetElementType() != sourceType) return this;
                action((TTarget)_value);
                _handled = true;

                return this;
            }

            public void Default(Action<TSource> action)
            {
                if (!_handled)
                    action(_value);
            }
        }
    }
}
