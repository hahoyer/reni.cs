using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;
using Reni.Feature;

namespace Reni.Context
{
    internal interface IContextItem  : IDumpShortProvider
    {
        RefAlignParam RefAlignParam { get; }
        void Search(SearchVisitor<IContextFeature> searchVisitor, ContextBase parent);
    }
}