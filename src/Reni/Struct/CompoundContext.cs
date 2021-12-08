using System.Collections.Generic;
using hw.DebugFormatter;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.Struct
{
    sealed class CompoundContext :
        Child,
        ISymbolProviderForPointer<Definable>,
        IContextReference
    {
        readonly int Order;

        internal CompoundContext(CompoundView view)
            : base(view.Compound.Parent)
        {
            Order = Closures.NextOrder++;
            View = view;
            StopByObjectIds();
        }

        [DisableDump]
        internal CompoundView View { get; }

        string GetCompoundIdentificationDump() => View.GetCompoundChildDump();

        protected override string GetContextChildIdentificationDump()
            => GetCompoundIdentificationDump();

        IImplementation ISymbolProviderForPointer<Definable>.Feature(Definable tokenClass)
            => View.Find(tokenClass, false);

        internal override CompoundView ObtainRecentCompoundView() => View;

        [DisableDump]
        protected override string LevelFormat => "compount";

        int IContextReference.Order => Order;

        [DisableDump]
        internal override IEnumerable<string> DeclarationOptions => View.DeclarationOptions;

        [EnableDump]
        ValueSyntax[] Syntax => View.Compound.Syntax.PureStatements;
    }
}