using System;
using HWClassLibrary.Helper.TreeViewSupport;

namespace Reni.Type
{
    public abstract class Child : Base
    {
        readonly Base _parent;

        protected Child(Base parent)
        {
            _parent = parent;
        }

        public Child(int objectId, Base parent): base(objectId)
        {
            _parent = parent;
        }

        [Node]
        public Base Parent { get { return _parent; } }
    }
}