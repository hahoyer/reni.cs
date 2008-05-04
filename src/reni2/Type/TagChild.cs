namespace Reni.Type
{
    internal abstract class TagChild : Child
    {
        protected TagChild(TypeBase parent)
            : base(parent)
        {
        }

        protected TagChild(int objectId, TypeBase parent)
            : base(objectId,parent)
        {
        }

        abstract protected string TagTitle { get; }
        internal override Size Size { get { return Parent.Size; } }

        public override string DumpShort()
        {
            return Parent.DumpShort() + "[" + TagTitle + "]";
        }

        internal override string DumpPrintText { get { return Parent.DumpPrintText + " #(# " + TagTitle + " #)#"; } }

        internal override Result DestructorHandler(Category category)
        {
            return Parent.DestructorHandler(category);
        }

        internal override Result ArrayDestructorHandler(Category category, int count)
        {
            return Parent.ArrayDestructorHandler(category, count);
        }

        internal override Result MoveHandler(Category category)
        {
            return Parent.MoveHandler(category);
        }

        internal override Result ArrayMoveHandler(Category category, int count)
        {
            return Parent.ArrayMoveHandler(category, count);
        }

        internal override Result ConvertToVirt(Category category, TypeBase dest)
        {
            return Parent.ConvertTo(category, dest);
        }

        internal override bool IsConvertableToVirt(TypeBase dest, ConversionFeature conversionFeature)
        {
            return Parent.IsConvertableTo(dest, conversionFeature);
        }
    }
}