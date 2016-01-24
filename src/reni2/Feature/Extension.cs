using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
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
            ValueCache
                = new FunctionCache<Func<Category, Result>, FunctionCache<TypeBase, Value>>
                    (
                    function =>
                        new FunctionCache<TypeBase, Value>(type => new Value(function, type)));

        static readonly FunctionCache<Func<Category, Result>, FunctionCache<TypeBase, Conversion>>
            ConversionCache
                = new FunctionCache<Func<Category, Result>, FunctionCache<TypeBase, Conversion>>
                    (
                    function =>
                        new FunctionCache<TypeBase, Conversion>
                            (type => new Conversion(function, type)));

        internal static Value Value(Func<Category, Result> function, TypeBase target = null)
            => ValueCache[function][(target ?? function.Target as TypeBase).AssertNotNull()];

        internal static Conversion Conversion
            (Func<Category, Result> function, TypeBase target = null)
            => ConversionCache[function][(target ?? function.Target as TypeBase).AssertNotNull()];

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

        internal static IImplementation FunctionFeature<T>
            (Func<Category, TypeBase, T, Result> function, T arg)
            => new ExtendedFunction<T>(function, arg);


        internal static IValue ExtendedValue(this IImplementation feature)
        {
            var function = ((IEvalImplementation) feature).Function;
            if(function != null && function.IsImplicit)
                return null;

            return feature.Value;
        }

        internal static MetaFunction MetaFeature
            (Func<Category, ResultCache, ContextBase, CompileSyntax, Result> function)
            => _metaFunctionCache[function];

        internal static TypeBase ResultType(this IConversion conversion)
            => conversion.Result(Category.Type)?.Type;

        internal static Result Result(this IConversion conversion, Category category)
        {
            var result = conversion.Execute(category);
            if(category.HasCode)
                Tracer.Assert
                    (result.Code.ArgType == conversion.Source, () => result.DebuggerDumpString);
            return result;
        }

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
            this IEvalImplementation feature,
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

            var valueResult = feature.ValueResult(context, right, valueCategory);

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

                var result = feature
                    .Function
                    .Result(category, context.ResultAsReferenceCache(right).Type);

                return result
                    .ReplaceArg(context.ResultAsReferenceCache(right));
            }

            return valueResult
                .Type.Execute(category, valueResult, token, null, context, right);
        }

        static Result ValueResult
            (
            this IEvalImplementation feature,
            ContextBase context,
            CompileSyntax right,
            Category valueCategory)
        {
            if(feature.Function != null && feature.Function.IsImplicit)
            {
                var result = feature
                    .Function
                    .Result(valueCategory, context.RootContext.VoidType);

                return result
                    .ReplaceArg(context.RootContext.VoidType.Result(Category.All));
            }

            if(right != null && feature.Function != null)
                return null;

            return feature.Value?.Execute(valueCategory);
        }

        internal static Result Result
            (
            this IImplementation feature,
            Category category,
            ResultCache left,
            SourcePart token,
            ContextBase context,
            CompileSyntax right)
        {
            var metaFeature = ((IMetaImplementation) feature).Function;
            if(metaFeature != null)
                return metaFeature.Result(category, left, context, right);

            return feature
                .Result(category, token, context, right)
                .ReplaceArg(left);
        }

        internal static T DistinctNotNull<T>(this IEnumerable<T> enumerable)
            where T : class
        {
            return enumerable
                .Where(x => x != null)
                .Distinct()
                .SingleOrDefault();
        }

        internal static IEnumerable<ResultCache.IResultProvider> GetDefinableResults
            (
            this IImplementation implementation,
            IContextReference ext,
            ContextBase context,
            CompileSyntax right)
        {
            var metaFeature = ((IMetaImplementation) implementation).Function;
            if(metaFeature != null)
            {
                Dumpable.NotImplementedFunction(implementation, ext, context, right);
                yield break;
            }

            var result = ((IEvalImplementation) implementation).Function?.FindSource(ext)
                ?? implementation.Value.FindSource(ext);
            if(result != null)
                yield return result;
        }
    }
}