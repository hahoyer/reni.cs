using Reni.Context;
using Reni.Parser;

namespace Reni.Type
{
    /// <summary>
    /// Primitive type, no destructor or move handler.
    /// </summary>
    internal abstract class Primitive : Base, IDefiningType
    {
        /// <summary>
        /// Destructors the specified category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// [created 02.06.2006 09:47]
        internal sealed override Result DestructorHandler(Category category)
        {
            return EmptyHandler(category);
        }

        /// <summary>
        /// Arrays the destructor.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        /// [created 04.06.2006 00:51]
        internal sealed override Result ArrayDestructorHandler(Category category, int count)
        {
            return EmptyHandler(category);
        }

        /// <summary>
        /// Moves the handler.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// [created 05.06.2006 16:47]
        internal sealed override Result MoveHandler(Category category)
        {
            return EmptyHandler(category);
        }

        /// <summary>
        /// Arrays the move handler.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        /// [created 05.06.2006 16:54]
        internal sealed override Result ArrayMoveHandler(Category category, int count)
        {
            return EmptyHandler(category);
        }

        internal protected override IDefiningType FindDefiningType
        {
             get { return this; }
        }

        public abstract SearchResult SearchDefinable(DefineableToken defineableToken, TypePath typePath);
        public abstract PrefixSearchResult SearchDefinablePrefix(DefineableToken defineableToken, TypePath typePath);
    }
}