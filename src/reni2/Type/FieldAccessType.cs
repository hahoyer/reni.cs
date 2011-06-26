using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.Struct;

namespace Reni.Type
{
    internal sealed class FieldAccessType : TypeBase
    {
        private static int _nextObjectId;
        private readonly TypeBase _valueType;
        private readonly ContainerContextObject _containerContextObject;
        private readonly int _position;

        public FieldAccessType(TypeBase valueType, ContainerContextObject containerContextObject, int position)
            : base(_nextObjectId++)
        {
            _valueType = valueType;
            _containerContextObject = containerContextObject;
            _position = position;
        }

        [DisableDump]
        internal RefAlignParam RefAlignParam { get { return _containerContextObject.RefAlignParam; } }

        internal ContainerContextObject ContainerContextObject { get { return _containerContextObject; } }
        internal TypeBase ValueType { get { return _valueType; } }
        internal int Position { get { return _position; } }

        internal Result DumpPrintOperationResult(Category category) { return OperationResult<IFeature>(category, new DumpPrintToken(), RefAlignParam); }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            _valueType.Search(searchVisitor.Child(this));
            _valueType.Search(searchVisitor);
            base.Search(searchVisitor);
        }

        protected override Result ConvertToImplementation(Category category, TypeBase dest)
        {
            var result = ValueType.Conversion(category, dest);
            NotImplementedMethod(category, dest);
            return null;
        }

        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionParameter conversionParameter)
        {
            return ValueType.IsConvertableTo(dest, conversionParameter);
        }

        protected override Size GetSize() { return _containerContextObject.RefAlignParam.RefSize; }

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