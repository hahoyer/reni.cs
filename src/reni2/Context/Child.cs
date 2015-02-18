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
        public override sealed string GetContextIdentificationDump()
            => Parent.GetContextIdentificationDump() 
            + "->" 
            + GetContextChildIdentificationDump();

        protected abstract string GetContextChildIdentificationDump();

        [DisableDump]
        readonly ContextBase _parent;

        internal Child(ContextBase parent) { _parent = parent; }

        [Node]
        internal ContextBase Parent => _parent;

        [DisableDump]
        internal override Root RootContext => Parent.RootContext;
        public override sealed string DumpPrintText => ChildDumpPrintText + (Parent == RootContext ? "" : " in " + _parent.DumpPrintText);
        protected abstract string ChildDumpPrintText { get; }
        internal override CompoundView ObtainRecentCompoundView() => Parent.ObtainRecentCompoundView();
        internal override IFunctionContext ObtainRecentFunctionContext() => Parent.ObtainRecentFunctionContext();
        internal override IEnumerable<ContextSearchResult> Declarations<TDefinable>(TDefinable tokenClass)
        {
            var result = base.Declarations(tokenClass).ToArray();
            return result.Any() ? result : Parent.Declarations(tokenClass);
        }
    }
}