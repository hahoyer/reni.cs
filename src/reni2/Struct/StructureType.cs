using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.Type;

namespace Reni.Struct
{
    sealed class StructureType
        : TypeBase
            , ISymbolProvider<DumpPrintToken, IFeatureImplementation>
    {
        readonly CompoundView _compoundView;

        internal StructureType(CompoundView compoundView)
        {
            _compoundView = compoundView;
        }

        IFeatureImplementation ISymbolProvider<DumpPrintToken, IFeatureImplementation>.Feature(DumpPrintToken tokenClass)
        {
            return Extension.SimpleFeature(DumpPrintTokenResult);
        }

        [DisableDump]
        internal CompoundView CompoundView { get { return _compoundView; } }

        [DisableDump]
        internal override Root RootContext { get { return _compoundView.RootContext; } }

        [DisableDump]
        internal override CompoundView FindRecentCompoundView { get { return CompoundView; } }

        [DisableDump]
        internal override bool Hllw { get { return CompoundView.Hllw; } }

        internal Result DumpPrintTokenResult(Category category) { return CompoundView.DumpPrintResultViaStructReference(category); }

        [DisableDump]
        internal override bool HasQuickSize { get { return false; } }

        protected override Size GetSize() { return CompoundView.StructSize; }

        protected override string GetNodeDump() { return "type(" + CompoundView.NodeDump + ")"; }
    }
}