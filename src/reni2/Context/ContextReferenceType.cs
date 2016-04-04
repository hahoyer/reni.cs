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
        readonly CompoundView Parent;

        internal ContextReferenceType(CompoundView parent) { Parent = parent; }

        [DisableDump]
        internal override Root Root => Parent.Root;
        [DisableDump]
        internal override CompoundView FindRecentCompoundView => Parent;
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


        protected override IEnumerable<IConversion> StripConversions
            => base.StripConversions
                .Concat(new[] {Feature.Extension.Conversion(PointerConversion)});

        Result PointerConversion(Category category)
            => Parent
                .Type
                .Pointer
                .Result
                (category, c => ArgResult(c).AddToReference(() => Parent.CompoundViewSize * -1));
    }
}