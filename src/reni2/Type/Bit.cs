using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Basics;

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
    }
}