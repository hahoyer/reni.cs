using HWClassLibrary.Debug;
using Reni.Parser;
using Reni.Parser.TokenClass;

namespace Reni.Context
{
    /// <summary>
    /// Environment with parent
    /// </summary>
    internal abstract class Child : ContextBase
    {
        readonly ContextBase _parent;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="parent"></param>
        public Child(ContextBase parent)
        {
            _parent = parent;
        }
        /// <summary>
        /// asis
        /// </summary>
        public ContextBase Parent { get { return _parent; } }

        /// <summary>
        /// Parameter to describe alignment for references
        /// </summary>
        public sealed override RefAlignParam RefAlignParam{get { return Parent.RefAlignParam; }}
        /// <summary>
        /// Return the root env
        /// </summary>
        [DumpData(false)]
        public sealed override Root RootContext { get { return Parent.RootContext; } }

        internal override bool IsChildOf(ContextBase context)
        {
            if(context == Parent)
                return true;
            return Parent.IsChildOf(context);
        }
    }
}
