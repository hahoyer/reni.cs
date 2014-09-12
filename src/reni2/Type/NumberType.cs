using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.Sequence;
using Reni.TokenClasses;

namespace Reni.Type
{
    sealed class NumberType
        : TypeBase
            , ISymbolProvider<DumpPrintToken, IFeatureImplementation>
    {
        readonly FunctionCache<RefAlignParam, ObjectReference> _objectReferencesCache;
        readonly ArrayType _parent;

        public NumberType(ArrayType parent)
        {
            _parent = parent;
            _objectReferencesCache = new FunctionCache<RefAlignParam, ObjectReference>
                (refAlignParam => new ObjectReference(this, refAlignParam));
        }

        [DisableDump]
        internal override Root RootContext { get { return _parent.RootContext; } }
        protected override Size GetSize() { return _parent.Size; }
        [DisableDump]
        internal override bool IsDataLess { get { return _parent.IsDataLess; } }

        internal ObjectReference UniqueObjectReference(RefAlignParam refAlignParam)
        {
            return _objectReferencesCache[refAlignParam];
        }

        IFeatureImplementation ISymbolProvider<DumpPrintToken, IFeatureImplementation>.Feature
        {
            get { return Extension.Feature(DumpPrintTokenResult); }
        }

        internal override SearchResult DeclarationsForType(Defineable tokenClass) { return tokenClass.Declarations(this); }

        Result DumpPrintTokenResult(Category category)
        {
            return VoidType.Result(category, DumpPrintNumberCode, CodeArgs.Arg);
        }
    }
}