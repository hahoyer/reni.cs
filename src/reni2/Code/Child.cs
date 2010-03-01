using HWClassLibrary.TreeStructure;
using System;
using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class Child : CodeBase
    {
        private readonly LeafElement _leafElement;
        private readonly CodeBase _parent;
        private static int _nextObjectId;

        internal Child(CodeBase parent, LeafElement leafElement)
            : base(_nextObjectId++)
        {
            Tracer.Assert(leafElement != null);
            _parent = parent;
            _leafElement = leafElement;
            StopByObjectId(-3281);
        }

        [Node]
        internal CodeBase Parent { get { return _parent; } }
        [Node]
        internal LeafElement LeafElement { get { return _leafElement; } }

        [DumpData(false)]
        protected override Size SizeImplementation { get { return LeafElement.Size; } }
        [DumpData(false)]
        protected override Size MaxSizeImplementation { get { return Parent.MaxSize.Max(LeafElement.Size); } }
        [DumpData(false)]
        internal override Refs RefsImplementation { get { return _parent.RefsImplementation; } }

        [DumpData(false)]
        internal override RefAlignParam RefAlignParam
        {
            get
            {
                var result = LeafElement.RefAlignParam;
                if(result != null)
                    return result;
                return Parent.RefAlignParam;
            }
        }

        internal override CodeBase CreateChild(LeafElement leafElement)
        {
            var newLeafElement = LeafElement.TryToCombineN(leafElement);
            if(newLeafElement != null)
                return Parent.CreateChildren(newLeafElement);

            return base.CreateChild(leafElement);
        }

        protected override T VisitImplementation<T>(Visitor<T> actual)
        {
            return actual.ChildVisit(this);
        }

        public int AddTo(Container container)
        {
            NotImplementedMethod(container);
            throw new NotImplementedException();
        }

        public CodeBase ReCreate(CodeBase parent)
        {
            NotImplementedMethod(parent);
            throw new NotImplementedException();
        }
    }
}