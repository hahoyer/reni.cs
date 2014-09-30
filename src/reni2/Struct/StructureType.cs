﻿using System;
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
        readonly Structure _structure;

        internal StructureType(Structure structure) { _structure = structure; }

        IFeatureImplementation ISymbolProvider<DumpPrintToken, IFeatureImplementation>.Feature(DumpPrintToken token)
        {
            return Extension.SimpleFeature(DumpPrintTokenResult);
        }

        [DisableDump]
        internal RefAlignParam RefAlignParam { get { return Structure.RefAlignParam; } }

        [DisableDump]
        internal Structure Structure { get { return _structure; } }

        internal override Root RootContext { get { return _structure.RootContext; } }
        protected override Size GetSize() { return Structure.StructSize; }

        protected override string GetNodeDump() { return "type(" + Structure.NodeDump + ")"; }

        [DisableDump]
        internal override Structure FindRecentStructure { get { return Structure; } }

        [DisableDump]
        internal override bool Hllw { get { return Structure.Hllw; } }

        internal Result DumpPrintTokenResult(Category category) { return Structure.DumpPrintResultViaStructReference(category); }

        [DisableDump]
        internal override bool HasQuickSize { get { return false; } }
    }
}