﻿using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Struct
{
    sealed class CompoundType
        : TypeBase
            , ISymbolProvider<DumpPrintToken, IFeatureImplementation>
            , ISymbolProvider<Definable, IFeatureImplementation>
    {
        internal CompoundType(CompoundView view) { View = view; }

        [DisableDump]
        internal CompoundView View { get; }
        [DisableDump]
        internal override Root RootContext => View.RootContext;
        [DisableDump]
        internal override CompoundView FindRecentCompoundView => View;
        [DisableDump]
        internal override bool Hllw => View.Hllw;
        [DisableDump]
        internal override bool HasQuickSize => false;

        [DisableDump]
        internal override IEnumerable<ISimpleFeature> StripConversions
        {
            get
            {
                Tracer.Assert(!View.Compound.Syntax.Converters.Any());

                if(Hllw)
                    yield return Extension.SimpleFeature(VoidConversion);
            }
        }

        Result VoidConversion(Category category) => RootContext.VoidType.Result(category, ArgResult);

        IFeatureImplementation ISymbolProvider<DumpPrintToken, IFeatureImplementation>.Feature(DumpPrintToken tokenClass)
            => Extension.SimpleFeature(DumpPrintTokenResult);

        IFeatureImplementation ISymbolProvider<Definable, IFeatureImplementation>.Feature(Definable tokenClass)
            => View.Find(tokenClass);

        protected override Size GetSize() => View.CompoundSize;

        protected override string GetNodeDump() => "type(" + View.NodeDump + ")";

        new Result DumpPrintTokenResult(Category category) => View.DumpPrintResultViaStructReference(category);
    }
}