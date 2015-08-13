using System.Linq;
using System.Collections.Generic;
using System;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Parser;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Feature
{
    static class Extension
    {
        static readonly FunctionCache<Func<Category, ResultCache, ContextBase, CompileSyntax, Result>, MetaFunction>
            _metaFunctionCache
                = new FunctionCache<Func<Category, ResultCache, ContextBase, CompileSyntax, Result>, MetaFunction>
                    (function => new MetaFunction(function));

        static readonly FunctionCache<Func<Category, Result>, FunctionCache<TypeBase, Value>> _simpleCache
            = new FunctionCache<Func<Category, Result>, FunctionCache<TypeBase, Value>>
                (function => new FunctionCache<TypeBase, Value>(type => new Value(function, type)));

        internal static Value Value(Func<Category, Result> function, TypeBase target = null)
            => _simpleCache[function][(target ?? function.Target as TypeBase).AssertNotNull()];

        internal static ObjectFunction FunctionFeature
            (
            Func<Category, IContextReference, TypeBase, Result> function,
            IContextReferenceProvider target = null
            )
        {
            var context = (target ?? function.Target as IContextReferenceProvider).AssertNotNull();
            return new ObjectFunction(function, context);
        }

        internal static Function FunctionFeature(Func<Category, TypeBase, Result> function) => new Function(function);

        internal static IFeatureImplementation FunctionFeature<T>(Func<Category, TypeBase, T, Result> function, T arg)
            => new ExtendedFunction<T>(function, arg);


        internal static IValueFeature ExtendedValue(this IFeatureImplementation feature)
        {
            var function = feature.Function;
            if(function != null && function.IsImplicit)
                return null;

            return feature.Value;
        }

        internal static MetaFunction MetaFeature(Func<Category, ResultCache, ContextBase, CompileSyntax, Result> function)
            => _metaFunctionCache[function];

        internal static TypeBase ResultType(this IValueFeature f) => f.Result(Category.Type)?.Type;

        internal static bool IsCloseRelative(IValueFeature feature) => !(feature is IStepRelative);

        public static IEnumerable<IGenericProviderForType> GenericListFromType<T>
            (this T target, IEnumerable<IGenericProviderForType> baseList = null)
            where T : TypeBase
            => CreateList(baseList, () => new GenericProviderForType<T>(target));

        public static IEnumerable<IGenericProviderForDefinable> GenericListFromDefinable<T>
            (this T target, IEnumerable<IGenericProviderForDefinable> baseList = null)
            where T : Definable
            => CreateList(baseList, () => new GenericProviderForDefinable<T>(target));

        static IEnumerable<TGeneric> CreateList<TGeneric>(IEnumerable<TGeneric> baseList, Func<TGeneric> creator)
        {
            yield return creator();
            if(baseList == null)
                yield break;

            foreach(var item in baseList)
                yield return item;
        }
    }
}