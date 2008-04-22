using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    /// Container of a leaf
    /// </summary>
    internal sealed class Leaf : CodeBase
    {
        private readonly LeafElement _leafElement;

        public Leaf(LeafElement leafElement)
        {
            _leafElement = leafElement;
        }

        public override Size Size { get { return LeafElement.Size; } }
        public LeafElement LeafElement { get { return _leafElement; } }
        public override bool IsEmpty { get { return LeafElement.IsEmpty; } }
        public override RefAlignParam RefAlignParam { get { return LeafElement.RefAlignParam; } }

        public override CodeBase CreateChild(LeafElement leafElement)
        {
            var newLeafElement = LeafElement.TryToCombine(leafElement);
            if(newLeafElement != null)
                return new Leaf(newLeafElement);

            return base.CreateChild(leafElement);
        }

        public override Result VirtVisit<Result>(Visitor<Result> actual)
        {
            return actual.Leaf(LeafElement);
        }

        internal override BitsConst Evaluate()
        {
            return LeafElement.Evaluate();
        }
    }
}