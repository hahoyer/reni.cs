using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Feature;

namespace Reni.Context
{
    [Serializable]
    internal abstract class Child : ReniObject, IContextItem
    {
        RefAlignParam IContextItem.RefAlignParam { get { return null; } }
        Result IContextItem.CreateArgsReferenceResult(ContextBase contextBase, Category category) { return CreateArgsReferenceResult(contextBase, category); }
        void IContextItem.Search(SearchVisitor<IContextFeature> searchVisitor) {  }
        string IDumpShortProvider.DumpShort() { return DumpShort(); }

        protected virtual Result CreateArgsReferenceResult(ContextBase contextBase, Category category)
        {
            NotImplementedMethod(contextBase, category);
            return null;
        }

        protected new virtual string DumpShort() { return base.DumpData(); }

    }
}