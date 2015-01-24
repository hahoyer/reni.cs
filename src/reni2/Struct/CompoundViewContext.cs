using System.Collections.Generic;
using System.Linq;
using System;
using hw.Debug;
using hw.Forms;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.TokenClasses;

namespace Reni.Struct
{
    sealed class CompoundViewContext
        : Child
            , ISymbolProviderForPointer<Definable, IFeatureImplementation>
    {
        internal CompoundViewContext(ContextBase parent, CompoundView view)
            : base(parent) { View = view; }

        [Node]
        CompoundView View { get; }
        [DisableDump]
        protected override string ChildDumpPrintText => View.Compound.Syntax.DumpPrintText;

        IFeatureImplementation ISymbolProviderForPointer<Definable, IFeatureImplementation>.Feature(Definable tokenClass)
            => View.Find(tokenClass);

        internal Result ObjectResult(Category category) => View.StructReferenceViaContextReference(category);

        internal override CompoundView ObtainRecentCompoundView() => View;
    }
}