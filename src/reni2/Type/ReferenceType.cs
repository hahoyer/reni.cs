using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.TokenClasses;

namespace Reni.Type
{
    internal abstract class ReferenceType: TypeBase, IReference
    {
        private static int _nextObjectId;
        private readonly TypeBase _valueType;

        protected ReferenceType(TypeBase valueType)
            : base(_nextObjectId++)
        {
            Tracer.Assert(!valueType.Size.IsZero);
            _valueType = valueType;
        }

        internal TypeBase ValueType { get { return _valueType; } }

        [DisableDump]
        internal abstract RefAlignParam RefAlignParam { get; }

        internal override string DumpPrintText { get { return DumpShort(); } }

        protected override Size GetSize() { return RefAlignParam.RefSize; }

        internal override bool IsRef(RefAlignParam refAlignParam)
        {
            Tracer.Assert(RefAlignParam == refAlignParam);
            return true;
        }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            _valueType.Search(searchVisitor.Child(this));
            _valueType.Search(searchVisitor);
            base.Search(searchVisitor);
        }

        internal override int SequenceCount(TypeBase elementType) { return ValueType.SequenceCount(elementType); }
        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionParameter conversionParameter) { return ValueType.IsConvertableTo(dest, conversionParameter); }
        internal override TypeBase TypeForTypeOperator() { return ValueType.TypeForTypeOperator(); }
    }
}