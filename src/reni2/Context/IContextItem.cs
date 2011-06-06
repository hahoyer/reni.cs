using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Feature;

namespace Reni.Context
{
    internal interface IContextItem  : IDumpShortProvider
    {
        RefAlignParam RefAlignParam { get; }
        Result CreateArgsReferenceResult(ContextBase contextBase, Category category);
        void Search(SearchVisitor<IContextFeature> searchVisitor);
    }
}