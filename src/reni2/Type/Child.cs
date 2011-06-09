using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using System;
using HWClassLibrary.TreeStructure;

namespace Reni.Type
{
    [Serializable]
    internal abstract class Child : TypeBase
    {
        private readonly TypeBase _parent;

        protected Child(TypeBase parent) { _parent = parent; }

        protected Child(int objectId, TypeBase parent)
            : base(objectId) { _parent = parent; }

        [Node, EnableDump]
        public TypeBase Parent { get { return _parent; } }

        [DisableDump]
        protected internal override int IndexSize { get { return Parent.IndexSize; } }

        protected abstract bool IsInheritor { get; }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            if(IsInheritor)
                Parent.Search(searchVisitor);
            base.Search(searchVisitor);
        }
    }
}