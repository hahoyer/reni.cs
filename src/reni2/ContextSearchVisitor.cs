using System;
using System.Linq;
using System.Collections.Generic;
using Reni.Context;
using Reni.Feature;
using Reni.Parser.TokenClass;

namespace Reni
{
    internal class ContextSearchVisitor<TFeature> : RootSearchVisitor<IContextFeature<TFeature>>
    {
        internal ContextSearchVisitor(Defineable defineable)
            : base(defineable) { }

        internal void Search(ContextBase contextBase)
        {
            if(IsSuccessFull)
                return;
            contextBase.Search(this);
        }
    }
}