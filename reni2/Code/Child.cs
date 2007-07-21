using System;
using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Child : Base
    {
        private readonly LeafElement _leafElement;
        private readonly Base _parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Child"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="leafElement">The leaf element.</param>
        /// created 29.09.2006 00:14
        public Child(Base parent, LeafElement leafElement)
        {
            _parent = parent;
            _leafElement = leafElement;
        }


        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        /// created 23.09.2006 14:15
        public override Size Size { get { return LeafElement.Size; } }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>The parent.</value>
        /// created 24.09.2006 15:22
        public Base Parent { get { return _parent; } }

        /// <summary>
        /// Gets the leaf.
        /// </summary>
        /// <value>The leaf.</value>
        /// created 05.10.2006 23:21
        public LeafElement LeafElement { get { return _leafElement; } }

        /// <summary>
        /// Gets the ref align param.
        /// </summary>
        /// <value>The ref align param.</value>
        /// created 19.10.2006 19:46
        /// created 19.10.2006 19:46
        public override RefAlignParam RefAlignParam
        {
            get
            {
                RefAlignParam result = LeafElement.RefAlignParam;
                if (result != null)
                    return result;
                return Parent.RefAlignParam;
            }
        }

        public override Refs Refs { get { return _parent.Refs; } }

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
                return Parent.CreateChild(newLeafElement);

            return base.CreateChild(leafElement);
        }

        /// <summary>
        /// Gets the size of the max.
        /// </summary>
        /// <value>The size of the max.</value>
        /// created 23.09.2006 14:13
        public override Size MaxSize { get { return Parent.MaxSize.Max(LeafElement.Size); } }

        /// <summary>
        /// Visitor to replace parts of code, overridable version.
        /// </summary>
        /// <param name="actual">The actual.</param>
        /// <returns></returns>
        public override T VirtVisit<T>(Visitor<T> actual)
        {
            return actual.ChildVisit(this);
        }

        /// <summary>
        /// Adds to.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns></returns>
        /// created 05.10.2006 23:37
        public int AddTo(Container container)
        {
            NotImplementedMethod(container);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Res the create.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <returns></returns>
        /// created 05.10.2006 23:37
        public Base ReCreate(Base parent)
        {
            NotImplementedMethod(parent);
            throw new NotImplementedException();
        }
    }
}