using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Basics;
using Reni.Code;
using Reni.Feature;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Context
{
    sealed class ContextReferenceType
        : TypeBase
           , IReference
            , ISymbolProvider<DumpPrintToken>
            , ISymbolProvider<Definable>
    {
        readonly int Order;
        readonly CompoundView Parent;

        public ContextReferenceType(CompoundView parent)
        {
            Parent = parent;
            Order = CodeArgs.NextOrder++;
        }

        [DisableDump]
        internal override Root RootContext => Parent.RootContext;
        [DisableDump]
        internal override CompoundView FindRecentCompoundView => Parent;

        [DisableDump]
        internal override bool Hllw => false;
        [DisableDump]
        internal override bool IsPointerPossible => false;

        protected override Size GetSize() => Root.DefaultRefAlignParam.RefSize;

        int IContextReference.Order => Order;

        IConversion IReference.Converter
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        bool IReference.IsWeak => false;

        IImplementation ISymbolProvider<DumpPrintToken>.Feature(DumpPrintToken tokenClass)
            => Feature.Extension.Value(DumpPrintTokenResult, this);

        protected override CodeBase DumpPrintCode()
            => CodeBase.DumpPrintText(ContextOperator.TokenId);

        new Result DumpPrintTokenResult(Category category)
            => VoidType
                .Result(category, DumpPrintCode);

        IImplementation ISymbolProvider<Definable>.Feature(Definable tokenClass)
            => Parent.Find(tokenClass);

    }
}