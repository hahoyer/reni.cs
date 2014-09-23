using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Syntax;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Feature
{
    interface IFeatureImplementation
    {
        IMetaFunctionFeature MetaFunction { get; }
        IFunctionFeature Function { get; }
        ISimpleFeature Simple { get; }
    }

    interface ISimpleFeature
    {
        Result Result(Category category);
    }

    interface IFunctionFeature
    {
        /// <summary>
        ///     Result code contains CodeBase.Arg for argsType and ObjectReference for function object, if appropriate
        /// </summary>
        /// <param name="category"> </param>
        /// <param name="argsType"> </param>
        /// <returns> </returns>
        Result ApplyResult(Category category, TypeBase argsType);

        /// <summary>
        ///     Gets a value indicating whether this function requires implicit call (i. e. call without argument list).
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is implicit; otherwise, <c>false</c>.
        /// </value>
        [DisableDump]
        bool IsImplicit { get; }

        [DisableDump]
        IContextReference ObjectReference { get; }
    }

    interface IMetaFunctionFeature
    {
        Result Result(ContextBase contextBase, Category category, CompileSyntax right);
    }

    interface ISearchResult
    {
        Result FunctionResult(ContextBase context, Category category, CompileSyntax left, CompileSyntax right);
        Result SimpleResult(Category category);
        ISearchResult WithConversion(IConverter converter);

        ISearchResult Convert<TProvider>(TProvider innerProvider);
    }

    interface IContextSearchResult
    {
        Result FunctionResult(ContextBase context, Category category, CompileSyntax right);
        Result SimpleResult(Category category);
        ISearchResult WithConversion(IConverter converter);
    }

    interface ISearchTarget
    {}

    interface ISymbolProvider<TDefinable, out TPath>
        where TDefinable : Defineable
    {
        TPath Feature { get; }
    }

    interface IConverterProvider<in TDestination, out TPath>
    {
        TPath Feature(TDestination destination, IConversionParameter parameter);
    }

    interface IPath<out TPath, in TProvider>
        where TProvider : TypeBase
    {
        TPath Convert(TProvider provider);
    }

    sealed class ContextSearchResult
    {
        internal readonly IAccessFeature Data;
        internal ContextSearchResult(IAccessFeature data) { Data = data; }
    }

    abstract class SearchResult : DumpableObject
    {
        public abstract IFeatureImplementation Feature { get; }
        public abstract Result Converter(Category category);
        public abstract TypeBase Type { get; }

        CallDescriptor CallDescriptor { get { return new CallDescriptor(Type, Feature, Converter); } }

        internal Result CallResult(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
        {
            return CallDescriptor
                .Result(category, context, right)
                .ReplaceArg(c => context.ObjectResult(c, left));
        }

        internal Result CallResult(Category category) { return CallDescriptor.Result(category); }

        internal Result ConversionResult(Category category, TypeBase source, TypeBase destination)
        {
            var callResult = CallResult(category.Typed);
            var result = callResult
                .ReplaceArg(source.ArgResult);

            var obviousConversion = result.Type.ObviousConversion(category, destination);
            return obviousConversion.ReplaceArg(result) & category;
        }
    }

    sealed class TypeSearchResult : SearchResult
    {
        readonly IFeatureImplementation _data;
        readonly TypeBase _definingType;

        internal TypeSearchResult(IFeatureImplementation data, TypeBase definingType)
        {
            _data = data;
            _definingType = definingType;
        }

        [DisableDump]
        public override IFeatureImplementation Feature { get { return _data; } }

        public override Result Converter(Category category) { return _definingType.ArgResult(category); }

        [DisableDump]
        public override TypeBase Type { get { return _definingType; } }
    }

    sealed class InheritedSearchResult : SearchResult
    {
        [EnableDump]
        readonly SearchResult _result;
        [EnableDump]
        readonly IFeatureInheritor _inheritor;

        public InheritedSearchResult(SearchResult result, IFeatureInheritor inheritor)
        {
            _result = result;
            _inheritor = inheritor;
        }

        [DisableDump]
        public override IFeatureImplementation Feature { get { return _result.Feature; } }

        public override Result Converter(Category category)
        {
            return _result
                .Converter(category)
                .ReplaceArg(_inheritor.Source);
        }

        [DisableDump]
        public override TypeBase Type { get { return _result.Type; } }
    }

    interface IFeature
    {
        Result FunctionResult(ContextBase context, Category category, CompileSyntax left, CompileSyntax right);
    }

    interface IAccessFeature
    {
        Result FunctionResult(ContextBase context, Category category, CompileSyntax right);
    }

    interface IFeatureInheritor
    {
        Result Source(Category category);
    }

    interface IPuppet<in TSource, TPath>
    {
        SearchResult ResolveConverter(TSource source);
    }

    interface IGenericProviderForType
    {
        IEnumerable<SearchResult> ConvertersForType(TypeBase source, IConversionParameter parameter);
    }

    sealed class GenericProviderForType<T> : DumpableObject, IGenericProviderForType
    {
        readonly T _target;
        public GenericProviderForType(T target) { _target = target; }

        IEnumerable<SearchResult> IGenericProviderForType.ConvertersForType(TypeBase source, IConversionParameter parameter)
        {
            return source.ConvertersForType(_target, parameter);
        }
    }
}