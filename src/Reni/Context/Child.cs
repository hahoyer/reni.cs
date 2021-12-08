using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Feature;
using Reni.Struct;

namespace Reni.Context
{
    abstract class Child : ContextBase
    {
        [DisableDump]
        readonly ContextBase Parent;

        internal Child(ContextBase parent) => Parent = parent;

        protected abstract string GetContextChildIdentificationDump();

        public sealed override string GetContextIdentificationDump()
            => Parent.GetContextIdentificationDump() + GetContextChildIdentificationDump();

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
            return result.Any()? result : Parent.Declarations(tokenClass);
        }

        [DisableDump]
        internal override IEnumerable<ContextBase> ParentChain
            => Parent.ParentChain.Concat(base.ParentChain);
    }
}