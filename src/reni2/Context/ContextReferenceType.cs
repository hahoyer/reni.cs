using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Basics;
using Reni.Code;
using Reni.Feature;
using Reni.Struct;
using Reni.Type;

namespace Reni.Context
{
    sealed class ContextReferenceType
        : TypeBase
            , ISymbolProviderForPointer<DumpPrintToken>
    {
        readonly int Order;
        readonly Compound Parent;

        public ContextReferenceType(Compound parent)
        {
            Parent = parent;
            Order = CodeArgs.NextOrder++;
        }

        [DisableDump]
        internal override Root RootContext => Parent.RootContext;
        [DisableDump]
        internal override CompoundView FindRecentCompoundView => Parent.CompoundView;

        [DisableDump]
        internal override bool Hllw => false;
        [DisableDump]
        internal override bool IsPointerPossible => true;

        protected override Size GetSize() => Root.DefaultRefAlignParam.RefSize;

        IImplementation ISymbolProviderForPointer<DumpPrintToken>.Feature(DumpPrintToken tokenClass)
            => Feature.Extension.Value(DumpPrintTokenResult, this);

        protected override CodeBase DumpPrintCode()
            => CodeBase.DumpPrintText(ContextOperator.TokenId);

        new Result DumpPrintTokenResult(Category category)
            => VoidType
                .Result(category, DumpPrintCode);

        protected override IEnumerable<IConversion> StripConversions
            => base.StripConversions
                .Concat(new[] {Feature.Extension.Conversion(PointerConversion)});

        Result PointerConversion(Category category) 
            => Parent
            .CompoundView
            .Type
            .Pointer
            .Result(category, c=>ArgResult(c).AddToReference(()=>Parent.Size()*-1));
    }
}