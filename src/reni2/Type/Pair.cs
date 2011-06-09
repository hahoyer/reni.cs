using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace Reni.Type
{
    [Serializable]
    internal class Pair : Child
    {
        private readonly TypeBase _second;

        internal Pair(TypeBase first, TypeBase second)
            : base(first) { _second = second; }

        internal TypeBase First { get { return Parent; } }
        internal TypeBase Second { get { return _second; } }

        protected override Size GetSize() { return First.Size + Second.Size; }

        [DisableDump]
        internal override string DumpPrintText
        {
            get
            {
                var result = "";
                var types = ToList;
                for(var i = 0; i < types.Length; i++)
                {
                    result += "\n";
                    result += types[i];
                }
                return "(" + result.Indent() + "\n)";
            }
        }

        [DisableDump]
        protected internal override TypeBase[] ToList
        {
            get
            {
                var result = new List<TypeBase>(Parent.ToList) {Second};
                return result.ToArray();
            }
        }

        internal override Result Destructor(Category category)
        {
            var firstHandler = First.Destructor(category);
            var secondHandler = Second.Destructor(category);
            if(firstHandler.IsEmpty)
                return secondHandler;
            if(secondHandler.IsEmpty)
                return firstHandler;

            NotImplementedMethod(category);
            throw new NotImplementedException();
        }

        internal override string DumpShort() { return "pair." + ObjectId; }

        protected override bool IsInheritor { get { return false; } }
    }
}