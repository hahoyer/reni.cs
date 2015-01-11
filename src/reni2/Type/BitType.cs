using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Feature.DumpPrint;

namespace Reni.Type
{
    sealed class BitType
        : TypeBase
            , ISymbolProvider<DumpPrintToken, IFeatureImplementation>
    {
        internal BitType(Root rootContext) { RootContext = rootContext; }

        [DisableDump]
        internal override bool Hllw => false;

        [DisableDump]
        internal override string DumpPrintText => "bit";

        [DisableDump]
        internal override Root RootContext { get; }

        protected override Size GetSize() => Size.Create(1);

        protected override string Dump(bool isRecursion) => GetType().PrettyName();

        protected override string GetNodeDump() => "bit";
        internal NumberType UniqueNumber(int bitCount) => UniqueArray(bitCount).UniqueNumber;
        internal Result Result(Category category, BitsConst bitsConst)
        {
            return UniqueNumber(bitsConst.Size.ToInt())
                .Result(category, () => CodeBase.BitsConst(bitsConst));
        }

        IFeatureImplementation ISymbolProvider<DumpPrintToken, IFeatureImplementation>.Feature(DumpPrintToken tokenClass)
            => Extension.SimpleFeature(DumpPrintTokenResult, this);

        protected override CodeBase DumpPrintCode()
            => UniquePointer
                .ArgCode
                .DePointer(Size)
                .DumpPrintNumber();
    }
}