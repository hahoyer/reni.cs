using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Feature;
using Reni.Parser.TokenClass;

namespace Reni.Context
{
    /// <summary>
    /// Environment with parent
    /// </summary>
    internal abstract class Child : ContextBase
    {
        private readonly ContextBase _parent;

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
        public override sealed RefAlignParam RefAlignParam { get { return Parent.RefAlignParam; } }
        /// <summary>
        /// Return the root env
        /// </summary>
        [DumpData(false)]
        public override sealed Root RootContext { get { return Parent.RootContext; } }

        protected override Sequence<ContextBase> ObtainChildChain()
        {
            return Parent.ChildChain + this;
        }

        /// <summary>
        /// Creates the args ref result.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// created 03.11.2006 22:00
        public override Result CreateArgsRefResult(Category category)
        {
            return Parent.CreateArgsRefResult(category);
        }

        internal override SearchResult<IContextFeature> Search(Defineable defineable)
        {
            return Parent.Search(defineable).SubTrial(Parent);
        }
    }
}