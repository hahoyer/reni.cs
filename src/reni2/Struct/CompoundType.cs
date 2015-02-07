using System;
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
            , ISymbolProviderForPointer<DumpPrintToken, IFeatureImplementation>
            , ISymbolProviderForPointer<Definable, IFeatureImplementation>
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
                foreach(var converter in View.ConverterFeatures)
                    yield return converter;

                if(Hllw)
                    yield return Extension.SimpleFeature(VoidConversion);
            }
        }

        Result VoidConversion(Category category) => RootContext.VoidType.Result(category, ArgResult);

        IFeatureImplementation ISymbolProviderForPointer<DumpPrintToken, IFeatureImplementation>.Feature
            (DumpPrintToken tokenClass)
            => Extension.SimpleFeature(DumpPrintTokenResult);

        IFeatureImplementation ISymbolProviderForPointer<Definable, IFeatureImplementation>.Feature(Definable tokenClass)
            => View.Find(tokenClass);

        IFeatureImplementation ISymbolProvider<DumpPrintToken, IFeatureImplementation>.Feature(DumpPrintToken tokenClass)
            => Extension.SimpleFeature(DumpPrintTokenResult);

        IFeatureImplementation ISymbolProvider<Definable, IFeatureImplementation>.Feature(Definable tokenClass)
            => View.Find(tokenClass);

        protected override Size GetSize() => View.CompoundViewSize;

        protected override string GetNodeDump() => base.GetNodeDump() + "(" + View.GetCompoundIdentificationDump() + ")";

        new Result DumpPrintTokenResult(Category category) => View.DumpPrintResultViaObject(category);
    }
}