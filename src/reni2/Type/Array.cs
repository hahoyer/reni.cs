using System;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Syntax;

namespace Reni.Type
{
    /// <summary>
    /// Fixed sized array of a type
    /// </summary>
    [Serializable]
    internal sealed class Array : Child, IArray
    {
        private readonly int _count;

        public Array(TypeBase element, int count)
            : base(element)
        {
            _count = count;
            Tracer.Assert(count > 0);
        }

        [Node]
        internal int Count { get { return _count; } }

        [Node]
        internal TypeBase Element { get { return Parent; } }

        protected override Size GetSize() { return Element.Size*_count; }

        internal override string DumpPrintText { get { return "(" + Element.DumpPrintText + ")array(" + Count + ")"; } }

        internal override Result Destructor(Category category) { return Element.ArrayDestructor(category, Count); }

        internal override Result Copier(Category category) { return Element.ArrayCopier(category, Count); }

        internal override Result DumpPrint(Category category) { return Element.ArrayDumpPrint(category, Count); }

        public override string Dump() { return GetType().FullName + "(" + Element.Dump() + ", " + Count + ")"; }

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

        internal override string DumpShort() { return "(" + Element.DumpShort() + ")array(" + Count + ")"; }

        TypeBase IArray.ElementType { get { return Element; } }
        long IArray.Count { get { return Count; } }
    }

    internal interface IArray : IDumpShortProvider
    {
        TypeBase ElementType { get; }
        long Count { get; }
    }

    internal class ConcatArraysFeature : IFeature
    {
        private readonly IArray _type;

        public ConcatArraysFeature(IArray type) { _type = type; }

        Result IFeature.ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object,
                                    ICompileSyntax args) { throw new NotImplementedException(); }
    }

    internal class ConcatArrayWithObjectFeature : IFeature
    {
        private readonly IArray _type;

        public ConcatArrayWithObjectFeature(IArray type) { _type = type; }

        Result IFeature.ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object,
                                    ICompileSyntax args)
        {
            var elementType = _type.ElementType;
            if(elementType == null)
                elementType = callContext.Type(args);

            var count = _type.Count + 1;
            Tracer.Assert(count == (int) count);
            var resultType = new Array(elementType, (int) count);

            var leftResult = callContext.Result(category, @object).AutomaticDereference();
            var rightResult = callContext
                .ConvertedRefResult(category, args, elementType.CreateAutomaticRef(callContext.RefAlignParam))
                .AutomaticDereference();

            return resultType.CreateResult
                (
                category, 
                ()=> rightResult.Code.CreateSequence(leftResult.Code),
                ()=> leftResult.Refs + rightResult.Refs
                );
        }
    }
}