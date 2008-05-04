using Reni.Code;

namespace Reni.Type
{
    internal sealed class TypeType : Primitive
    {
        private readonly TypeBase _parent;

        public TypeType(TypeBase parent)
        {
            _parent = parent;
        }

        internal override Size Size { get { return Size.Zero; } }
        internal override string DumpPrintText { get { return "(" + _parent.DumpPrintText + "()) type"; } }

        internal override Result ConvertToItself(Category category)
        {
            return CreateVoidResult(category);
        }

        internal override Result DumpPrint(Category category)
        {
            return Void.CreateResult(category, DumpPrintCode);
        }

        private CodeBase DumpPrintCode()
        {
            return CodeBase.CreateDumpPrintText(_parent.DumpPrintText);
        }
    }
}