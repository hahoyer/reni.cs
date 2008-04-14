using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper.TreeViewSupport;

namespace Reni.Type
{
    internal abstract class Child : Base
    {
        readonly Base _parent;

        protected Child(Base parent)
        {
            _parent = parent;
        }

        protected Child(int objectId, Base parent): base(objectId)
        {
            _parent = parent;
        }

        [Node, DumpData(true)]
        public Base Parent { get { return _parent; } }

        sealed internal protected override Base FindDefiningParent { get { return Parent; } }
    }

    internal abstract class TagChild : Child
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
        internal override string DumpPrintText { get { return Parent.DumpPrintText + " #(# " + TagTitle + " #)#"; } }

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

        /// <summary>
        /// Converts to.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="dest">The dest.</param>
        /// <returns></returns>
        /// created 11.01.2007 22:12
        internal override Result ConvertToVirt(Category category, Base dest)
        {
            return Parent.ConvertToVirt(category, dest);
        }

        /// <summary>
        /// Determines whether [is convertable to virt] [the specified dest].
        /// </summary>
        /// <param name="dest">The dest.</param>
        /// <param name="conversionFeature">The conversion feature.</param>
        /// <returns>
        /// 	<c>true</c> if [is convertable to virt] [the specified dest]; otherwise, <c>false</c>.
        /// </returns>
        /// created 30.01.2007 22:42
        internal override bool IsConvertableToVirt(Base dest, ConversionFeature conversionFeature)
        {
            return Parent.IsConvertableToVirt(dest, conversionFeature);
        }
    }
}