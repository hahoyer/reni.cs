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
    internal sealed class FunctionalFeatureType<TObjectType, TFeature> : TypeBase
        where TObjectType: TypeBase
        where TFeature: IFunctionalFeature
    {
        private static int _nextObjectId;
        private readonly TObjectType _objectType;
        private readonly TFeature _functionalFeature;

        internal FunctionalFeatureType(TObjectType objectType, TFeature functionalFeature)
            : base(_nextObjectId++)
        {
            _objectType = objectType;
            _functionalFeature = functionalFeature;
        }

        protected override Size GetSize() { return _objectType.Size; }

        internal override string DumpShort() { return "(" + _objectType.DumpShort() + " " + _functionalFeature.DumpShort() + ")"; }
        internal override IFunctionalFeature FunctionalFeature() { return _functionalFeature; }
        internal override TypeBase ObjectType() { return _objectType; }
    }

}