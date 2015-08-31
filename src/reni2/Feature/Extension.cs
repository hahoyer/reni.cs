using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Scanner;
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
        static readonly
            FunctionCache
                <Func<Category, ResultCache, ContextBase, CompileSyntax, Result>, MetaFunction>
            _metaFunctionCache
                =
                new FunctionCache
                    <Func<Category, ResultCache, ContextBase, CompileSyntax, Result>, MetaFunction>
                    (function => new MetaFunction(function));

        static readonly FunctionCache<Func<Category, Result>, FunctionCache<TypeBase, Value>>
            _simpleCache
                = new FunctionCache<Func<Category, Result>, FunctionCache<TypeBase, Value>>
                    (
                    function =>
                        new FunctionCache<TypeBase, Value>(type => new Value(function, type)));

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

        internal static Function FunctionFeature(Func<Category, TypeBase, Result> function)
            => new Function(function);

        internal static ITypeImplementation FunctionFeature<T>
            (Func<Category, TypeBase, T, Result> function, T arg)
            => new ExtendedFunction<T>(function, arg);


        internal static IValue ExtendedValue(this ITypeImplementation feature)
        {
            var function = ((IImplementation) feature).Function;
            if(function != null && function.IsImplicit)
                return null;

            return feature.Value;
        }

        internal static MetaFunction MetaFeature
            (Func<Category, ResultCache, ContextBase, CompileSyntax, Result> function)
            => _metaFunctionCache[function];

        internal static TypeBase ResultType(this IValue f) => f.Result(Category.Type)?.Type;

        internal static bool IsCloseRelative(IValue feature) => !(feature is IStepRelative);

        public static IEnumerable<IGenericProviderForType> GenericListFromType<T>
            (this T target, IEnumerable<IGenericProviderForType> baseList = null)
            where T : TypeBase
            => CreateList(baseList, () => new GenericProviderForType<T>(target));

        public static IEnumerable<IDeclarationProvider> GenericListFromDefinable<T>
            (this T target, IEnumerable<IDeclarationProvider> baseList = null)
            where T : Definable
            => CreateList(baseList, () => new GenericProviderForDefinable<T>(target));

        static IEnumerable<TGeneric> CreateList<TGeneric>
            (IEnumerable<TGeneric> baseList, Func<TGeneric> creator)
        {
            yield return creator();
            if(baseList == null)
                yield break;

            foreach(var item in baseList)
                yield return item;
        }

        internal static Result Result
            (
            this IImplementation feature,
            Category category,
            SourcePart token,
            ContextBase context,
            CompileSyntax right)
        {
            Tracer.Assert
                (
                    feature.Function == null
                        || !feature.Function.IsImplicit
                        || feature.Value == null
                );

            var valueCategory = category;
            if(right != null)
                valueCategory = category.Typed;

            var valueResult = feature.Function != null && feature.Function.IsImplicit
                ? feature
                    .Function
                    .Result(valueCategory, context.RootContext.VoidType)
                    .ReplaceArg(context.RootContext.VoidType.Result(Category.All))
                : right == null || feature.Function == null
                    ? feature.Value?.Result(valueCategory)
                    : null;

            if(right == null)
            {
                if(valueResult == null)
                    Dumpable.NotImplementedFunction(feature, category, token, context, right);
                return valueResult;
            }

            if(valueResult == null)
            {
                if(feature.Function == null)
                    Dumpable.NotImplementedFunction(feature, category, token, context, right);
                Tracer.Assert(feature.Function != null);
                return feature
                    .Function
                    .Result(category, context.ResultAsReferenceCache(right).Type)
                    .ReplaceArg(context.ResultAsReferenceCache(right));
            }

            return valueResult
                .Type.Execute(category, valueResult, token, null, context, right);
        }

        internal static Result Result
            (
            this ITypeImplementation feature,
            Category category,
            Func<Category, Result> objectReference,
            SourcePart token,
            ContextBase context,
            CompileSyntax right)
        {
            var metaFeature = ((IMetaImplementation) feature).Function;
            if(metaFeature != null)
                return metaFeature.Result(category, new ResultCache(objectReference), context, right);

            return feature
                .Result(category, token, context, right)
                .ReplaceArg(objectReference);
        }
    }
}