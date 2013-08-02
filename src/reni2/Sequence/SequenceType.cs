#region Copyright (C) 2013

//     Project Reni2
//     Copyright (C) 2011 - 2013 Harald Hoyer
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

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Code;
using Reni.Feature;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Sequence
{
    sealed class SequenceType
        : TagChild<ArrayType>
            , ISymbolFeature<ConcatArrays>
            , ISymbolFeature<TokenClasses.EnableCut>
            , ISymbolFeature<UndecorateToken>
            , IPathFeature<IConversionFeature, SequenceType>
            , IPathFeature<IConversionFeature, ArrayType>
    {
        readonly DictionaryEx<RefAlignParam, ObjectReference> _objectReferencesCache;

        internal Result EnableCutResult(Category category)
        {
            return UniqueEnableCutType
                .Result
                (
                    category,
                    () => PointerKind.ArgCode.DePointer(Size),
                    CodeArgs.Arg
                );
        }

        public SequenceType(ArrayType parent)
            : base(parent)
        {
            _objectReferencesCache = new DictionaryEx<RefAlignParam, ObjectReference>(refAlignParam => new ObjectReference(this, refAlignParam));
            StopByObjectId(-172);
        }

        IFeatureImplementation ISymbolFeature<ConcatArrays>.Feature { get { return Extension.Feature(ConcatArraysResult); } }
        IFeatureImplementation ISymbolFeature<TokenClasses.EnableCut>.Feature { get { return Extension.Feature(EnableCutResult); } }
        IFeatureImplementation ISymbolFeature<UndecorateToken>.Feature { get { return Extension.Feature(UndecorateTokenResult); } }

        IConversionFeature IPathFeature<IConversionFeature, SequenceType>.Convert(SequenceType target) { throw new NotImplementedException(); }
        IConversionFeature IPathFeature<IConversionFeature, ArrayType>.Convert(ArrayType target) { throw new NotImplementedException(); }

        Simple GetFeature(SequenceType target) { return ConversionFeature(target); }
        Simple GetFeature(ArrayType target) { return ConversionFeature(target); }
        Simple Convert2(SequenceType type) { return type.ConversionFeature(this); }

        [DisableDump]
        protected override string TagTitle { get { return "Sequence"; } }
        [DisableDump]
        internal int Count { get { return Parent.Count; } }
        [Node]
        [DisableDump]
        public TypeBase Element { get { return Parent.ElementType; } }
        [DisableDump]
        internal override string DumpPrintText { get { return "(" + Element.DumpPrintText + ")sequence(" + Count + ")"; } }


        Simple ConversionFeature(SequenceType destination)
        {
            return destination.Count >= Count
                ? Extension.Feature(c => ConversionAsReference(c, destination))
                : null;
        }

        internal Simple ConversionFeature(ArrayType destination)
        {
            return Parent == destination
                ? Extension.Feature(PointerConversionResult)
                : null;
        }

        internal override int? SmartSequenceLength(TypeBase elementType)
        {
            return Parent
                .SmartArrayLength(elementType);
        }

        internal override int? SmartArrayLength(TypeBase elementType)
        {
            if(elementType is SequenceType && IsConvertable(elementType))
                return 1;
            NotImplementedMethod(elementType);
            return null;
        }

        internal override void Search(SearchVisitor searchVisitor)
        {
            searchVisitor.Search(this, () => Parent);
            if(!searchVisitor.IsSuccessFull)
                base.Search(searchVisitor);
        }

        internal Result ConcatArraysResult(Category category, IContextReference objectReference, TypeBase argsType)
        {
            var trace = ObjectId == -1 && category.HasCode;
            StartMethodDump(trace, category, objectReference, argsType);
            try
            {
                var result = Parent.InternalConcatArrays(category.Typed, objectReference, argsType);
                Dump("result", result);
                BreakExecution();

                var type = (ArrayType) result.Type;
                return ReturnMethodDump(type.UniqueSequence.Result(category, result));
            }
            finally
            {
                EndMethodDump();
            }
        }

        Result ExtendFrom(Category category, int oldCount)
        {
            var result = Result
                (
                    category,
                    () => Element.UniqueArray(oldCount).UniqueSequence.ArgCode.BitCast(Size),
                    CodeArgs.Arg
                );
            return result;
        }

        Result RemoveElementsAtEnd(Category category, int newCount)
        {
            var destructor = Element.Destructor(category);
            if(!destructor.IsEmpty)
            {
                NotImplementedMethod(category, newCount, "destructor", destructor);
                return null;
            }
            var tempNewCount = Math.Min(Count, newCount);
            var newType = Element.UniqueArray(tempNewCount).UniqueSequence;
            var result = newType
                .Result
                (
                    category
                    ,
                    () => ArgCode.BitCast(newType.Size)
                    ,
                    CodeArgs.Arg
                );
            return result;
        }

        internal ObjectReference UniqueObjectReference(RefAlignParam refAlignParam) { return _objectReferencesCache[refAlignParam]; }

        Result FlatConversion(Category category, SequenceType destination)
        {
            var result = ArgResult(category.Typed);
            if(Count > destination.Count)
                result = RemoveElementsAtEnd(category, destination.Count);

            if(Element != destination.Element)
            {
                DumpDataWithBreak
                    (
                        "Element type dismatch",
                        "category", category,
                        "source", this,
                        "destination", destination,
                        "result", result
                    );
                return null;
            }

            if(Count < destination.Count)
                result = destination.ExtendFrom(category, Count).ReplaceArg(result);
            return result;
        }

        Result ConversionAsReference(Category category, SequenceType destination)
        {
            var trace = ObjectId == -3;
            StartMethodDump(trace, category, this);
            try
            {
                var flatResult = FlatConversion(category, destination);
                Dump("flatResult", flatResult);
                Func<Category, Result> forArg = UnalignedDereferencePointerResult;
                if(trace)
                    Dump("forArg", forArg(Category.All));
                var result = flatResult.ReplaceArg(forArg);
                Dump("result", result);
                return ReturnMethodDump(result.LocalPointerKindResult);
            }
            finally
            {
                EndMethodDump();
            }
        }

        Result UnalignedDereferencePointerResult(Category category) { return PointerKind.ArgResult(category.Typed).DereferenceResult & category; }

        Result ConvertWithCut(Category category, SequenceType destination)
        {
            var trace = ObjectId == -10;
            StartMethodDump(trace, category, this);
            try
            {
                var result = ConversionAsReference(category, destination);
                Dump("result", result);

                Func<Category, Result> forArg = RemoveEnableCutReferenceResult;
                if(trace)
                    Dump("forArg", forArg(Category.All));

                return ReturnMethodDump(result.ReplaceArg(RemoveEnableCutReferenceResult));
            }
            finally
            {
                EndMethodDump();
            }
        }

        Result RemoveEnableCutReferenceResult(Category category)
        {
            return UniquePointer
                .Result(category, EnableCutReferenceResult);
        }

        Result EnableCutReferenceResult(Category c)
        {
            return UniqueEnableCutType
                .UniquePointer
                .ArgResult(c.Typed);
        }

        internal Result UndecorateTokenResult(Category category) { return Parent.Result(category, ArgResult); }
    }
}