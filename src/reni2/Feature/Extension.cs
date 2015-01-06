using System.Linq;
using System.Collections.Generic;
using System;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.ReniSyntax;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Feature
{
    static class Extension
    {
        static readonly FunctionCache<Func<ContextBase, Category, CompileSyntax, CompileSyntax, Result>, MetaFunction> _metaFunctionCache
                = new FunctionCache<Func<ContextBase, Category, CompileSyntax, CompileSyntax, Result>, MetaFunction>(function => new MetaFunction(function));

        static readonly FunctionCache<Func<Category, Result>, FunctionCache<TypeBase, Simple>> _simpleCache
            = new FunctionCache<Func<Category, Result>, FunctionCache<TypeBase, Simple>>(function => new FunctionCache<TypeBase, Simple>(type => new Simple(function, type)));

        internal static Simple SimpleFeature(Func<Category, Result> function, TypeBase target = null)
            => _simpleCache[function][(target ?? function.Target as TypeBase).AssertNotNull()];

        internal static Function FunctionFeature(Func<Category, IContextReference, TypeBase, Result> function)
            => new Function(function);

        internal static IFeatureImplementation FunctionFeature<T>
            (Func<Category, TypeBase, T, Result> function, T arg) => new ExtendedFunction<T>(function, arg);


        internal static ISimpleFeature SimpleFeature(this IFeatureImplementation feature)
        {
            var function = feature.Function;
            if(function != null && function.IsImplicit)
                return null;

            return feature.Simple;
        }

        internal static MetaFunction MetaFeature(Func<ContextBase, Category, CompileSyntax, CompileSyntax, Result> function)
            => _metaFunctionCache[function];

        internal static TypeBase ResultType(this ISimpleFeature f) => f.Result(Category.Type).Type;

        internal static IEnumerable<SearchResult> ResolveDeclarations<TDefinable>
            (this IFeatureInheritor inheritor, TDefinable tokenClass)
            where TDefinable : Definable
        {
            return inheritor
                .Source(Category.Type)
                .Type
                .Declarations(tokenClass)
                .Select(result => new InheritedTypeSearchResult(result, inheritor));
        }

        internal static IEnumerable<SearchResult> FilterLowerPriority(this IEnumerable<SearchResult> target)
            => target.FrameElementList((x, y) => x!=y && y.Overrides(x));

        internal static bool Overrides(this IFeatureImplementation left, IFeatureImplementation right)
        {
            var l = left as IDefinitionPriority;
            if (l == null)
                return false;

            var r = right as IDefinitionPriority;
            if (r == null)
                return true;

            return l.Overrides(r);
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