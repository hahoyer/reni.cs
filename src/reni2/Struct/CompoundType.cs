using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

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
            , IChild<ContextBase>
    {
        internal CompoundType(CompoundView view) { View = view; }

        [Node]
        [DisableDump]
        internal CompoundView View { get; }
        [DisableDump]
        internal override Root Root => View.Root;
        [DisableDump]
        internal override CompoundView FindRecentCompoundView => View;
        [DisableDump]
        internal override bool Hllw => View.Hllw;

        bool IsDumpPrintTextActive; 
        internal override string DumpPrintText
        {
            get
            {
                if(IsDumpPrintTextActive)
                    return "?";
                IsDumpPrintTextActive = true;
                var result = View.DumpPrintTextOfType;
                IsDumpPrintTextActive = false;
                return result;
            } }

        [DisableDump]
        internal override bool HasQuickSize => false;

        [DisableDump]
        internal override IEnumerable<IConversion> StripConversionsFromPointer
            => View.ConverterFeatures.Union(View.MixinConversions);

        internal override IEnumerable<string> DeclarationOptions
            => base.DeclarationOptions.Concat(InternalDeclarationOptions);

        IEnumerable<string> InternalDeclarationOptions
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }


        [DisableDump]
        protected override IEnumerable<IConversion> StripConversions
        {
            get
            {
                if(Hllw)
                    yield return Feature.Extension.Conversion(VoidConversion);
            }
        }

        Result VoidConversion(Category category) => Mutation(Root.VoidType) & category;

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

        ContextBase IChild<ContextBase>.Parent => View.CompoundContext;

        internal override Result Cleanup(Category category) => View.Compound.Cleanup(category);
    }
}