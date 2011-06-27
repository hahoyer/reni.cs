using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.Struct;

namespace Reni.Type
{
    internal sealed class AccessType : TypeBase
    {
        private static int _nextObjectId;
        private readonly TypeBase _valueType;
        private readonly Structure _accessPoint;
        private readonly int _position;

        public AccessType(TypeBase valueType, Structure accessPoint, int position)
            : base(_nextObjectId++)
        {
            _valueType = valueType;
            _accessPoint = accessPoint;
            _position = position;
        }

        [DisableDump]
        internal RefAlignParam RefAlignParam { get { return _accessPoint.RefAlignParam; } }

        internal Structure AccessPoint { get { return _accessPoint; } }
        internal TypeBase ValueType { get { return _valueType; } }
        internal int Position { get { return _position; } }

        internal Result DumpPrintOperationResult(Category category) { return OperationResult<IFeature>(category, new DumpPrintToken(), RefAlignParam); }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            _valueType.Search(searchVisitor.Child(this));
            _valueType.Search(searchVisitor);
            base.Search(searchVisitor);
        }

        protected override TypeBase ToReference(RefAlignParam refAlignParam) { return this; }

        protected override Result ConvertToImplementation(Category category, TypeBase dest)
        {
            return ValueType
                .SpawnReference(RefAlignParam)
                .Conversion(category, dest)
                .ReplaceArg(ValueReferenceViaFieldReference(category));
        }

        private Result ValueReferenceViaFieldReference(Category category)
        {
            var result = new Result
                (category
                 , () => RefAlignParam.RefSize
                 , () => ValueType.SpawnReference(RefAlignParam)
                 , () => CodeBase.Arg(this).AddToReference(RefAlignParam, AccessPoint.FieldOffsetFromThisReference(Position))
                 , Refs.None
                );
            return result;
        }

        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionParameter conversionParameter) { return ValueType.IsConvertableTo(dest, conversionParameter); }

        protected override Size GetSize() { return _accessPoint.RefAlignParam.RefSize; }
        internal override bool IsRef(RefAlignParam refAlignParam) { return refAlignParam == RefAlignParam; }

        internal override TypeBase AutomaticDereference()
        {
            NotImplementedMethod();
            return null;
            ;
        }

        protected override TypeBase Dereference()
        {
            NotImplementedMethod();
            return null;
            ;
        }

        protected override Result DereferenceResult(Category category)
        {
            NotImplementedMethod(category);
            return null;
            ;
        }
    }
}