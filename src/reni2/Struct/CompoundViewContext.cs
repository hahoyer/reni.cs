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
            , ISymbolProviderForPointer<Definable>
    {
        internal CompoundViewContext(ContextBase parent, CompoundView view)
            : base(parent) { View = view; }

        [Node]
        [EnableDump]
        CompoundView View { get; }

        string GetCompoundIdentificationDump() => View.GetCompoundIdentificationDump();

        protected override string GetContextChildIdentificationDump() => GetCompoundIdentificationDump();

        ITypeImplementation ISymbolProviderForPointer<Definable>.Feature(Definable tokenClass)
            => View.Find(tokenClass);

        internal Result ObjectResult(Category category) => View.ObjectPointerViaContext(category);

        internal override CompoundView ObtainRecentCompoundView() => View;
    }
}