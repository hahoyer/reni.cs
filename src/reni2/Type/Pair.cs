using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Basics;
using Reni.Context;

namespace Reni.Type
{
    sealed class Pair : TypeBase
    {
        [EnableDump]
        readonly TypeBase _first;
        [EnableDump]
        readonly TypeBase _second;

        internal Pair(TypeBase first, TypeBase second)
        {
            Tracer.Assert(first.Root == second.Root);
            _first = first;
            _second = second;
        }

        [DisableDump]
        internal override bool Hllw => _first.Hllw && _second.Hllw;
        [DisableDump]
        internal override Root Root => _first.Root;
        protected override Size GetSize() => _first.Size + _second.Size;

        [DisableDump]
        internal override string DumpPrintText
        {
            get
            {
                var result = "";
                var types = ToList;
                foreach(var t in types)
                {
                    result += "\n";
                    result += t;
                }
                return "(" + result.Indent() + "\n)";
            }
        }

        [DisableDump]
        internal override TypeBase[] ToList
        {
            get
            {
                var result = new List<TypeBase>(_first.ToList) {_second};
                return result.ToArray();
            }
        }

        internal override Result Destructor(Category category)
        {
            var firstHandler = _first.Destructor(category);
            var secondHandler = _second.Destructor(category);
            if(firstHandler.IsEmpty)
                return secondHandler;
            if(secondHandler.IsEmpty)
                return firstHandler;

            NotImplementedMethod(category);
            throw new NotImplementedException();
        }

        protected override string GetNodeDump() => "pair." + ObjectId;
    }
}