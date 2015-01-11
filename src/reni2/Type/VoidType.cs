using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Context;
using Reni.Feature;
using Reni.Feature.DumpPrint;

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
            => Extension.SimpleFeature(DumpPrintTokenResult);

        internal override TypeBase Pair(TypeBase second) => second;
        internal override string DumpPrintText => "void";
        protected override string GetNodeDump() => "void";
    }
}