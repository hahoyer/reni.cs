using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Basics;
using Reni.Code;

namespace Reni.Type
{
    [Serializable]
    internal sealed class Bit : TypeBase
    {
        protected override Size GetSize() { return Size.Create(1); }

        internal override string DumpPrintText { get { return "bit"; } }
        internal override int SequenceCount(TypeBase elementType) { return 1; }

        internal bool VirtualIsConvertable(TypeBase destination, ConversionParameter conversionParameter)
        {
            if(conversionParameter.IsUseConverter)
                return destination.HasConverterFromBit();

            return false;
        }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            searchVisitor.ChildSearch(this);
            base.Search(searchVisitor);
        }

        protected override string Dump(bool isRecursion) { return GetType().PrettyName(); }

        internal override string DumpShort() { return "bit"; }

        internal static CodeBase BitSequenceOperation(Size size, ISequenceOfBitBinaryOperation token, int objectBits, int argsBits)
        {
            var objectType = Number(objectBits).Align(BitsConst.SegmentAlignBits);
            var argsType = Number(argsBits).Align(BitsConst.SegmentAlignBits);
            return objectType
                .Pair(argsType)
                .ArgCode()
                .BitSequenceOperation(token, size, Size.Create(objectBits).ByteAlignedSize);
        }
    }
}