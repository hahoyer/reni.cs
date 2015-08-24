using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Parser;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Struct
{
    sealed class CompoundType
        : TypeBase
            , ISymbolProviderForPointer<DumpPrintToken>
            , ISymbolProviderForPointer<Definable>
            , ISymbolProvider<DumpPrintToken>
            , ISymbolProvider<Definable>
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
        internal override string DumpPrintText => View.DumpPrintTextOfType;
        [DisableDump]
        internal override bool HasQuickSize => false;

        [DisableDump]
        internal override IEnumerable<IValueFeature> StripConversions
        {
            get
            {
                foreach(var converter in View.ConverterFeatures)
                    yield return converter;

                if(Hllw)
                    yield return Feature.Extension.Value(VoidConversion);
            }
        }

        Result VoidConversion(Category category) => RootContext.VoidType.Result(category, ArgResult);

        IFeatureImplementation ISymbolProviderForPointer<DumpPrintToken>.Feature
            (DumpPrintToken tokenClass)
            => Feature.Extension.Value(DumpPrintTokenResult);

        IFeatureImplementation ISymbolProviderForPointer<Definable>.Feature(Definable tokenClass)
            => View.Find(tokenClass);

        IFeatureImplementation ISymbolProvider<DumpPrintToken>.Feature(DumpPrintToken tokenClass)
            => Feature.Extension.Value(DumpPrintTokenResult);

        IFeatureImplementation ISymbolProvider<Definable>.Feature(Definable tokenClass)
            => View.Find(tokenClass);

        protected override Size GetSize() => View.CompoundViewSize;

        protected override string GetNodeDump() => base.GetNodeDump() + "(" + View.GetCompoundIdentificationDump() + ")";

        new Result DumpPrintTokenResult(Category category) => View.DumpPrintResultViaObject(category);

        internal override IEnumerable<Syntax> GetMixins()
        {
            return View.GetMixins();
        }
    }
}