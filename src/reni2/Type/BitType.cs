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
        readonly Root _rootContext;

        internal BitType(Root rootContext) { _rootContext = rootContext; }

        [DisableDump]
        internal override bool Hllw { get { return false; } }

        [DisableDump]
        internal override string DumpPrintText { get { return "bit"; } }

        [DisableDump]
        internal override Root RootContext { get { return _rootContext; } }

        protected override Size GetSize() { return Size.Create(1); }

        protected override string Dump(bool isRecursion) { return GetType().PrettyName(); }

        protected override string GetNodeDump() { return "bit"; }
        internal NumberType UniqueNumber(int bitCount) { return UniqueArray(bitCount).UniqueNumber; }
        internal Result Result(Category category, BitsConst bitsConst)
        {
            return UniqueNumber(bitsConst.Size.ToInt())
                .Result(category, () => CodeBase.BitsConst(bitsConst));
        }

        internal interface IPrefix
        {
            [DisableDump]
            string Name { get; }
        }

        IFeatureImplementation ISymbolProvider<DumpPrintToken, IFeatureImplementation>.Feature(DumpPrintToken tokenClass)
            => Extension.SimpleFeature(DumpPrintTokenResult);

        Result DumpPrintTokenResult(Category category) => VoidType.Result(category, () => ArgCode.Align().DumpPrintNumber(), CodeArgs.Arg);
    }
}