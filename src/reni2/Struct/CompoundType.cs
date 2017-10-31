using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.TokenClasses;
using Reni.Type;
using Reni.Validation;

namespace Reni.Struct
{
    sealed class CompoundType
        : TypeBase,
            ISymbolProviderForPointer<DumpPrintToken>,
            ISymbolProviderForPointer<Definable>,
            ISymbolProvider<DumpPrintToken>,
            ISymbolProvider<Definable>,
            IChild<ContextBase>
    {
        bool IsDumpPrintTextActive;

        internal CompoundType(CompoundView view)
        {
            Tracer.Assert(!view.HasIssues, () => Tracer.Dump(view.Issues));
            View = view;
        }

        ContextBase IChild<ContextBase>.Parent => View.CompoundContext;

        IImplementation ISymbolProvider<Definable>.Feature(Definable tokenClass)
            => Hllw ? View.Find(tokenClass, true) : null;

        IImplementation ISymbolProvider<DumpPrintToken>.Feature(DumpPrintToken tokenClass)
            => Hllw ? Feature.Extension.Value(DumpPrintTokenResult) : null;

        IImplementation ISymbolProviderForPointer<Definable>.Feature(Definable tokenClass)
            => View.Find(tokenClass, true);

        IImplementation ISymbolProviderForPointer<DumpPrintToken>.Feature
            (DumpPrintToken tokenClass)
            => Feature.Extension.Value(DumpPrintTokenResult);

        [Node]
        [DisableDump]
        internal CompoundView View {get;}

        [DisableDump]
        internal override Root Root => View.Root;

        [DisableDump]
        internal override CompoundView FindRecentCompoundView => View;

        [DisableDump]
        internal override bool Hllw => View.Hllw;

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
            }
        }

        [DisableDump]
        internal override bool HasQuickSize => false;

        [DisableDump]
        internal override IEnumerable<IConversion> StripConversionsFromPointer
            => View.ConverterFeatures.Union(View.MixinConversions);

        [DisableDump]
        internal override IEnumerable<string> DeclarationOptions
            => base.DeclarationOptions.Concat(InternalDeclarationOptions);

        [DisableDump]
        IEnumerable<string> InternalDeclarationOptions => View.DeclarationOptions;

        [DisableDump]
        protected override IEnumerable<IConversion> StripConversions
        {
            get
            {
                if(Hllw)
                    yield return Feature.Extension.Conversion(VoidConversion);
            }
        }

        [DisableDump]
        internal override ContextBase ToContext => View.Context;

        internal override Issue[] Issues => View.Issues;

        Result VoidConversion(Category category) => Mutation(Root.VoidType) & category;

        protected override Size GetSize() => View.CompoundViewSize;

        protected override string GetNodeDump()
            => base.GetNodeDump() + "(" + View.GetCompoundIdentificationDump() + ")";

        new Result DumpPrintTokenResult(Category category)
            => View.DumpPrintResultViaObject(category);

        internal override Result Cleanup(Category category) => View.Compound.Cleanup(category);
    }
}