using System;
using HWClassLibrary.Helper.TreeViewSupport;

namespace Reni.Type
{
    public abstract class Child : Base
    {
        readonly Base _parent;

        protected Child(Base parent)
        {
            _parent = parent;
        }

        public Child(int objectId, Base parent): base(objectId)
        {
            _parent = parent;
        }

        [Node]
        public Base Parent { get { return _parent; } }

    }
    public abstract class TagChild : Child
    {
        protected TagChild(Base parent)
            : base(parent)
        {
        }

        public TagChild(int objectId, Base parent)
            : base(objectId,parent)
        {
        }

        /// <summary>
        /// The size of type
        /// </summary>
        public override Size Size { get { return Parent.Size; } }

        /// <summary>
        /// Gets the dump print text.
        /// </summary>
        /// <value>The dump print text.</value>
        /// created 08.01.2007 17:54
        public override string DumpPrintText { get { return Parent.DumpPrintText + " #(# " + TagTitle + " #)#"; } }

        abstract protected string TagTitle { get;}

        /// <summary>
        /// Destructors the specified category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// [created 02.06.2006 09:47]
        internal override Result DestructorHandler(Category category)
        {
            return Parent.DestructorHandler(category);
        }

        /// <summary>
        /// Arrays the destructor.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        /// [created 04.06.2006 00:51]
        internal override Result ArrayDestructorHandler(Category category, int count)
        {
            return Parent.ArrayDestructorHandler(category, count);
        }

        /// <summary>
        /// Moves the handler.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// [created 05.06.2006 16:47]
        internal override Result MoveHandler(Category category)
        {
            return Parent.MoveHandler(category);
        }

        /// <summary>
        /// Arrays the move handler.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        /// [created 05.06.2006 16:54]
        internal override Result ArrayMoveHandler(Category category, int count)
        {
            return Parent.ArrayMoveHandler(category, count);
        }
    }
}