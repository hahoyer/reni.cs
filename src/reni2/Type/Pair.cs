using System;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;

namespace Reni.Type
{
    internal class Pair : Child
    {
        private readonly TypeBase _second;

        internal Pair(TypeBase first, TypeBase second) : base(first)
        {
            _second = second;
        }

        internal TypeBase First { get { return Parent; } }
        internal TypeBase Second { get { return _second; } }
        internal override Size Size { get { return First.Size + Second.Size; } }

        [DumpData(false)]
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
                return "(" + HWString.Indent(result) + "\n)";
            }
        }

        [DumpData(false)]
        internal protected override TypeBase[] ToList
        {
            get
            {
                var result = new List<TypeBase>(Parent.ToList) {Second};
                return result.ToArray();
            }
        }

        internal override Result DestructorHandler(Category category)
        {
            var firstHandler = First.DestructorHandler(category);
            var secondHandler = Second.DestructorHandler(category);
            if(firstHandler.IsEmpty)
                return secondHandler;
            if(secondHandler.IsEmpty)
                return firstHandler;

            NotImplementedMethod(category);
            throw new NotImplementedException();
        }

        internal override Result DumpPrintFromRef(Category category, RefAlignParam refAlignParam)
        {
            var result = DumpPrintArrayFromRef(category, refAlignParam);
            return Result.ConcatPrintResult(category, result);
        }

        private List<Result> DumpPrintArrayFromRef(Category category, RefAlignParam refAlignParam)
        {
            var result = new List<Result>();
            var list = ToList;
            for(var i = 0; i < list.Length; i++)
            {
                var iResult = list[i].DumpPrintFromRef(category, refAlignParam);
                result.Add(iResult);
            }
            return result;
        }
    }
}