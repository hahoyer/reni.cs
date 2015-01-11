using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.ReniSyntax;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Feature
{
    interface IFeatureImplementation
    {
        IMetaFunctionFeature Meta { get; }
        IFunctionFeature Function { get; }
        ISimpleFeature Simple { get; }
        IContextMetaFunctionFeature ContextMeta { get; }
    }

    abstract class EmptyFeatureImplementation : DumpableObject, IFeatureImplementation
    {
        protected EmptyFeatureImplementation(int? nextObjectId)
            : base(nextObjectId) { }
        protected EmptyFeatureImplementation() { }

        IContextMetaFunctionFeature IFeatureImplementation.ContextMeta => ContextMeta;
        IMetaFunctionFeature IFeatureImplementation.Meta => Meta;
        IFunctionFeature IFeatureImplementation.Function => Function;
        ISimpleFeature IFeatureImplementation.Simple => Simple;

        protected virtual IContextMetaFunctionFeature ContextMeta => null;
        protected virtual IMetaFunctionFeature Meta => null;
        protected virtual IFunctionFeature Function => null;
        protected virtual ISimpleFeature Simple => null;
    }

    interface ISimpleFeature
    {
        Result Result(Category category);
        TypeBase TargetType { get; }
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
        Result SimpleResult(Category category);
    }

    interface ISearchTarget {}

    // ReSharper disable once TypeParameterCanBeVariant
    // Exact match for TDefinable is required here.
    interface ISymbolProvider<TDefinable, out TPath>
        where TDefinable : Definable
    {
        TPath Feature(TDefinable tokenClass);
    }

    interface IConverterProvider<in TDestination, out TPath>
    {
        TPath Feature(TDestination destination, IConversionParameter parameter);
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

        [DisableDump]
        protected override TypeBase Type => _definingItem.FindRecentCompoundView.Type;

        [DisableDump]
        protected override IFeatureImplementation Feature => _feature;

        [DisableDump]
        protected override Func<Category, Result> ConverterResult => _definingItem.GetObjectResult;
    }

    abstract class SearchResult : DumpableObject
    {
        public abstract IFeatureImplementation Feature { get; }
        public abstract Result Converter(Category category);
        public abstract TypeBase Type { get; }

        CallDescriptor CallDescriptor => new CallDescriptor(Type, Feature, Converter);

        internal Result CallResult(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
            => CallDescriptor.Result(category, context, left, right);

        internal Result CallResult(Category category) => CallDescriptor.Result(category);

        internal abstract bool Overrides(SearchResult other);
        internal abstract bool IsOverriddenBy(TypeSearchResult other);
        internal abstract bool IsOverriddenBy(InheritedTypeSearchResult other);
    }

    sealed class TypeSearchResult : SearchResult
    {
        readonly TypeBase _definingItem;

        internal TypeSearchResult(IFeatureImplementation data, TypeBase definingItem)
        {
            Feature = data;
            _definingItem = definingItem;
            StopByObjectId(-39);
        }

        [DisableDump]
        public override IFeatureImplementation Feature { get; }

        public override Result Converter(Category category) => _definingItem.UniquePointer.ArgResult(category);

        [DisableDump]
        public override TypeBase Type => _definingItem;
        internal override bool Overrides(SearchResult other) => other.IsOverriddenBy(this);
        internal override bool IsOverriddenBy(TypeSearchResult other)
        {
            if(_definingItem == other._definingItem)
                return other.Feature.Overrides(Feature);
            NotImplementedMethod(other);
            return false;
        }
        internal override bool IsOverriddenBy(InheritedTypeSearchResult other)
        {
            NotImplementedMethod(other);
            return false;
        }
    }

    interface IDefinitionPriority
    {
        bool Overrides(IDefinitionPriority other);
        bool IsOverriddenBy(AccessFeature other);
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
        public override IFeatureImplementation Feature => _result.Feature;

        public override Result Converter(Category category)
            => _result
                .Converter(category)
                .ReplaceArg(_inheritor.Source);

        [DisableDump]
        public override TypeBase Type => _result.Type;
        internal override bool Overrides(SearchResult other) => other.IsOverriddenBy(this);
        internal override bool IsOverriddenBy(TypeSearchResult other)
        {
            NotImplementedMethod(other);
            return false;
        }
        internal override bool IsOverriddenBy(InheritedTypeSearchResult other)
        {
            if(_inheritor == other._inheritor)
                return other._result.Overrides(_result);
            NotImplementedMethod(other);
            return false;
        }
    }

    interface IFeatureInheritor
    {
        Result Source(Category category);
    }

    interface IGenericProviderForType
    {
        IEnumerable<ISimpleFeature> GetForcedConversions(TypeBase typeBase);
    }

    interface IGenericProviderForDefinable
    {
        IEnumerable<SearchResult> Declarations(TypeBase source);
        IEnumerable<ContextCallDescriptor> Declarations(ContextBase source);
    }

    sealed class GenericProviderForType<T> : DumpableObject, IGenericProviderForType
    {
        readonly T _target;
        public GenericProviderForType(T target) { _target = target; }

        IEnumerable<ISimpleFeature> IGenericProviderForType.GetForcedConversions(TypeBase source)
            => source.GetForcedConversions(_target);
    }

    sealed class GenericProviderForDefinable<T> : DumpableObject, IGenericProviderForDefinable
        where T : Definable
    {
        readonly T _target;
        public GenericProviderForDefinable(T target) { _target = target; }

        IEnumerable<SearchResult> IGenericProviderForDefinable.Declarations(TypeBase source) => source.Declarations(_target);
        IEnumerable<ContextCallDescriptor> IGenericProviderForDefinable.Declarations(ContextBase source)
            => source.Declarations(_target);
    }

    sealed class MetaFunction : DumpableObject, IFeatureImplementation, IMetaFunctionFeature
    {
        readonly Func<ContextBase, Category, CompileSyntax, CompileSyntax, Result> _function;
        public MetaFunction(Func<ContextBase, Category, CompileSyntax, CompileSyntax, Result> function) { _function = function; }

        IContextMetaFunctionFeature IFeatureImplementation.ContextMeta => null;
        IMetaFunctionFeature IFeatureImplementation.Meta => this;
        IFunctionFeature IFeatureImplementation.Function => null;
        ISimpleFeature IFeatureImplementation.Simple => null;

        Result IMetaFunctionFeature.Result(ContextBase contextBase, Category category, CompileSyntax left, CompileSyntax right)
            => _function(contextBase, category, left, right);
    }

    sealed class ContextMetaFunction : EmptyFeatureImplementation, IContextMetaFunctionFeature
    {
        readonly Func<ContextBase, Category, CompileSyntax, Result> _function;
        public ContextMetaFunction(Func<ContextBase, Category, CompileSyntax, Result> function) { _function = function; }

        protected override IContextMetaFunctionFeature ContextMeta => this;

        Result IContextMetaFunctionFeature.Result(ContextBase contextBase, Category category, CompileSyntax right)
            => _function(contextBase, category, right);
    }

    sealed class ContextMetaFunctionFromSyntax : DumpableObject, IFeatureImplementation, IContextMetaFunctionFeature
    {
        [EnableDump]
        readonly CompileSyntax _definition;
        public ContextMetaFunctionFromSyntax(CompileSyntax definition) { _definition = definition; }

        IContextMetaFunctionFeature IFeatureImplementation.ContextMeta => this;
        IMetaFunctionFeature IFeatureImplementation.Meta => null;
        IFunctionFeature IFeatureImplementation.Function => null;
        ISimpleFeature IFeatureImplementation.Simple => null;

        Result IContextMetaFunctionFeature.Result(ContextBase callContext, Category category, CompileSyntax right)
            => callContext.Result(category, _definition.ReplaceArg(right));
    }
}