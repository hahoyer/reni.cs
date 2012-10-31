#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
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
using Reni.Sequence;

namespace Reni.Type
{
    sealed class ArrayType
        : RepeaterType
          , ISearchPath<ISuffixFeature, SequenceType>
    {
        [Node]
        readonly SimpleCache<SequenceType> _sequenceCache;
        [Node]
        readonly SimpleCache<TextItemsType> _textItemsCache;

        public ArrayType(TypeBase elementType, int count)
            : base(elementType, count)
        {
            _sequenceCache = new SimpleCache<SequenceType>(() => new SequenceType(this));
            _textItemsCache = new SimpleCache<TextItemsType>(() => new TextItemsType(this));
        }

        [Node]
        [DisableDump]
        internal SequenceType UniqueSequence { get { return _sequenceCache.Value; } }
        [Node]
        [DisableDump]
        internal TextItemsType UniqueTextItemsType { get { return _textItemsCache.Value; } }
        [DisableDump]
        internal override bool IsDataLess { get { return Count == 0 || ElementType.IsDataLess; } }
        [DisableDump]
        protected override ReferenceType SmartReference { get { return ElementType.UniqueReference(Count); } }
        internal override string DumpPrintText { get { return "(" + ElementType.DumpPrintText + ")array(" + Count + ")"; } }

        internal override int? SmartArrayLength(TypeBase elementType) { return ElementType.IsConvertable(elementType) ? Count : base.SmartArrayLength(elementType); }
        protected override Size GetSize() { return ElementType.Size * Count; }
        internal override Result Destructor(Category category) { return ElementType.ArrayDestructor(category, Count); }
        internal override Result Copier(Category category) { return ElementType.ArrayCopier(category, Count); }

        internal override void Search(SearchVisitor searchVisitor)
        {
            searchVisitor.Search(this, () => ElementType);
            if(!searchVisitor.IsSuccessFull)
                base.Search(searchVisitor);
        }

        internal Result TextItemsResult(Category category) { return UniqueTextItemsType.PointerResult(category, PointerArgResult); }

        internal override Result ConstructorResult(Category category, TypeBase argsType)
        {
            return Result
                (category
                 , c => InternalConstructorResult(c, argsType)
                );
        }

        Result InternalConstructorResult(Category category, TypeBase argsType)
        {
            if(category.IsNone)
                return null;

            if(argsType == VoidType)
                return Result(category, () => CodeBase.BitsConst(Size, BitsConst.Convert(0)));

            var function = argsType as IFunctionFeature;
            if(function != null)
                return InternalConstructorResult(category, function);

            NotImplementedMethod(category, argsType);
            return null;
        }
        Result InternalConstructorResult(Category category, IFunctionFeature function)
        {
            var indexType = BitType
                .UniqueArray(BitsConst.Convert(Count).Size.ToInt())
                .UniqueSequence.UniqueAlign;
            var constructorResult = function.ApplyResult(category.Typed, indexType);
            var elements = Count
                .Array(i => ElementConstructorResult(category, constructorResult, i, indexType))
                .Aggregate(VoidType.Result(category), (c, n) => n + c);
            return Result(category, elements);
        }

        Result ElementConstructorResult(Category category, Result elementConstructorResult, int i, TypeBase indexType)
        {
            var resultForArg = indexType
                .Result(category.Typed, () => CodeBase.BitsConst(indexType.Size, BitsConst.Convert(i)));
            return elementConstructorResult
                       .ReplaceArg(resultForArg)
                       .Conversion(ElementType.UnAlignedType)
                       .ObviousExactConversion(ElementType)
                   & category;
        }

        protected override string GetNodeDump() { return ElementType.NodeDump + "*" + Count; }

        internal Result ConcatArrays(Category category, IContextReference objectReference, TypeBase argsType) { return InternalConcatArrays(category, objectReference, argsType); }
        internal Result ConcatArraysFromReference(Category category, IContextReference objectReference, TypeBase argsType) { return InternalConcatArrays(category, objectReference, argsType); }

        internal Result InternalConcatArrays(Category category, IContextReference objectReference, TypeBase argsType)
        {
            var oldElementsResult = UniquePointer
                .Result(category.Typed, objectReference)
                .DereferenceResult();

            var isElementArg = argsType.IsConvertable(ElementType.UnAlignedType);
            var newCount = isElementArg ? 1 : argsType.ArrayLength(ElementType.UnAlignedType);
            var newElementsResultRaw
                = isElementArg
                      ? argsType.Conversion(category.Typed, ElementType.UnAlignedType)
                      : argsType.Conversion(category.Typed, ElementType.UniqueArray(newCount));

            var newElementsResult = newElementsResultRaw.DereferencedAlignedResult();
            var result = ElementType
                .UniqueArray(Count + newCount)
                .Result(category, newElementsResult + oldElementsResult);
            return result;
        }

        internal Result DumpPrintResult(Category category)
        {
            return VoidType
                .Result
                (category
                 , CreateDumpPrintCode
                 , () => ElementType.GenericDumpPrintResult(Category.CodeArgs).CodeArgs
                );
        }

        CodeBase CreateDumpPrintCode()
        {
            var elementReference = ElementType.UniquePointer;
            var argCode = UniquePointer.ArgCode;
            var elementDumpPrint = elementReference.GenericDumpPrintResult(Category.Code).Code;
            var code = CodeBase.DumpPrintText("array(" + ElementType.DumpPrintText + ",(");
            for(var i = 0; i < Count; i++)
            {
                if(i > 0)
                    code = code + CodeBase.DumpPrintText(", ");
                var elemCode = elementDumpPrint.ReplaceArg(elementReference, argCode.ReferencePlus(ElementType.Size * i));
                code = code + elemCode;
            }
            return code + CodeBase.DumpPrintText("))");
        }

        internal Result SequenceResult(Category category)
        {
            return ElementType
                .AutomaticDereferenceType
                .UniqueArray(Count)
                .UniqueSequence
                .UniquePointer
                .Result(category, UniquePointer.ArgResult(category));
        }

        internal Result SequenceTypeResult(Category category)
        {
            return UniqueSequence
                .UniqueTypeType
                .Result(category);
        }
        ISuffixFeature ISearchPath<ISuffixFeature, SequenceType>.Convert(SequenceType type)
        {
            if(type.Parent != this)
                return null;
            return Extension.Feature(type.ReferenceConversionResult);
        }
    }
}