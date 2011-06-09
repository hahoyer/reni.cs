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

        void IContextItem.Search(SearchVisitor<IContextFeature> searchVisitor, ContextBase parent) { Search(searchVisitor, parent); }
        string IDumpShortProvider.DumpShort() { return DumpShort(); }

        protected new virtual string DumpShort() { return base.DumpData(); }
        protected virtual void Search(SearchVisitor<IContextFeature> searchVisitor, ContextBase parent) { }
    }
}