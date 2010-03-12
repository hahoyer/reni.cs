using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Context;
using Reni.Syntax;

namespace Reni.Type
{
    [Serializable]
    internal sealed class Bit : TypeBase
    {
        protected override Size GetSize() { return Size.Create(1); }

        internal override string DumpPrintText { get { return "bit"; } }
        internal override int GetSequenceCount(TypeBase elementType) { return 1; }

        internal override bool IsConvertableTo_Implementation(TypeBase dest, ConversionFeature conversionFeature)
        {
            if(conversionFeature.IsUseConverter)
                return dest.HasConverterFromBit();

            return false;
        }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            searchVisitor.Child(this).Search();
            base.Search(searchVisitor);
        }

        public override string Dump() { return GetType().FullName; }

        internal override string DumpShort() { return "bit"; }
        internal override bool IsValidRefTarget() { return true; }
    }

    internal interface ISequenceOfBitPrefixOperation 
    {
        [DumpData(false)]
        string CSharpNameOfDefaultOperation { get; }
        [DumpData(false)]
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