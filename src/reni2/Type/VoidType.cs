using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Context;

namespace Reni.Type
{
    sealed class VoidType : TypeBase
    {
        readonly Root _rootContext;
        public VoidType(Root rootContext) { _rootContext = rootContext; }

        protected override TypeBase ReversePair(TypeBase first) { return first; }

        [DisableDump]
        internal override Root RootContext { get { return _rootContext; } }

        [DisableDump]
        internal override bool Hllw { get { return true; } }

        internal override TypeBase Pair(TypeBase second) { return second; }
        internal override string DumpPrintText { get { return "void"; } }
        protected override string GetNodeDump() { return "void"; }
        Result DumpPrintTokenResult(Category category) { return Result(category); }
    }
}