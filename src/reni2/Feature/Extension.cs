using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using hw.Helper;
using JetBrains.Annotations;
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
        static readonly FunctionCache<Func<Category, Result>, Simple> _simpleCache
            = new FunctionCache<Func<Category, Result>, Simple>(function => new Simple(function));

        internal static Simple Feature(Func<Category, Result> function) { return _simpleCache[function]; }

        internal static Simple<T1> Feature<T1>(Func<Category, T1, Result> function)
            where T1 : TypeBase
        {
            return
                new Simple<T1>(function);
        }

        internal static Simple<T1, T2> Feature<T1, T2>(Func<Category, T1, T2, Result> function)
            where T1 : TypeBase
            where T2 : TypeBase
        {
            return
                new Simple<T1, T2>(function);
        }

        internal static string Dump(this IFeatureImplementation feature) { return Tracer.Dump(feature); }
        internal static Function Feature(Func<Category, IContextReference, TypeBase, Result> function)
        {
            return
                new Function(function);
        }

#pragma warning disable 0414
        [UsedImplicitly]
        static bool _isPrettySearchPathHumanFriendly = true;


        internal static ISimpleFeature SimpleFeature(this IFeatureImplementation feature)
        {
            var function = feature.Function;
            if(function != null && function.IsImplicit)
                return null;

            return feature.Simple;
        }

        internal static ISearchResult GetFeature<TDefinable>(this ISearchTarget target, TDefinable definable)
            where TDefinable : Defineable
        {
            var result = target.GetFeature<TDefinable, IFeatureImplementation>();
            if(result == null)
                return null;
            return result;
        }
        public static SearchResult DeclarationsForType<TDefinable>(this ISymbolInheritor inheritor)
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
    }

    sealed class InheritedSearchResult : SearchResult
    {
        [EnableDump]
        readonly SearchResult _result;
        [EnableDump]
        readonly ISymbolInheritor _inheritor;

        public InheritedSearchResult(SearchResult result, ISymbolInheritor inheritor)
        {
            _result = result;
            _inheritor = inheritor;
        }
        public override Result CallResult(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
        {
            NotImplementedMethod(context, category, left, right);
            return null;
        }
        public override Result CallResult(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }
    }
}