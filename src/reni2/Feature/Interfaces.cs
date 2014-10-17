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
        IContextMetaFunctionFeature ContextMetaFunction { get; }
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
        Result Result(ContextBase contextBase, Category category, CompileSyntax left, CompileSyntax right);
    }

    interface IContextMetaFunctionFeature
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
        where TDefinable : Definable
    {
        TPath Feature(TDefinable tokenClass);
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

    sealed class ContextSearchResult : DumpableObject
    {
        [EnableDump]
        readonly IFeatureImplementation _feature;
        [EnableDump]
        readonly ContextBase _definingItem;
        internal ContextSearchResult(IFeatureImplementation feature, ContextBase definingItem)
        {
            _feature = feature;
            _definingItem = definingItem;
        }

        FeatureDescriptor FeatureDescriptor { get { return new ContextCallDescriptor(_definingItem, _feature); } }

        public Result CallResult(ContextBase callContext, Category category, CompileSyntax right)
        {
            return FeatureDescriptor.Result(category, callContext, right);
        }
    }

    sealed class ContextCallDescriptor : FeatureDescriptor
    {
        [EnableDump]
        readonly ContextBase _definingItem;
        [EnableDump]
        readonly IFeatureImplementation _feature;

        public ContextCallDescriptor(ContextBase definingItem, IFeatureImplementation feature)
        {
            _definingItem = definingItem;
            _feature = feature;
        }
        protected override TypeBase Type
        {
            get
            {
                NotImplementedMethod();
                return null;
                ;
            }
        }
        protected override IFeatureImplementation Feature { get { return _feature; } }
        protected override Func<Category, Result> ConverterResult { get { return null; } }
    }

    abstract class SearchResult : DumpableObject
    {
        public abstract IFeatureImplementation Feature { get; }
        public abstract Result Converter(Category category);
        public abstract TypeBase Type { get; }

        CallDescriptor CallDescriptor { get { return new CallDescriptor(Type, Feature, Converter); } }

        internal Result CallResult
            (ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
        {
            return CallDescriptor
                .Result(category, context, left, right);
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
        readonly TypeBase _definingItem;

        internal TypeSearchResult(IFeatureImplementation data, TypeBase definingItem)
        {
            _data = data;
            _definingItem = definingItem;
        }

        [DisableDump]
        public override IFeatureImplementation Feature { get { return _data; } }

        public override Result Converter(Category category) { return _definingItem.ArgResult(category); }

        [DisableDump]
        public override TypeBase Type { get { return _definingItem; } }
    }

    sealed class InheritedTypeSearchResult : SearchResult
    {
        [EnableDump]
        readonly SearchResult _result;
        [EnableDump]
        readonly IFeatureInheritor _inheritor;

        public InheritedTypeSearchResult(SearchResult result, IFeatureInheritor inheritor)
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

    interface IContextFeatureInheritor
    {
        Result Source(Category category);
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
        IEnumerable<SearchResult> Converters(TypeBase source, IConversionParameter parameter);
    }

    interface IGenericProviderForDefinable
    {
        IEnumerable<SearchResult> Declarations(TypeBase source);
        IEnumerable<ContextSearchResult> Declarations(ContextBase source);
    }

    sealed class GenericProviderForType<T> : DumpableObject, IGenericProviderForType
    {
        readonly T _target;
        public GenericProviderForType(T target) { _target = target; }

        IEnumerable<SearchResult> IGenericProviderForType.Converters(TypeBase source, IConversionParameter parameter)
        {
            return source.ConvertersForType(_target, parameter);
        }
    }

    sealed class GenericProviderForDefinable<T> : DumpableObject, IGenericProviderForDefinable
        where T : Definable
    {
        readonly T _target;
        public GenericProviderForDefinable(T target) { _target = target; }

        IEnumerable<SearchResult> IGenericProviderForDefinable.Declarations(TypeBase source)
        {
            return source.Declarations(_target);
        }
        IEnumerable<ContextSearchResult> IGenericProviderForDefinable.Declarations(ContextBase source)
        {
            return source.Declarations(_target);
        }
    }

    sealed class MetaFunction : DumpableObject, IFeatureImplementation, IMetaFunctionFeature
    {
        readonly Func<ContextBase, Category, CompileSyntax, CompileSyntax, Result> _function;
        public MetaFunction(Func<ContextBase, Category, CompileSyntax, CompileSyntax, Result> function) { _function = function; }

        IContextMetaFunctionFeature IFeatureImplementation.ContextMetaFunction { get { return null; } }
        IMetaFunctionFeature IFeatureImplementation.MetaFunction { get { return this; } }
        IFunctionFeature IFeatureImplementation.Function { get { return null; } }
        ISimpleFeature IFeatureImplementation.Simple { get { return null; } }

        Result IMetaFunctionFeature.Result(ContextBase contextBase, Category category, CompileSyntax left, CompileSyntax right)
        {
            return _function(contextBase, category, left, right);
        }
    }

    sealed class ContextMetaFunctionFromSyntax : DumpableObject, IFeatureImplementation, IContextMetaFunctionFeature
    {
        [EnableDump]
        readonly CompileSyntax _definition;
        public ContextMetaFunctionFromSyntax(CompileSyntax definition) { _definition = definition; }

        IContextMetaFunctionFeature IFeatureImplementation.ContextMetaFunction { get { return this; } }
        IMetaFunctionFeature IFeatureImplementation.MetaFunction { get { return null; } }
        IFunctionFeature IFeatureImplementation.Function { get { return null; } }
        ISimpleFeature IFeatureImplementation.Simple { get { return null; } }

        Result IContextMetaFunctionFeature.Result(ContextBase callContext, Category category, CompileSyntax right)
        {
            return callContext.Result(category, _definition.ReplaceArg(right));
        }
    }
}