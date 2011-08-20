using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;

namespace Reni.Struct
{
    internal sealed class Context : ReniObject, IContextItem
    {
        [Node]
        internal readonly int Position;
        [Node]
        internal readonly Container Container;

        internal Context(Container container, int position)
        {
            Position = position;
            Container = container;
        }

        internal void Search(SearchVisitor<IContextFeature> searchVisitor, ContainerContextObject context)
        {
            if(!searchVisitor.IsSuccessFull)
            {
                searchVisitor.InternalResult =
                    Container
                        .SearchFromStructContext(searchVisitor.Defineable)
                        .CheckedConvert(context);
            }
        }
    }
}