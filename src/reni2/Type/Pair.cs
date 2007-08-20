using System;
using HWClassLibrary.Helper.TreeViewSupport;

namespace Reni.Type
{
    /// <summary>
    /// Pair of types
    /// </summary>
    internal class Pair : Child
    {
        private readonly Base _second;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pair"/> class.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// created 19.11.2006 22:57
        public Pair(Base first, Base second) : base(first)
        {
            _second = second;
        }

        /// <summary>
        /// Gets the first.
        /// </summary>
        /// <value>The first.</value>
        /// created 19.11.2006 22:59
        public Base First { get { return Parent; } }
        /// <summary>
        /// Gets the second.
        /// </summary>
        /// <value>The second.</value>
        /// created 19.11.2006 22:59
        [Node]
        public Base Second { get { return _second; } }

        /// <summary>
        /// The size of type
        /// </summary>
        public override Size Size { get { return First.Size + Second.Size; } }

        /// <summary>
        /// Destructors the specified category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// [created 02.06.2006 09:47]
        internal override Result DestructorHandler(Category category)
        {
            Result firstHandler = First.DestructorHandler(category);
            Result secondHandler = Second.DestructorHandler(category);
            if (firstHandler.IsEmpty)
                return secondHandler;
            if (secondHandler.IsEmpty)
                return firstHandler;

            NotImplementedMethod(category);
            throw new NotImplementedException();
        }
    }
}