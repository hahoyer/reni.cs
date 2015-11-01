using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
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

        [Node]
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
        internal override IEnumerable<IConversion> StripConversionsFromPointer
            => View.ConverterFeatures.Union(View.MixinConversions);

        [DisableDump]
        protected override IEnumerable<IConversion> StripConversions
        {
            get
            {
                if(Hllw)
                    yield return Feature.Extension.Conversion(VoidConversion);
            }
        }

        Result VoidConversion(Category category) => Mutation(RootContext.VoidType) & category;

        IImplementation ISymbolProviderForPointer<DumpPrintToken>.Feature
            (DumpPrintToken tokenClass)
            => Feature.Extension.Value(DumpPrintTokenResult);

        IImplementation ISymbolProviderForPointer<Definable>.Feature(Definable tokenClass)
            => View.Find(tokenClass);

        IImplementation ISymbolProvider<DumpPrintToken>.Feature(DumpPrintToken tokenClass)
            => Hllw ? Feature.Extension.Value(DumpPrintTokenResult) : null;

        IImplementation ISymbolProvider<Definable>.Feature(Definable tokenClass)
            => Hllw ? View.Find(tokenClass) : null;

        protected override Size GetSize() => View.CompoundViewSize;

        protected override string GetNodeDump()
            => base.GetNodeDump() + "(" + View.GetCompoundIdentificationDump() + ")";

        new Result DumpPrintTokenResult(Category category)
            => View.DumpPrintResultViaObject(category);

        [DisableDump]
        internal override ContextBase ToContext => View.Context;
    }
}