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
using Reni.Type;

namespace Reni.Sequence
{
    [Serializable]
    sealed class SequenceType
        : TagChild<ArrayType>
            , ISearchPath<ISuffixFeature, SequenceType>
            , ISearchPath<ISearchPath<ISuffixFeature, EnableCut>, SequenceType>
    {
        readonly DictionaryEx<RefAlignParam, ObjectReference> _objectReferencesCache;

        internal Result EnableCutFeature(Category category)
        {
            return UniqueEnableCutType
                .Result
                (
                    category
                    ,
                    () => PointerKind.ArgCode.DePointer(Size)
                    ,
                    CodeArgs.Arg
                );
        }

        public SequenceType(ArrayType parent)
            : base(parent)
        {
            _objectReferencesCache = new DictionaryEx<RefAlignParam, ObjectReference>
                (refAlignParam => new ObjectReference(this, refAlignParam));
            StopByObjectId(-172);
        }

        [DisableDump]
        protected override string TagTitle { get { return "Sequence"; } }
        [DisableDump]
        internal int Count { get { return Parent.Count; } }
        [Node]
        [DisableDump]
        public TypeBase Element { get { return Parent.ElementType; } }
        [DisableDump]
        internal override string DumpPrintText { get { return "(" + Element.DumpPrintText + ")sequence(" + Count + ")"; } }

        internal new ISuffixFeature Feature(FeatureBase featureBase) { return new FunctionFeature(this, featureBase); }
        internal IPrefixFeature PrefixFeature(ISequenceOfBitPrefixOperation definable) { return new PrefixFeature(this, definable); }

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

        internal Result ConcatArrays(Category category, IContextReference objectReference, TypeBase argsType)
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
                    category
                    ,
                    () => Element.UniqueArray(oldCount).UniqueSequence.ArgCode.BitCast(Size)
                    ,
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

        Result FlatConversion(Category category, SequenceType source)
        {
            var result = source.ArgResult(category.Typed);
            if(source.Count > Count)
                result = source.RemoveElementsAtEnd(category, Count);

            if(source.Element != Element)
            {
                DumpDataWithBreak
                    (
                        "Element type dismatch"
                        ,
                        "category",
                        category
                        ,
                        "source",
                        source
                        ,
                        "destination",
                        this
                        ,
                        "result",
                        result
                    );
                return null;
            }

            if(source.Count < Count)
                result = ExtendFrom(category, source.Count).ReplaceArg(result);
            return result;
        }

        Result ConversionAsReference(Category category, SequenceType source)
        {
            var trace = ObjectId == -3;
            StartMethodDump(trace, category, source);
            try
            {
                var flatResult = FlatConversion(category, source);
                Dump("flatResult", flatResult);
                Func<Category, Result> forArg = source.UnalignedDereferencePointerResult;
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

        ISuffixFeature ISearchPath<ISuffixFeature, SequenceType>.Convert(SequenceType type)
        {
            return Count < type.Count
                ? null
                : Extension.Feature(c => ConversionAsReference(c, type));
        }

        ISearchPath<ISuffixFeature, EnableCut>
            ISearchPath<ISearchPath<ISuffixFeature, EnableCut>, SequenceType>.Convert(SequenceType type)
        {
            return
                Extension.Feature<EnableCut>((c, t) => ConvertFromEnableCut(c, type));
        }

        Result ConvertFromEnableCut(Category category, SequenceType source)
        {
            var trace = ObjectId == -10;
            StartMethodDump(trace, category, source);
            try
            {
                var result = ConversionAsReference(category, source);
                Dump("result", result);

                Func<Category, Result> forArg = source.RemoveEnableCutReferenceResult;
                if(trace)
                    Dump("forArg", forArg(Category.All));

                return ReturnMethodDump(result.ReplaceArg(source.RemoveEnableCutReferenceResult));
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

        internal Result UndecorateResult(Category category) { return Parent.Result(category, ArgResult); }
    }
}