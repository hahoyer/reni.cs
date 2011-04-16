using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Reni.Code
{
    [Serializable]
    internal sealed class RecursiveCall : FiberHead
    {
        protected override Size GetSize() { return Size.Zero; }
        protected override string CSharpString() { return CSharpGenerator.CreateRecursiveCall(); }
        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.RecursiveCall(); }
    }
}