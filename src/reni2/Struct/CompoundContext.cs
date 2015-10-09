using System.Collections.Generic;
using System.Linq;
using System;
using hw.Debug;
using hw.Forms;
using Reni.Context;
using Reni.Feature;
using Reni.TokenClasses;

namespace Reni.Struct
{
    sealed class CompoundContext
        : Child
            , ISymbolProviderForPointer<Definable>
    {
        internal CompoundContext(CompoundView view)
            : base(view.Compound.Parent) { View = view; }

        [Node]
        [EnableDump]
        CompoundView View { get; }

        string GetCompoundIdentificationDump() => View.GetCompoundIdentificationDump();

        protected override string GetContextChildIdentificationDump() => GetCompoundIdentificationDump();

        IImplementation ISymbolProviderForPointer<Definable>.Feature(Definable tokenClass)
            => View.Find(tokenClass);

        internal override CompoundView ObtainRecentCompoundView() => View;
    }
}