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
        internal readonly ContainerSyntax Container;

        internal Context(ContextBase parent, ContainerSyntax container, int position)
            : base(parent)
        {
            Position = position;
            Container = container;
        }

        [DisableDump]
        internal ContainerView ContainerView { get { return Parent.UniqueStructure(Container, Position); } }

        IFeatureImplementation ISymbolProvider<Definable, IFeatureImplementation>.Feature(Definable tokenClass)
        {
            var structurePosition = Container.Find(tokenClass.Name);
            return structurePosition == null ? null : structurePosition.Convert(ContainerView);
        }

        internal Result ObjectResult(Category category) { return ContainerView.StructReferenceViaContextReference(category); }

        protected override string ChildDumpPrintText { get { return Container.DumpPrintText; } }
        internal override ContainerView ObtainRecentStructure() { return ContainerView; }
    }
}