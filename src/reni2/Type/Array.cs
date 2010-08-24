using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Context;
using Reni.Feature;
using Reni.Syntax;

namespace Reni.Type
{
    /// <summary>
    /// Fixed sized array of a type
    /// </summary>
    [Serializable]
    internal sealed class Array : Child
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

        public override string Dump() { return GetType().FullName + "(" + Element.Dump() + ", " + Count + ")"; }

        protected override Result ConvertToImplementation(Category category, TypeBase dest)
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

        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionParameter conversionParameter)
        {
            var destArray = dest as Array;
            if(destArray != null)
            {
                if(Count == destArray.Count)
                    return Element.IsConvertableTo(destArray.Element, conversionParameter.DontUseConverter);
                return false;
            }
            return false;
        }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            searchVisitor.ChildSearch(this);
            base.Search(searchVisitor);
        }

        internal override string DumpShort() { return "(" + Element.DumpShort() + ")array(" + Count + ")"; }

        protected override bool IsInheritor { get { return false; } }
    }

    internal class ConcatArraysFeature : ReniObject, IFeature
    {
        private Array _type;
        public ConcatArraysFeature(Array type) { _type = type; }

        TypeBase IFeature.DefiningType()
        {
            NotImplementedMethod();
            return null;
        }

        Result IFeature.Apply(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }
    }

    internal class ConcatArrayWithObjectFeatureBase : ReniObject
    {
        protected static Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object, ICompileSyntax args, TypeBase elementType, int count)
        {
            var resultType = new Array(elementType, count);

            var categoryWithType = category | Category.Type;

            var leftResult = callContext
                .Result(categoryWithType, @object)
                .AutomaticDereference();

            var rightResult = callContext
                .ConvertedRefResult(categoryWithType, args, elementType.Reference(callContext.RefAlignParam))
                .AutomaticDereference()
                .Align(callContext.AlignBits);

            return resultType.Result
                (
                category,
                () => rightResult.Code.Sequence(leftResult.Code),
                () => leftResult.Refs + rightResult.Refs
                );
        }
    }

    internal class CreateArrayFeature : ConcatArrayWithObjectFeatureBase, IFeature
    {
        Result ApplyResult(
            ContextBase callContext,
            Category category,
            ICompileSyntax @object,
            ICompileSyntax args)
        {
            var elementType = callContext.Type(args).Align(callContext.AlignBits);
            return ApplyResult(callContext, category, @object, args, elementType, 1);
        }

        TypeBase IFeature.DefiningType()
        {
            NotImplementedMethod();
            return null;
        }

        Result IFeature.Apply(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }
    }

    internal class ConcatArrayWithObjectFeature : ConcatArrayWithObjectFeatureBase, IFeature
    {
        private readonly Array _type;

        public ConcatArrayWithObjectFeature(Array type) { _type = type; }

        TypeBase IFeature.DefiningType()
        {
            NotImplementedMethod();
            return null;
        }

        Result ApplyResult(
            ContextBase callContext,
            Category category,
            ICompileSyntax @object,
            ICompileSyntax args) { return ApplyResult(callContext, category, @object, args, _type.Element, _type.Count + 1); }

        Result IFeature.Apply(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }
    }
}