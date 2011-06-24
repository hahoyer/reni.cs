using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.TreeStructure;
using Reni.Context;
using Reni.Feature;

namespace Reni.Struct
{
    internal sealed class Context : Child
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

        protected override void Search(SearchVisitor<IContextFeature> searchVisitor, ContextBase parent)
        {
            if(!searchVisitor.IsSuccessFull)
            {
                searchVisitor.InternalResult =
                    Container
                        .SearchFromStructContext(searchVisitor.Defineable)
                        .CheckedConvert(parent.SpawnContainerContext(Container));
            }
            base.Search(searchVisitor, parent);
        }
    }
}