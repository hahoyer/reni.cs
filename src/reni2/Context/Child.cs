using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Feature;
using Reni.Struct;

namespace Reni.Context
{
    abstract class Child : ContextBase
    {
        public sealed override string GetContextIdentificationDump()
            => Parent.GetContextIdentificationDump()
                + GetContextChildIdentificationDump();

        protected abstract string GetContextChildIdentificationDump();

        [DisableDump]
        readonly ContextBase _parent;

        internal Child(ContextBase parent) { _parent = parent; }

        [Node]
        [DisableDump]
        internal ContextBase Parent => _parent;

        [DisableDump]
        internal override bool IsRecursionMode => Parent.IsRecursionMode;

        [DisableDump]
        internal override Root RootContext => Parent.RootContext;

        internal override CompoundView ObtainRecentCompoundView()
            => Parent.ObtainRecentCompoundView();

        internal override IFunctionContext ObtainRecentFunctionContext()
            => Parent.ObtainRecentFunctionContext();

        internal override IEnumerable<IImplementation> Declarations<TDefinable>
            (TDefinable tokenClass)
        {
            var result = base.Declarations(tokenClass).ToArray();
            return result.Any() ? result : Parent.Declarations(tokenClass);
        }

        [DisableDump]
        internal override IEnumerable<ContextBase> ParentChain
            => _parent.ParentChain.Concat(base.ParentChain);
    }
}