using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;

namespace Reni.Type
{
    sealed class VoidType : TypeBase, ISymbolProvider<DumpPrintToken>
    {
        internal VoidType(Root rootContext) { RootContext = rootContext; }

        [DisableDump]
        internal override Root RootContext { get; }

        [DisableDump]
        internal override string DumpPrintText => "void";
        protected override string GetNodeDump() => "void";

        [DisableDump]
        internal override bool Hllw => true;

        IImplementation ISymbolProvider<DumpPrintToken>.Feature(DumpPrintToken tokenClass)
            => Feature.Extension.Value(DumpPrintTokenResult);

        protected override TypeBase ReversePair(TypeBase first) => first;
        internal override TypeBase Pair(TypeBase second) => second;
        new Result DumpPrintTokenResult(Category category) => Result(category);
    }
}