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

        [Node]
        internal ContextBase Parent { get { return _parent; } }

        internal override sealed RefAlignParam RefAlignParam { get { return Parent.RefAlignParam; } }

        [DumpData(false)]
        internal override sealed Root RootContext { get { return Parent.RootContext; } }

        sealed internal protected override Sequence<ContextBase> ObtainChildChain()
        {
            return Parent.ChildChain + this;
        }

        internal override Result CreateArgsRefResult(Category category)
        {
            return Parent.CreateArgsRefResult(category);
        }

        internal override SearchResult<IContextFeature> Search(Defineable defineable)
        {
            return Parent.Search(defineable).SubTrial(Parent);
        }
    }
}