using HWClassLibrary.TreeStructure;
using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace Reni.Type
{
    /// <summary>
    /// Fixed sized array of a type
    /// </summary>
    [Serializable]
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

        internal override Result Destructor(Category category)
        {
            return Element.ArrayDestructor(category, Count);
        }

        internal override Result Copier(Category category)
        {
            return Element.ArrayCopier(category, Count);
        }

        internal override Result DumpPrint(Category category)
        {
            return Element.ArrayDumpPrint(category, Count);
        }

        public override string Dump()
        {
            return GetType().FullName + "(" + Element.Dump() + ", " + Count + ")";
        }

        internal override Result ConvertToImplementation(Category category, TypeBase dest)
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

        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionFeature conversionFeature)
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