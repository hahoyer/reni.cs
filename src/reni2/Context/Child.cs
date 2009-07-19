using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Feature;
using Reni.Parser.TokenClass;
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

        protected override sealed Sequence<ContextBase> ObtainChildChain()
        {
            return Parent.ChildChain + this;
        }

        internal override Result CreateArgsRefResult(Category category)
        {
            return Parent.CreateArgsRefResult(category);
        }

        internal override SearchResult<IContextFeature> Search(Defineable defineable)
        {
            return Parent.Search(defineable).RecordSubTrial(this);
        }

        internal override IStructContext FindStruct()
        {
            if(this is IStructContext)
                return (IStructContext) this;
            return Parent.FindStruct();
        }
    }
}