using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Helper;
using hw.Scanner;
using Reni.Context;
using Reni.Feature;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Struct
{
    sealed class CompoundContext
        : Child
            , ISymbolProviderForPointer<Definable>
            , ISourceProvider
    {
        internal CompoundContext(CompoundView view)
            : base(view.Compound.Parent)
        {
            View = view;
        }

        [Node]
        [DisableDump]
        internal CompoundView View { get; }

        string GetCompoundIdentificationDump() => View.GetCompoundChildDump();

        protected override string GetContextChildIdentificationDump()
            => GetCompoundIdentificationDump();

        IImplementation ISymbolProviderForPointer<Definable>.Feature(Definable tokenClass)
            => View.Find(tokenClass);

        internal override CompoundView ObtainRecentCompoundView() => View;

        SourcePart ISourceProvider.Value
            => View
                .Compound
                .Syntax
                .SourceParts
                .Take(View.ViewPosition)
                .Aggregate();
    }
}