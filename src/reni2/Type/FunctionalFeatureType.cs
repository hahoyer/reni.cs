using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Struct;
using Reni.Syntax;

namespace Reni.Type
{
    internal sealed class FunctionalFeatureType<TFeature> : TypeBase
        where TFeature: IFunctionalFeature
    {
        private readonly TypeBase _objectType;
        private readonly TFeature _functionalFeature;

        internal FunctionalFeatureType(TypeBase objectType, TFeature functionalFeature)
        {
            _objectType = objectType;
            _functionalFeature = functionalFeature;
        }

        protected override Size GetSize() { return _objectType.Size; }

        internal override string DumpShort() { return base.DumpShort()+"(" + _functionalFeature.DumpShort() + ")"; }
        internal override IFunctionalFeature FunctionalFeature() { return _functionalFeature; }
        internal override TypeBase ObjectType() { return _objectType; }
    }

}