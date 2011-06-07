using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Code;
using Reni.Feature;
using Reni.Syntax;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Context
{
    internal sealed class StructContext : ReniObject
    {
        private readonly Struct.Context _context;
        private readonly ContextBase _parent;

        [Node]
        [SmartNode]
        private readonly DictionaryEx<ICompileSyntax, FunctionalFeatureType> _functionalFeatureCache;

        private readonly SimpleCache<Struct.Type> _typeCache;

        internal StructContext(Struct.Context context, ContextBase parent)
        {
            _context = context;
            _parent = parent;
            _functionalFeatureCache = new DictionaryEx<ICompileSyntax, FunctionalFeatureType>(body => new FunctionalFeatureType(this, body));
            _typeCache = new SimpleCache<Struct.Type>(() => new Struct.Type(this));
        }

        [IsDumpEnabled(false)]
        internal int Position
        {
            get { return _context.Position; }
        }
        [IsDumpEnabled(false)]
        internal Struct.Type ContextType { get { return _typeCache.Value; } }

        [IsDumpEnabled(false)]
        internal Reference ContextReferenceType { get { return ContextType.Reference(RefAlignParam); } }

        [IsDumpEnabled(false)]
        internal Size StructSize { get { return _context.InnerSize(_parent); } }

        [IsDumpEnabled(false)]
        internal TypeBase InnerType { get { return _context.InnerType(_parent); } }

        [IsDumpEnabled(false)]
        internal Size InnerOffset { get { throw new NotImplementedException(); } }

        [IsDumpEnabled(false)]
        internal RefAlignParam RefAlignParam { get { return _parent.RefAlignParam; } }

        [IsDumpEnabled(false)]
        internal TypeBase IndexType { get { return TypeBase.Number(IndexSize); } }

        [IsDumpEnabled(false)]
        private int IndexSize { get { return _context.IndexSize; } }

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

        internal ISearchPath<IFeature, Struct.Type> SearchFromRefToStruct(Defineable defineable)
        {
            return _context.SearchFromRefToStruct(defineable);
        }

        internal IReferenceInCode ReferenceInCode { get { return _parent.SpawnStruct(_context); } }

        internal Result ContextReferenceAsArg(Category category) { return _context.ContextReferenceAsArg(_parent, category); }

        internal Result AccessResultFromArg(Category category, int position)
        {
            return AccessResult(category, position)
                .ReplaceContextReferenceByArg(this);
        }

        private Result AccessResult(Category category, int position)
        {
            var thisResult = ThisReferenceResult(category | Category.Type);
            return _context
                .SpawnField(_parent)
                .Result(category)
                .ReplaceArg(thisResult);
        }

        private Result AccessResult(bool isProperty, int position, Category category)
        {
            if (isProperty)
            {
                return AccessResult(Category.Type, position)
                    .Type
                    .Apply(category, TypeBase.VoidResult, RefAlignParam);
            }
            return AccessResult(category, position);
        }

        private CodeBase ContextCode()
        {
            return CodeBase
                .ReferenceInCode(ReferenceInCode)
                .AddToReference(RefAlignParam, StructSize*-1, "ContextCode");
        }

        private Refs ContextRefs() { return Refs.Create(ReferenceInCode); }
    }
}