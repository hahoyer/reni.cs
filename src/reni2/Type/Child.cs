using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace Reni.Type
{
    internal abstract class Child : TypeBase
    {
        readonly TypeBase _parent;

        protected Child(TypeBase parent)
        {
            _parent = parent;
        }

        protected Child(int objectId, TypeBase parent): base(objectId)
        {
            _parent = parent;
        }

        [Node, DumpData(true)]
        public TypeBase Parent { get { return _parent; } }

        protected internal override int IndexSize
        {
            get { return Parent.IndexSize; }
        }
    }
}