using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Code;
using Reni.Feature;

namespace Reni.Type
{
    sealed class EnableCut
        : TagChild<TypeBase>
            , IForcedConversionProvider<NumberType>
    {
        internal EnableCut(TypeBase parent)
            : base(parent)
        {
            Tracer.Assert(Parent.IsCuttingPossible, Parent.Dump);
        }

        [DisableDump]
        protected override string TagTitle { get { return "enable_cut"; } }

        IEnumerable<ISimpleFeature> IForcedConversionProvider<NumberType>.Result(NumberType destination)
        {
            return Parent.CutEnabledConversion(destination);
        }
    }

    sealed class EnableReassignType : DataSetterTargetType
    {
        readonly TypeBase _parent;
        public EnableReassignType(TypeBase parent)
        {
            Tracer.Assert(!parent.Hllw);
            _parent = parent;
        }
        internal override TypeBase ValueType { get { return _parent; } }
        protected override CodeBase SetterCode()
        {
            NotImplementedMethod();
            return null;
        }
        protected override CodeBase GetterCode()
        {
            NotImplementedMethod();
            return null;
        }
        internal override bool Hllw { get { return false; } }
    }
}