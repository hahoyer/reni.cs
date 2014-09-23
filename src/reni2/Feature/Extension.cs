using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Feature
{
    static class Extension
    {
        static readonly FunctionCache<Func<Category, Result>, Simple> _simpleCache
            = new FunctionCache<Func<Category, Result>, Simple>(function => new Simple(function));

        internal static Simple Feature(Func<Category, Result> function) { return _simpleCache[function]; }

        internal static Simple<T1> Feature<T1>(Func<Category, T1, Result> function)
            where T1 : TypeBase
        {
            return new Simple<T1>(function);
        }

        internal static Simple<T1, T2> Feature<T1, T2>(Func<Category, T1, T2, Result> function)
            where T1 : TypeBase
            where T2 : TypeBase
        {
            return new Simple<T1, T2>(function);
        }

        internal static string Dump(this IFeatureImplementation feature) { return Tracer.Dump(feature); }
        internal static Function Feature(Func<Category, IContextReference, TypeBase, Result> function)
        {
            return new Function(function);
        }

        internal static ISimpleFeature SimpleFeature(this IFeatureImplementation feature)
        {
            var function = feature.Function;
            if(function != null && function.IsImplicit)
                return null;

            return feature.Simple;
        }

        public static SearchResult ResolveDeclarationsForType<TDefinable>(this IFeatureInheritor inheritor)
            where TDefinable : Defineable
        {
            var result = inheritor
                .Source(Category.Type)
                .Type
                .DeclarationsForType<TDefinable>();
            if(result == null)
                return null;
            return new InheritedSearchResult(result, inheritor);
        }

        public static SearchResult ResolveConverterForType<TSource, TPath>(this IPuppet<TSource, TPath> puppet, TSource source)
            where TSource : TypeBase
        {
            var result = puppet
                .ResolveConverter(source);
            if(result == null)
                return null;
            Dumpable.NotImplementedFunction(puppet, source);
            return null;
        }

        public static SearchResult ResolveConverterForType<TSource, TPath>
            (this IConverterProvider<TSource, TPath> provider, TypeBase destination, TSource source)
            where TSource : TypeBase
        {
            Dumpable.NotImplementedFunction(provider, source);
            return null;
        }
    }

    static class GenericizeExtension
    {
        public static IEnumerable<IGenericProviderForType> GenericList<T>
            (this T target, IEnumerable<IGenericProviderForType> baseList = null) where T : TypeBase
        {
            return CreateList(baseList, () => new GenericProviderForType<T>(target));
        }

        static IEnumerable<TGeneric> CreateList<TGeneric>(IEnumerable<TGeneric> baseList, Func<TGeneric> creator)
        {
            yield return creator();
            if(baseList != null)
                foreach(var item in baseList)
                    yield return item;
        }
    }
}