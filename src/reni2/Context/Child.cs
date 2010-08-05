using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Feature;
using Reni.Struct;

namespace Reni.Context
{
    [Serializable]
    internal abstract class Child : ContextBase
    {
        private readonly ContextBase _parent;

        protected Child(ContextBase parent)
        {
            _parent = parent;
        }

        [Node]
        internal ContextBase Parent { get { return _parent; } }

        internal override sealed RefAlignParam RefAlignParam { get { return Parent.RefAlignParam; } }

        [DumpData(false)]
        internal override sealed Root RootContext { get { return Parent.RootContext; } }

        protected override sealed ContextBase[] ObtainChildChain()
        {
            return Parent.ChildChain.Concat(new []{this}).ToArray();
        }

        internal override Result CreateArgsReferenceResult(Category category)
        {
            return Parent.CreateArgsReferenceResult(category);
        }

        internal override void Search(SearchVisitor<IContextFeature> searchVisitor)
        {
            Parent.Search(searchVisitor);
        }

        internal override Struct.Context FindStruct()
        {
            return Parent.FindStruct();
        }
    }
}