using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.TokenClasses;

namespace Reni.Struct
{
    sealed class CompoundContext :
        Child,
        ISymbolProviderForPointer<Definable>,
        IContextReference
    {
        readonly int _order;

        internal CompoundContext(CompoundView view)
            : base(view.Compound.Parent)
        {
            _order = CodeArgs.NextOrder++;
            View = view;
            StopByObjectIds();
        }

        [DisableDump]
        internal CompoundView View { get; }

        string GetCompoundIdentificationDump() => View.GetCompoundChildDump();

        protected override string GetContextChildIdentificationDump()
            => GetCompoundIdentificationDump();

        IImplementation ISymbolProviderForPointer<Definable>.Feature(Definable tokenClass)
            => View.Find(tokenClass, true);

        internal override CompoundView ObtainRecentCompoundView() => View;

        [DisableDump]
        protected override string LevelFormat => "compount";

        int IContextReference.Order => _order;

        [DisableDump]
        internal override IEnumerable<string> DeclarationOptions => View.DeclarationOptions;

        [EnableDump]
        Parser.Value[] Syntax => View.Compound.Syntax.Statements;
    }
}