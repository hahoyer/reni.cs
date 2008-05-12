namespace Reni.Type
{
    /// <summary>
    /// Primitive type, no destructor or move handler.
    /// </summary>
    internal abstract class Primitive : TypeBase
    {
        protected Primitive()
        {
        }

        protected Primitive(int objectId)
            : base(objectId)
        {
        }

        internal override sealed Result DestructorHandler(Category category)
        {
            return EmptyHandler(category);
        }

        internal override sealed Result ArrayDestructorHandler(Category category, int count)
        {
            return EmptyHandler(category);
        }

        internal override sealed Result MoveHandler(Category category)
        {
            return EmptyHandler(category);
        }

        internal override sealed Result ArrayMoveHandler(Category category, int count)
        {
            return EmptyHandler(category);
        }
    }
}