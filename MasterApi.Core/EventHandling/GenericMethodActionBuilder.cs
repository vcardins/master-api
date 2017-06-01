using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace MasterApi.Core.EventHandling
{
    internal class GenericMethodActionBuilder<TTargetBase, TParamBase>
    {
        private readonly ConcurrentDictionary<Type, Action<TTargetBase, TParamBase>> _actionCache = new ConcurrentDictionary<Type, Action<TTargetBase, TParamBase>>();

        private readonly Type _targetType;
        private readonly string _method;
        public GenericMethodActionBuilder(Type targetType, string method)
        {
            _targetType = targetType;
            _method = method;
        }

        public Action<TTargetBase, TParamBase> GetAction(TParamBase paramInstance)
        {
            var paramType = paramInstance.GetType();

            if (!_actionCache.ContainsKey(paramType))
            {
                _actionCache[paramType] = BuildActionForMethod(paramType);
            }

            return _actionCache[paramType];
        }

        private Action<TTargetBase, TParamBase> BuildActionForMethod(Type paramType)
        {
            var handlerType = _targetType.MakeGenericType(paramType);

            var ehParam = Expression.Parameter(typeof(TTargetBase));
            var evtParam = Expression.Parameter(typeof(TParamBase));
            var invocationExpression =
                Expression.Lambda(
                    Expression.Block(
                        Expression.Call(
                            Expression.Convert(ehParam, handlerType),
                            handlerType.GetMethod(_method),
                            Expression.Convert(evtParam, paramType))),
                    ehParam, evtParam);

            return (Action<TTargetBase, TParamBase>)invocationExpression.Compile();
        }
    }
}
