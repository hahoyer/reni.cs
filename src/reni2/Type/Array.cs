//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Syntax;

namespace Reni.Type
{
    /// <summary>
    ///     Fixed sized array of a type
    /// </summary>
    [Serializable]
    internal sealed class Array : Child
    {
        private readonly DictionaryEx<RefAlignParam, ConcatArraysFeature> _concatArrayWithObjectFeatureCache;

        private readonly int _count;

        public Array(TypeBase element, int count)
            : base(element)
        {
            _count = count;
            Tracer.Assert(count > 0);
            _concatArrayWithObjectFeatureCache = new DictionaryEx<RefAlignParam, ConcatArraysFeature>(refAlignParam => new ConcatArraysFeature(this,refAlignParam));
        }

        [Node]
        internal int Count { get { return _count; } }

        [Node]
        internal TypeBase Element { get { return Parent; } }
        internal override string DumpPrintText { get { return "(" + Element.DumpPrintText + ")array(" + Count + ")"; } }

        protected override Size GetSize() { return Element.Size*_count; }

        internal override Result Destructor(Category category) { return Element.ArrayDestructor(category, Count); }

        internal override Result Copier(Category category) { return Element.ArrayCopier(category, Count); }

        protected override string Dump(bool isRecursion)
        {
            if(isRecursion)
                return "ObjectId=" + ObjectId;
            return GetType().PrettyName() + "(" + Element.Dump() + ", " + Count + ")";
        }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            searchVisitor.ChildSearch(this);
            base.Search(searchVisitor);
        }

        internal override string DumpShort() { return "(" + Element.DumpShort() + ")array(" + Count + ")"; }

        protected override bool IsInheritor { get { return false; } }

        internal Result ConcatArrays(Category category, RefAlignParam refAlignParam)
        {
            NotImplementedMethod(category, refAlignParam);
            return null;
        }
        internal Result ConcatArrayWithObject(Category category, RefAlignParam refAlignParam)
        {
            return _concatArrayWithObjectFeatureCache
                .Find(refAlignParam)
                .Result(category, () => ReferenceArgCode(refAlignParam));
        }

        internal Result DumpPrintResult(Category category, RefAlignParam refAlignParam)
        {
            return Void
                .Result
                (category
                 , () => DumpPrintCode(refAlignParam)
                 , () => Element.GenericDumpPrintResult(Category.Refs, refAlignParam).Refs
                );
        }

        private CodeBase DumpPrintCode(RefAlignParam refAlignParam)
        {
            var elementReference = Element.UniqueAutomaticReference(refAlignParam);
            var argCode = ReferenceArgCode(refAlignParam);
            var elementDumpPrint = Element.GenericDumpPrintResult(Category.Code, refAlignParam).Code;
            var code = CodeBase.DumpPrintText("array(" + Element.DumpPrintText + ",(");
            for(var i = 0; i < Count; i++)
            {
                if(i > 0)
                    code = code.Sequence(CodeBase.DumpPrintText(", "));
                var elemCode = elementDumpPrint.ReplaceArg(elementReference, argCode.AddToReference(refAlignParam, Element.Size*i));
                code = code.Sequence(elemCode);
            }
            code = code.Sequence(CodeBase.DumpPrintText("))"));
            return code;
        }
    }

    internal sealed class ConcatArraysFeature : TypeBase, IFunctionalFeature
    {
        [EnableDump]
        private Array _type;
        private readonly RefAlignParam _refAlignParam;

        public ConcatArraysFeature(Array type, RefAlignParam refAlignParam)
        {
            _type = type;
            _refAlignParam = refAlignParam;
        }

        protected override Size GetSize() { return _refAlignParam.RefSize; }
        internal override IFunctionalFeature FunctionalFeature { get { return this; } }

        Result IFunctionalFeature.ObtainApplyResult(Category category, Result operationResult, Result argsResult, RefAlignParam refAlignParam)
        {
            var newElementResult = argsResult.Conversion(_type.Element);
            return _type
                .Element
                .UniqueArray(_type.Count + 1)
                .Result
                (category
                 , () => newElementResult.Code.Sequence(operationResult.Code.Dereference(refAlignParam,_type.Size))
                 , () => newElementResult.Refs + operationResult.Refs
                );
        }
    }

    internal abstract class ConcatArrayWithObjectFeatureBase : TypeBase
    {
        protected override Size GetSize() { return Size.Zero; }

        protected static Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object, ICompileSyntax args, TypeBase elementType, int count)
        {
            //var resultType = new Array(elementType, count);

            //var categoryWithType = category | Category.Type;

            //var leftResult = callContext
            //    .Result(categoryWithType, @object)
            //    .AutomaticDereference();

            //var rightResult = callContext
            //    .ConvertedRefResult(categoryWithType, args, elementType.UniqueReference(callContext.RefAlignParam))
            //    .AutomaticDereference()
            //    .Align(callContext.AlignBits);

            //return resultType.Result
            //    (
            //        category,
            //        () => rightResult.Code.Sequence(leftResult.Code),
            //        () => leftResult.Refs + rightResult.Refs
            //    );
            return null;
        }
    }

    internal sealed class CreateArrayFeature : ConcatArrayWithObjectFeatureBase, IFunctionalFeature
    {
        private Result ApplyResult(
            ContextBase callContext,
            Category category,
            ICompileSyntax @object,
            ICompileSyntax args)
        {
            var elementType = callContext.Type(args).Align(callContext.AlignBits);
            return ApplyResult(callContext, category, @object, args, elementType, 1);
        }

        internal override IFunctionalFeature FunctionalFeature { get { return this; } }

        Result IFunctionalFeature.ObtainApplyResult(Category category, Result operationResult, Result argsResult, RefAlignParam refAlignParam)
        {
            var result = argsResult.AutomaticDereference().Align(refAlignParam.AlignBits);
            return new Result
                (category
                 , () => result.Size
                 , () => result.Type.UniqueArray(1)
                 , () => result.Code
                 , () => result.Refs
                );
        }
    }

    internal sealed class ConcatArrayWithObjectFeature : ConcatArrayWithObjectFeatureBase, IFeature
    {
        private readonly Array _type;

        public ConcatArrayWithObjectFeature(Array type) { _type = type; }

        TypeBase IFeature.ObjectType
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        private Result ApplyResult(
            ContextBase callContext,
            Category category,
            ICompileSyntax @object,
            ICompileSyntax args) { return ApplyResult(callContext, category, @object, args, _type.Element, _type.Count + 1); }

        Result IFeature.ObtainResult(Category category, RefAlignParam refAlignParam)
        {
            NotImplementedMethod(category);
            return null;
        }
    }
}