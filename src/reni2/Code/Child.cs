using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using JetBrains.Annotations;
using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    /// 
    /// </summary>
    [Obsolete]
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
            StopByObjectId(-6);
        }

        [Node]
        internal CodeBase Parent { get { return _parent; } }

        [Node]
        internal LeafElement LeafElement { get { return _leafElement; } }

        protected override Size GetSize() { return LeafElement.Size; }

        [IsDumpEnabled(false)]
        protected override Size MaxSizeImplementation { get { return Parent.MaxSize.Max(LeafElement.Size); } }

        [IsDumpEnabled(false)]
        internal override Refs RefsImplementation { get { return Parent.RefsImplementation; } }

        internal override CodeBase CreateFiber(FiberItem subsequentElement) { throw new NotImplementedException(); }

        [IsDumpEnabled(false)]
        internal override RefAlignParam RefAlignParam { get { return Parent.RefAlignParam; } }

        internal int AddTo(Container container)
        {
            NotImplementedMethod(container);
            throw new NotImplementedException();
        }

        public override string DumpData()
        {
            var count = 0;
            return "[*] " + InternalDump(ref count);
        }

        private string InternalDump(ref int count)
        {
            var result = "";
            var parent = Parent as Child;
            if(parent != null)
                result += parent.InternalDump(ref count);
            else
                result += Tracer.Dump(Parent);

            result += "\n[" + count + "] ";
            count++;
            result += Tracer.Dump(LeafElement);
            return result;
        }
    }
}