using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Syntax;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class PositionContainerContext : ReniObject
    {
        private readonly ContainerContext _containerContext;
        private readonly int _position;
        private readonly SimpleCache<PositionFeature> _propertyFeatureCache;
        private readonly SimpleCache<PositionFeature> _nonPropertyFeatureCache;
        private readonly DictionaryEx<ICompileSyntax, FunctionalFeatureType> _functionalFeatureCache;

        private readonly SimpleCache<Type> _typeCache;

        private readonly SimpleCache<Field> _fieldCache;

        internal PositionContainerContext(ContainerContext containerContext, int position)
        {
            _containerContext = containerContext;
            _position = position;
            _functionalFeatureCache = new DictionaryEx<ICompileSyntax, FunctionalFeatureType>(body => new FunctionalFeatureType(this, body));
            _typeCache = new SimpleCache<Type>(() => new Type(this));
            _fieldCache = new SimpleCache<Field>(() => new Field(this));
            _propertyFeatureCache = new SimpleCache<PositionFeature>(() => new PositionFeature(this, true));
            _nonPropertyFeatureCache = new SimpleCache<PositionFeature>(() => new PositionFeature(this, false));
        }

        [IsDumpEnabled(false)]
        internal int Position { get { return _position; } }

        [IsDumpEnabled(false)]
        internal ContainerContext ContainerContext { get { return _containerContext; } }

        [IsDumpEnabled(false)]
        internal ContextBase SpawnContext { get { return ContainerContext.SpawnContext(_position); } }

        [IsDumpEnabled(false)]
        internal Root RootContext { get { return ContainerContext.RootContext; } }

        [IsDumpEnabled(false)]
        internal Type ContextType { get { return _typeCache.Value; } }

        [IsDumpEnabled(false)]
        internal Reference ContextReferenceType { get { return ContextType.Reference(RefAlignParam); } }

        [IsDumpEnabled(false)]
        internal Size StructSize { get { return ContainerContext.InnerSize(_position); } }

        [IsDumpEnabled(false)]
        internal Refs ConstructorRefs
        {
            get
            {
                return ContainerContext
                    .Container
                    .InnerResult(Category.Refs, ContainerContext.Parent, Position).Refs;
            }
        }

        [IsDumpEnabled(false)]
        internal TypeBase InnerType { get { return ContainerContext.InnerType(_position); } }

        [IsDumpEnabled(false)]
        internal RefAlignParam RefAlignParam { get { return ContainerContext.RefAlignParam; } }

        [IsDumpEnabled(false)]
        internal TypeBase IndexType { get { return TypeBase.Number(IndexSize); } }

        [IsDumpEnabled(false)]
        private int IndexSize { get { return ContainerContext.IndexSize; } }

        [IsDumpEnabled(false)]
        internal Size InnerOffset { get { return ContainerContext.InnerOffset(Position); } }

        internal PositionFeature ToProperty(bool isProperty) { return isProperty ? _propertyFeatureCache.Value : _nonPropertyFeatureCache.Value; }

        internal Result FunctionalResult(Category category, ICompileSyntax body)
        {
            return _functionalFeatureCache
                .Find(body)
                .Result(category);
        }

        internal Result ThisReferenceResult(Category category)
        {
            return ContextReferenceType
                .Result(category, ContextCode, ContextRefs);
        }

        internal ISearchPath<IFeature, Type> SearchFromRefToStruct(Defineable defineable) { return ContainerContext.Container.SearchFromRefToStruct(defineable); }

        internal IReferenceInCode ReferenceInCode { get { return SpawnContext; } }

        internal Result ContextReferenceAsArg(Category category)
        {
            return ContextReferenceType
                .ArgResult(category)
                .AddToReference(category, RefAlignParam, StructSize, "ContextReferenceAsArg");
        }

        internal Result AccessResultFromArg(Category category, int position)
        {
            return ContainerContext.SpawnContext(position)
                .FindRecentStructContext
                .AccessResultFromArg(category);
        }

        internal Result AccessResultFromArg(Category category)
        {
            var accessResult = AccessResult(category);
            return accessResult
                .ReplaceAbsolute(ReferenceInCode, () => ContextReferenceAsArg(accessResult.CompleteCategory));
        }

        private Field SpawnField { get { return _fieldCache.Value; } }

        private Result AccessResult(Category category)
        {
            var thisResult = ThisReferenceResult(category | Category.Type);
            return SpawnField
                .Result(category)
                .ReplaceArg(thisResult);
        }

        private Result AccessResult(bool isProperty, Category category)
        {
            if(isProperty)
            {
                return AccessResult(Category.Type)
                    .Type
                    .Apply(category, TypeBase.VoidResult, RefAlignParam);
            }
            return AccessResult(category);
        }

        private CodeBase ContextCode()
        {
            return CodeBase
                .ReferenceInCode(ReferenceInCode)
                .AddToReference(RefAlignParam, StructSize*-1, "ContextCode");
        }

        private Refs ContextRefs() { return Refs.Create(ReferenceInCode); }

        internal Result CreateFunctionCall(Category category, ICompileSyntax body, Result argsResult) { return RootContext.CreateFunctionCall(this, category, body, argsResult); }

        internal Result PositionFeatureApply(Category category, bool isProperty, bool isContextFeature)
        {
            var trace = category.HasCode && isContextFeature;
            StartMethodDump(trace, category, isProperty, isContextFeature);
            var accessResult = AccessResult(isProperty, category);
            var result = accessResult;
            if (!isContextFeature)
                result = result.ReplaceContextReferenceByArg(this);
            Dump(trace, "accessCode", accessResult.Code, "result.Code", result.Code);
            return ReturnMethodDumpWithBreak(trace, result);
        }

        internal Result[] CreateArgCodes(Category category)
        {
            var result = new Result[Position];
            var size = Size.Zero;
            for(var i = 0; i < Position; i++)
            {
                result[i] = AutomaticDereference(ContainerContext.InnerType(i), size, category);
                size += ContainerContext.InnerSize(i).Align(RefAlignParam.AlignBits);
            }
            return result;
        }

        private Result AutomaticDereference(TypeBase type, Size offset, Category category)
        {
            var reference = type.Reference(RefAlignParam);
            var result = reference
                .Result(category, () => CreateRefArgCode().AddToReference(RefAlignParam, offset, "AutomaticDereference"));
            return result.AutomaticDereference() & category;
        }

        private CodeBase CreateRefArgCode() { return ContextType.Reference(RefAlignParam).ArgCode(); }

    }
}