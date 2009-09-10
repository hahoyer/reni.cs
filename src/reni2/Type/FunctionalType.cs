using System;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;

namespace Reni.Type
{
    internal class FunctionalType : TypeBase
    {
        [DumpData(true)]
        private readonly TypeBase _objectType;

        [DumpData(true)]
        private readonly IFunctionalFeature _feature;

        public FunctionalType(TypeBase objectType, IFunctionalFeature feature)
        {
            _objectType = objectType;
            _feature = feature;
        }

        protected override Size GetSize() { return _objectType.Size; }
        internal override string DumpShort() { return _objectType.DumpShort() + " " + _feature.DumpShort(); }
        internal override IFunctionalFeature FunctionalFeature { get { return _feature; } }
        internal override TypeBase StripFunctional() { return _objectType; }
    }
}