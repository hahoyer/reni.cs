using System;
using Reni.Code;

namespace Reni.Type
{
    [Serializable]
    internal sealed class TypeType : TypeBase
    {
        internal readonly TypeBase Value;

        public TypeType(TypeBase value)
        {
            Value = value;
        }

        protected override Size GetSize()
        {
            return Size.Zero;
        }

        internal override string DumpPrintText { get { return "(" + Value.DumpPrintText + "()) type"; } }

        protected override Result ConvertToItself(Category category)
        {
            return CreateVoidResult(category);
        }

        internal override Result DumpPrint(Category category)
        {
            return Void.CreateResult(category, DumpPrintCode);
        }

        private CodeBase DumpPrintCode()
        {
            return CodeBase.CreateDumpPrintText(Value.DumpPrintText);
        }

        internal override string DumpShort()
        {
            return "(" + Value.DumpShort() + ") type"; 
        }
    }
}