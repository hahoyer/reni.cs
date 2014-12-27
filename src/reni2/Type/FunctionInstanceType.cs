using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using Reni.Struct;

namespace Reni.Type
{
    sealed class FunctionInstanceType : TagChild<TypeBase>
    {
        public FunctionInstanceType(TypeBase parent)
            : base(parent) { }
        [DisableDump]
        protected override string TagTitle { get { return "function_instance"; } }
        [DisableDump]
        internal override CompoundView FindRecentCompoundView { get { return Parent.FindRecentCompoundView; } }
    }
}