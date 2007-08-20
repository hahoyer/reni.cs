using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    /// Container of a leaf
    /// </summary>
    sealed class Leaf : Base
    {
        private readonly LeafElement _leafElement;

        /// <summary>
        /// Gets the element.
        /// </summary>
        /// <value>The element.</value>
        /// created 05.10.2006 23:33
        public LeafElement LeafElement { get { return _leafElement; } }

        /// <summary>
        /// Creates the child.
        /// </summary>
        /// <param name="leafElement">The leaf element.</param>
        /// <returns></returns>
        /// created 06.10.2006 00:20
        public override Base CreateChild(LeafElement leafElement)
        {
            LeafElement newLeafElement = LeafElement.TryToCombine(leafElement);
            if (newLeafElement != null)
                return new Leaf(newLeafElement);

            return base.CreateChild(leafElement);
        }

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        /// created 23.09.2006 14:15
        public override Size Size { get { return LeafElement.Size; } }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        /// created 23.09.2006 14:23
        public override bool IsEmpty { get { return LeafElement.IsEmpty; } }

        /// <summary>
        /// Gets the ref align param.
        /// </summary>
        /// <value>The ref align param.</value>
        /// created 19.10.2006 19:46
        public override RefAlignParam RefAlignParam { get { return LeafElement.RefAlignParam; } }

        /// <summary>
        /// Visitor to replace parts of code, overridable version.
        /// </summary>
        /// <param name="actual">The actual.</param>
        /// <returns></returns>
        public override Result VirtVisit<Result>(Visitor<Result> actual)
        {
            return actual.Leaf(LeafElement);
        }

        internal override BitsConst Evaluate()
        {
            return LeafElement.Evaluate();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Leaf"/> class.
        /// </summary>
        /// <param name="leafElement">The element.</param>
        /// created 05.10.2006 23:33
        public Leaf(LeafElement leafElement)
        {
            _leafElement = leafElement;
        }
    }
}