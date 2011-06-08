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

        [Node]
        [SmartNode]
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
        }

        [IsDumpEnabled(false)]
        internal int Position { get { return _position; } }

        [IsDumpEnabled(false)]
        internal ContextBase SpawnContext { get { return _containerContext.SpawnContext(_position); } }

        [IsDumpEnabled(false)]
        internal Root RootContext { get { return _containerContext.RootContext; } }

        [IsDumpEnabled(false)]
        internal Type ContextType { get { return _typeCache.Value; } }

        [IsDumpEnabled(false)]
        internal Reference ContextReferenceType { get { return ContextType.Reference(RefAlignParam); } }

        [IsDumpEnabled(false)]
        internal Size StructSize { get { return _containerContext.InnerSize(_position); } }

        [IsDumpEnabled(false)]
        internal TypeBase InnerType { get { return _containerContext.InnerType(_position); } }

        [IsDumpEnabled(false)]
        internal Size InnerOffset { get { throw new NotImplementedException(); } }

        [IsDumpEnabled(false)]
        internal RefAlignParam RefAlignParam { get { return _containerContext.RefAlignParam; } }

        [IsDumpEnabled(false)]
        internal TypeBase IndexType { get { return TypeBase.Number(IndexSize); } }

        [IsDumpEnabled(false)]
        internal ContextPosition[] Features { get { return _containerContext.Features; } }

        [IsDumpEnabled(false)]
        private int IndexSize { get { return _containerContext.IndexSize; } }

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

        internal ISearchPath<IFeature, Type> SearchFromRefToStruct(Defineable defineable) { return _containerContext.Container.SearchFromRefToStruct(defineable); }

        internal IReferenceInCode ReferenceInCode { get { return SpawnContext; } }

        internal Result ContextReferenceAsArg(Category category)
        {
            return ContextReferenceType
                .ArgResult(category)
                .AddToReference(category, RefAlignParam, StructSize, "ContextReferenceAsArg");
        }

        internal Result AccessResultFromArg(Category category, int position)
        {
            return _parent
                .SpawnStruct(_context.Container, position)
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
    }
}