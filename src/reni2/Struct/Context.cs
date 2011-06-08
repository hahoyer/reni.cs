using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class Context : Reni.Context.Child
    {
        private readonly int _position;
        [Node]
        internal readonly Container Container;

        internal Context(Container container, int position)
        {
            _position = position;
            Container = container;
        }

        internal int Position { get { return _position; } }

        [IsDumpEnabled(false)]
        internal int IndexSize { get { return Container.IndexSize; } }

        [IsDumpEnabled(false)]
        internal IReferenceInCode ForCode { get { throw new NotImplementedException(); } }

        internal TypeBase ContextReferenceType(ContextBase context) { throw new NotImplementedException(); }

        protected override void Search(SearchVisitor<IContextFeature> searchVisitor, ContextBase parent)
        {
            if (!searchVisitor.IsSuccessFull)
            {
                searchVisitor.InternalResult =
                    Container
                        .SearchFromStructContext(searchVisitor.Defineable)
                        .CheckedConvert(Container.SpawnContainerContext(parent));
            }
            base.Search(searchVisitor, parent);
        }

    }
}