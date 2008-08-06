using System;
using Reni.Code;

namespace Reni.Type
{
    [Serializable]
    internal sealed class TypeType : TypeBase
    {
        private readonly TypeBase _parent;

        public TypeType(TypeBase parent)
        {
            _parent = parent;
        }

        protected override Size GetSize()
        {
            return Size.Zero;
        }

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

        internal override string DumpShort()
        {
            return "(" + _parent.DumpShort() + ") type"; 
        }
    }
}