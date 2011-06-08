using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Feature;

namespace Reni.Context
{
    [Serializable]
    internal abstract class Child : ReniObject, IContextItem
    {
        RefAlignParam IContextItem.RefAlignParam { get { return null; } }

        Result IContextItem.CreateArgsReferenceResult
            (ContextBase contextBase, Category category) { return CreateArgsReferenceResult(contextBase, category); }
        void IContextItem.Search(SearchVisitor<IContextFeature> searchVisitor, ContextBase parent) { Search(searchVisitor, parent); }
        string IDumpShortProvider.DumpShort() { return DumpShort(); }

        protected virtual Result CreateArgsReferenceResult(ContextBase contextBase, Category category)
        {
            NotImplementedMethod(contextBase, category);
            return null;
        }

        protected new virtual string DumpShort() { return base.DumpData(); }
        protected virtual void Search(SearchVisitor<IContextFeature> searchVisitor, ContextBase parent) { }
    }
}