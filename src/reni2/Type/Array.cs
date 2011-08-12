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
using Reni.Feature;

namespace Reni.Type
{
    /// <summary>
    ///     Fixed sized array of a type
    /// </summary>
    [Serializable]
    internal sealed class Array : Child
    {
        private readonly DictionaryEx<Size, ConcatArraysFeature> _concatArraysFeatureCache;

        private readonly int _count;
        public readonly ISearchPath<IFeature, ReferenceType> ConcatArraysFromReferenceFeature;

        public Array(TypeBase element, int count)
            : base(element)
        {
            ConcatArraysFromReferenceFeature = new ConcatArraysFromReferenceFeature(this);
            _count = count;
            Tracer.Assert(count > 0);
            _concatArraysFeatureCache = new DictionaryEx<Size, ConcatArraysFeature>(size => new ConcatArraysFeature(this, size));
        }

        [Node]
        internal int Count { get { return _count; } }

        [Node]
        internal TypeBase Element { get { return Parent; } }
        internal override string DumpPrintText { get { return "(" + Element.DumpPrintText + ")array(" + Count + ")"; } }
        [DisableDump]
        internal override int ArrayElementCount { get { return Count; } }
        [DisableDump]
        internal override bool IsArray { get { return true; } }

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

        internal override string DumpShort() { return base.DumpShort() + "(" + Element.DumpShort() + ")array(" + Count + ")"; }

        protected override bool IsInheritor { get { return false; } }

        internal Result ConcatArrays(Category category, RefAlignParam refAlignParam)
        {
            return _concatArraysFeatureCache
                .Find(refAlignParam.RefSize)
                .Result(category, ReferenceArgResult(category, refAlignParam));
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
            var argCode = UniqueAutomaticReference(refAlignParam).ArgCode();
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

        internal Result Result(Category category, IFunctionalFeature functionalFeature)
        {
            var operationResult = VoidResult(category);
            var result = VoidResult(category);
            for(var i = 0; i < Count; i++)
            {
                var index = BitsConst.Convert(i);
                var argsResult = UniqueNumber(index.Size.ToInt())
                    .Result(category.Typed, () => CodeBase.BitsConst(index), Refs.ArgLess);
                var rawResult = functionalFeature.ObtainApplyResult(category, operationResult, argsResult, null);
                var convertedResult = rawResult.Conversion(Element) & result.CompleteCategory;
                result = convertedResult.Sequence(result);
            }
            return Result(category, result);
        }
        
        internal Result SequenceResult(Category category, RefAlignParam refAlignParam)
        {
            return Element
                .UniqueSequence(Count)
                .UniqueAutomaticReference(refAlignParam)
                .Result(category, UniqueAutomaticReference(refAlignParam).ArgResult(category));
        }
    }
}