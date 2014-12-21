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
        readonly ContainerView _containerView;

        internal StructureType(ContainerView containerView)
        {
            _containerView = containerView;
        }

        IFeatureImplementation ISymbolProvider<DumpPrintToken, IFeatureImplementation>.Feature(DumpPrintToken tokenClass)
        {
            return Extension.SimpleFeature(DumpPrintTokenResult);
        }

        [DisableDump]
        internal ContainerView ContainerView { get { return _containerView; } }

        [DisableDump]
        internal override Root RootContext { get { return _containerView.RootContext; } }

        [DisableDump]
        internal override ContainerView FindRecentContainerView { get { return ContainerView; } }

        [DisableDump]
        internal override bool Hllw { get { return ContainerView.Hllw; } }

        internal Result DumpPrintTokenResult(Category category) { return ContainerView.DumpPrintResultViaStructReference(category); }

        [DisableDump]
        internal override bool HasQuickSize { get { return false; } }

        protected override Size GetSize() { return ContainerView.StructSize; }

        protected override string GetNodeDump() { return "type(" + ContainerView.NodeDump + ")"; }
    }
}