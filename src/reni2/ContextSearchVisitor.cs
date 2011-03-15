using System;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Feature;
using Reni.TokenClasses;

namespace Reni
{
    internal class ContextSearchVisitor : RootSearchVisitor<IContextFeature>
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