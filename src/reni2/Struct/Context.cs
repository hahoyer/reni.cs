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
    sealed class Context 
        : Child
        , ISymbolProvider<Definable, IFeatureImplementation>
    {
        [Node]
        internal readonly int Position;
        [Node]
        internal readonly CompoundSyntax Syntax;

        internal Context(ContextBase parent, CompoundSyntax syntax, int position)
            : base(parent)
        {
            Position = position;
            Syntax = syntax;
        }

        [DisableDump]
        internal CompoundView CompoundView { get { return Parent.UniqueCompoundView(Syntax, Position); } }

        IFeatureImplementation ISymbolProvider<Definable, IFeatureImplementation>.Feature(Definable tokenClass)
        {
            var structurePosition = Syntax.Find(tokenClass.Name);
            return structurePosition == null ? null : structurePosition.Convert(CompoundView);
        }

        internal Result ObjectResult(Category category) { return CompoundView.StructReferenceViaContextReference(category); }

        protected override string ChildDumpPrintText { get { return Syntax.DumpPrintText; } }
        internal override CompoundView ObtainRecentCompoundView() { return CompoundView; }
    }
}