using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;

namespace Reni.Type
{
    [Serializable]
    internal sealed class Bit : TypeBase
    {
        protected override Size GetSize() { return Size.Create(1); }

        internal override string DumpPrintText { get { return "bit"; } }
        internal override int SequenceCount(TypeBase elementType) { return 1; }

        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionParameter conversionParameter)
        {
            if(conversionParameter.IsUseConverter)
                return dest.HasConverterFromBit();

            return false;
        }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            searchVisitor.ChildSearch(this);
            base.Search(searchVisitor);
        }

        protected override string Dump(bool isRecursion) { return GetType().FullName; }

        internal override string DumpShort() { return "bit"; }
    }

    internal interface ISequenceOfBitPrefixOperation
    {
        [IsDumpEnabled(false)]
        string CSharpNameOfDefaultOperation { get; }

        [IsDumpEnabled(false)]
        string DataFunctionName { get; }

        Result SequenceOperationResult(Category category, Size objSize);
    }

    internal interface ISequenceOfBitBinaryOperation
    {
        int ResultSize(int objBitCount, int argBitCount);
        bool IsCompareOperator { get; }
        string DataFunctionName { get; }
        string CSharpNameOfDefaultOperation { get; }
    }
}