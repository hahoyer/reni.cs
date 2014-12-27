using System.Collections.Generic;
using System.Linq;
using System;
using hw.Debug;
using hw.Forms;
using Reni.Feature;
using Reni.Struct;

namespace Reni.Context
{
    abstract class Child : ContextBase
    {
        [DisableDump]
        readonly ContextBase _parent;

        internal Child(ContextBase parent) { _parent = parent; }

        [Node]
        internal ContextBase Parent { get { return _parent; } }

        [DisableDump]
        internal override Root RootContext { get { return Parent.RootContext; } }
        public override sealed string DumpPrintText
        {
            get { return ChildDumpPrintText + (Parent == RootContext ? "" : " in " + _parent.DumpPrintText); }
        }
        protected abstract string ChildDumpPrintText { get; }
        internal override CompoundView ObtainRecentStructure() { return Parent.ObtainRecentStructure(); }
        internal override IFunctionContext ObtainRecentFunctionContext() { return Parent.ObtainRecentFunctionContext(); }
        internal override IEnumerable<ContextCallDescriptor> Declarations<TDefinable>(TDefinable tokenClass)
        {
            var result = base.Declarations(tokenClass).ToArray();
            return result.Any() ? result : Parent.Declarations(tokenClass);
        }
    }
}