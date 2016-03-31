using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.DebugFormatter;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;

namespace Reni.Type
{
    sealed class BitType : TypeBase, ISymbolProviderForPointer<DumpPrintToken>
    {
        internal BitType(Root root) { Root = root; }

        [DisableDump]
        internal override Root Root { get; }

        [DisableDump]
        internal override string DumpPrintText => "bit";
        protected override string GetNodeDump() => "bit";

        [DisableDump]
        internal override bool Hllw => false;
        protected override Size GetSize() => Size.Create(1);

        IImplementation ISymbolProviderForPointer<DumpPrintToken>.Feature
            (DumpPrintToken tokenClass)
            => Feature.Extension.Value(DumpPrintTokenResult, this);

        protected override string Dump(bool isRecursion) => GetType().PrettyName();

        internal NumberType Number(int bitCount) => Array(bitCount).Number;

        internal Result Result(Category category, BitsConst bitsConst)
        {
            return Number(bitsConst.Size.ToInt())
                .Result(category, () => CodeBase.BitsConst(bitsConst));
        }

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


        protected override CodeBase DumpPrintCode() => Align.ArgCode.DumpPrintNumber();
    }
}