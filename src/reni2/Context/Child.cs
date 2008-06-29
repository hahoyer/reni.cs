using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Feature;
using Reni.Parser.TokenClass;

namespace Reni.Context
{
    internal abstract class Child : ContextBase
    {
        private readonly ContextBase _parent;

        protected Child(ContextBase parent)
        {
            _parent = parent;
        }

        public ContextBase Parent { get { return _parent; } }

        public override sealed RefAlignParam RefAlignParam { get { return Parent.RefAlignParam; } }

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