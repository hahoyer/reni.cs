using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;

namespace Reni.Code
{
    [Serializable]
    internal sealed class RecursiveCall : FiberHead
    {
        protected override Size GetSize() { return Size.Zero; }
        protected override string CSharpString() { return CSharpGenerator.CreateRecursiveCall(); }
        internal override void Visit(IVisitor visitor) { visitor.RecursiveCall(); }
    }
}