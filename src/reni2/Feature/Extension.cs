using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Syntax;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Feature
{
    static class Extension
    {
        static readonly FunctionCache<Func<ContextBase, Category, CompileSyntax, CompileSyntax, Result>, MetaFunction>
            _metaFunctionCache
                = new FunctionCache<Func<ContextBase, Category, CompileSyntax, CompileSyntax, Result>, MetaFunction>
                    (function => new MetaFunction(function));

        static readonly FunctionCache<Func<Category, Result>, FunctionCache<TypeBase, Simple>> _simpleCache
            = new FunctionCache<Func<Category, Result>, FunctionCache<TypeBase, Simple>>
                (function => new FunctionCache<TypeBase, Simple>(type => new Simple(function, type)));

        internal static Simple SimpleFeature(Func<Category, Result> function, TypeBase target = null)
        {
            return _simpleCache[function][(target ?? function.Target as TypeBase).AssertNotNull()];
        }

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
        internal static Function FunctionFeature(Func<Category, IContextReference, TypeBase, Result> function)
        {
            return new Function(function);
        }

        public static IFeatureImplementation FunctionFeature<T>
            (Func<Category, TypeBase, T, Result> function, T arg)
        {
            return new ExtendedFunction<T>(function, arg);
        }


        internal static ISimpleFeature SimpleFeature(this IFeatureImplementation feature)
        {
            var function = feature.Function;
            if(function != null && function.IsImplicit)
                return null;

            return feature.Simple;
        }

        internal static MetaFunction MetaFeature(Func<ContextBase, Category, CompileSyntax, CompileSyntax, Result> function)
        {
            return _metaFunctionCache[function];
        }

        internal static TypeBase ResultType(this ISimpleFeature f) { return f.Result(Category.Type).Type; }

        public static IEnumerable<SearchResult> ResolveDeclarations<TDefinable>
            (this IFeatureInheritor inheritor, TDefinable tokenClass)
            where TDefinable : Definable
        {
            return inheritor
                .Source(Category.Type)
                .Type
                .Declarations(tokenClass)
                .Select(result => new InheritedTypeSearchResult(result, inheritor));
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
        public static IEnumerable<IGenericProviderForType> GenericListFromType<T>
            (this T target, IEnumerable<IGenericProviderForType> baseList = null)
            where T : TypeBase
        {
            return CreateList(baseList, () => new GenericProviderForType<T>(target));
        }

        public static IEnumerable<IGenericProviderForDefinable> GenericListFromDefinable<T>
            (this T target, IEnumerable<IGenericProviderForDefinable> baseList = null)
            where T : Definable
        {
            return CreateList(baseList, () => new GenericProviderForDefinable<T>(target));
        }

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