using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;

namespace Reni.Type
{
    sealed class BitType
        : TypeBase
            , ISymbolProviderForPointer<DumpPrintToken, IFeatureImplementation>
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
        internal NumberType Number(int bitCount) => Array(bitCount).Number;
        internal Result Result(Category category, BitsConst bitsConst)
        {
            return Number(bitsConst.Size.ToInt())
                .Result(category, () => CodeBase.BitsConst(bitsConst));
        }

        IFeatureImplementation ISymbolProviderForPointer<DumpPrintToken, IFeatureImplementation>.Feature
            (DumpPrintToken tokenClass)
            => Feature.Extension.Value(DumpPrintTokenResult, this);

        protected override CodeBase DumpPrintCode()
            => Align.ArgCode.DumpPrintNumber();
    }
}