using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Context;

namespace Reni.Type
{
    internal sealed class ConversionFeature : FunctionalFeature
    {
        private readonly TypeBase _objectType;

        public ConversionFeature(TypeBase objectType) { _objectType = objectType; }

        internal override string DumpShort() { return _objectType.DumpShort() + " type"; }
        protected override Result ObtainApplyResult(Category category, TypeBase argsType, RefAlignParam refAlignParam) { return argsType.Conversion(category, _objectType); }
        protected override TypeBase ObjectType { get { return _objectType; } }
    }
}