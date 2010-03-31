using System;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Feature;

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

    internal class FunctionDefinitionType : TypeBase
    {
        [DumpData(true)]
        private readonly IFunctionalFeature _feature;

        public FunctionDefinitionType(IFunctionalFeature feature)
        {
            _feature = feature;
        }

        public IFeature ContextOperatorFeature
        {
            get { throw new NotImplementedException(); } }

        protected override Size GetSize() { return Size.Zero; }
        internal override string DumpShort() { return _feature.DumpShort(); }
        internal override IFunctionalFeature GetFunctionalFeature() { return _feature; }
    }
}