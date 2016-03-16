using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.DebugFormatter;

using hw.Scanner;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.TokenClasses;

namespace Reni.Struct
{
    sealed class CompoundContext
        : Child
            , ISymbolProviderForPointer<Definable>
            , ISourceProvider
            , IContextReference
    {
        readonly int _order;

        internal CompoundContext(CompoundView view)
            : base(view.Compound.Parent)
        {
            _order = CodeArgs.NextOrder++;
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

        int IContextReference.Order => _order;

        SourcePart ISourceProvider.Value
            => View
                .Compound
                .Syntax
                .SourceParts
                .Take(View.ViewPosition)
                .Aggregate();
    }
}