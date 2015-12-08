using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;

namespace Reni
{
    public static class ValueCacheExtension
    {
        public static ValueCache<TValueType> NewCache<TValueType>(Func<TValueType> func)
            => new ValueCache<TValueType>(func);

        public static TValueType CachedValue<TValueType>
            (IContainer container, Func<TValueType> func)
            => GetCache(container, func).Value;

        static ValueCache<TValueType> GetCache<TValueType>
            (IContainer container, Func<TValueType> func)
        {
            object oldResult;
            if(container.Cache.TryGetValue(func, out oldResult))
                return (ValueCache<TValueType>) oldResult;

            var newResult = new ValueCache<TValueType>(func);
            container.Cache.Add(func, newResult);
            return newResult;
        }

        public sealed class Container: Dictionary<object, object> { }

        public interface IContainer
        {
            Container Cache { get; }
        }
    }

}