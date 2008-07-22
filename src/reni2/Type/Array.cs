﻿using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace Reni.Type
{
    /// <summary>
    /// Fixed sized array of a type
    /// </summary>
    internal sealed class Array : Child
    {
        private readonly int _count;

        public Array(TypeBase element, int count) : base(element)
        {
            _count = count;
            Tracer.Assert(count > 0);
        }

        [Node]
        internal int Count { get { return _count; } }
        [Node]
        internal TypeBase Element { get { return Parent; } }

        protected override Size GetSize()
        {
            return Element.Size*_count;
        }

        internal override string DumpPrintText { get { return "(" + Element.DumpPrintText + ")array(" + Count + ")"; } }

        internal override Result DestructorHandler(Category category)
        {
            return Element.ArrayDestructorHandler(category, Count);
        }

        internal override Result MoveHandler(Category category)
        {
            return Element.ArrayMoveHandler(category, Count);
        }

        internal override Result DumpPrint(Category category)
        {
            return Element.ArrayDumpPrint(category, Count);
        }

        public override string Dump()
        {
            return GetType().FullName + "(" + Element.Dump() + ", " + Count + ")";
        }

        internal override Result ConvertToVirt(Category category, TypeBase dest)
        {
            var destArray = dest as Array;
            if(destArray != null)
            {
                var result = Element.ConvertTo(category, destArray.Element);
                NotImplementedMethod(category, dest, "result", result);
                return null;
            }
            NotImplementedMethod(category, dest);
            return null;
        }

        internal override bool IsConvertableToVirt(TypeBase dest, ConversionFeature conversionFeature)
        {
            var destArray = dest as Array;
            if(destArray != null)
            {
                if(Count == destArray.Count)
                    return Element.IsConvertableTo(destArray.Element, conversionFeature.DontUseConverter);
                return false;
            }
            return false;
        }

        internal override string DumpShort()
        {
            return "(" + Element.DumpShort() + ")array(" + Count + ")";
        }
    }
}