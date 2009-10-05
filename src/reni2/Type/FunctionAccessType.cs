using System;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;

namespace Reni.Type
{
    internal class FunctionAccessType : TypeBase
    {
        [DumpData(true)]
        private readonly TypeBase _objectType;

        [DumpData(true)]
        private readonly IFunctionalFeature _feature;

        public FunctionAccessType(TypeBase objectType, IFunctionalFeature feature)
        {
            _objectType = objectType;
            _feature = feature;
        }

        protected override Size GetSize() { return _objectType.Size; }
        internal override string DumpShort() { return _objectType.DumpShort() + " " + _feature.DumpShort(); }
        internal override IFunctionalFeature GetFunctionalFeature() { return _feature; }
        internal override TypeBase StripFunctional() { return _objectType; }
    }
}