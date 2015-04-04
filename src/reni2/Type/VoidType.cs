using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;

namespace Reni.Type
{
    sealed class VoidType : TypeBase, ISymbolProvider<DumpPrintToken, IFeatureImplementation>
    {
        public VoidType(Root rootContext) { RootContext = rootContext; }

        protected override TypeBase ReversePair(TypeBase first) => first;

        [DisableDump]
        internal override Root RootContext { get; }

        [DisableDump]
        internal override bool Hllw => true;

        IFeatureImplementation ISymbolProvider<DumpPrintToken, IFeatureImplementation>.Feature(DumpPrintToken tokenClass)
            => Feature.Extension.SimpleFeature(DumpPrintTokenResult);

        internal override TypeBase Pair(TypeBase second) => second;
        new Result DumpPrintTokenResult(Category category) => Result(category);
        internal override string DumpPrintText => "void";
        protected override string GetNodeDump() => "void";
    }
}