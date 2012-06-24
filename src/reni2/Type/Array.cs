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
using Reni.Context;
using Reni.Feature;
using Reni.Sequence;

namespace Reni.Type
{
    /// <summary>
    ///     Fixed sized array of a type
    /// </summary>
    [Serializable]
    sealed class Array
        : TypeBase
          , ISearchPath<ISuffixFeature, SequenceType>
    {
        readonly TypeBase _element;
        readonly int _count;
        readonly SimpleCache<SequenceType> _sequenceCache;
        readonly SimpleCache<TextItemsType> _textItemsCache;

        public Array(TypeBase element, int count)
        {
            _element = element;
            _count = count;
            Tracer.Assert(count > 0);
            Tracer.Assert(element.Reference == null);
            _sequenceCache = new SimpleCache<SequenceType>(() => new SequenceType(this));
            _textItemsCache = new SimpleCache<TextItemsType>(() => new TextItemsType(this));
        }

        [Node]
        [DisableDump]
        internal SequenceType UniqueSequence { get { return _sequenceCache.Value; } }
        [Node]
        [DisableDump]
        internal TextItemsType UniqueTextItemsType { get { return _textItemsCache.Value; } }
        [Node]
        [DisableDump]
        internal int Count { get { return _count; } }
        [Node]
        [DisableDump]
        internal TypeBase Element { get { return _element; } }
        [DisableDump]
        internal override bool IsDataLess { get { return Count == 0 || Element.IsDataLess; } }
        internal override string DumpPrintText { get { return "(" + Element.DumpPrintText + ")array(" + Count + ")"; } }

        internal override int? SmartArrayLength(TypeBase elementType) { return Element.IsConvertable(elementType) ? Count : base.SmartArrayLength(elementType); }
        protected override Size GetSize() { return Element.Size * _count; }
        internal override Result Destructor(Category category) { return Element.ArrayDestructor(category, Count); }
        internal override Result Copier(Category category) { return Element.ArrayCopier(category, Count); }

        internal override void Search(SearchVisitor searchVisitor)
        {
            searchVisitor.Search(this, () => Element);
            if(!searchVisitor.IsSuccessFull)
                base.Search(searchVisitor);
        }

        internal override Result ConstructorResult(Category category, TypeBase argsType)
        {
            return Result
                (category
                 , c => ConstructorResultExec(c, argsType)
                );
        }

        Result ConstructorResultExec(Category category, TypeBase argsType)
        {
            if(category.IsNone)
                return null;

            var function = (IFunctionFeature) argsType;
            var indexType = Bit
                .UniqueArray(BitsConst.Convert(Count).Size.ToInt())
                .UniqueSequence
                .UniqueAlign(Root.DefaultRefAlignParam.AlignBits);
            var constructorResult = function.ApplyResult(category.Typed, indexType);
            var elements = Count
                .Array(i => ElementConstructorResult(category, constructorResult, i, indexType))
                .Aggregate(Void.Result(category), (c, n) => n + c);
            return Result(category, elements);
        }

        Result ElementConstructorResult(Category category, Result elementConstructorResult, int i, TypeBase indexType)
        {
            var resultForArg = indexType
                .Result(category.Typed, () => CodeBase.BitsConst(indexType.Size, BitsConst.Convert(i)));
            return elementConstructorResult
                       .ReplaceArg(resultForArg)
                       .Conversion(Element)
                   & category;
        }

        internal override string DumpShort() { return Element.DumpShort() + "*" + Count; }

        internal Result ConcatArrays(Category category, IContextReference objectReference, TypeBase argsType) { return InternalConcatArrays(category, objectReference, argsType); }
        internal Result ConcatArraysFromReference(Category category, IContextReference objectReference, TypeBase argsType) { return InternalConcatArrays(category, objectReference, argsType); }

        internal Result InternalConcatArrays(Category category, IContextReference objectReference, TypeBase argsType)
        {
            var oldElementsResult = UniqueReference
                .Type()
                .Result(category.Typed, objectReference)
                .DereferenceResult();

                var newElementsResult = argsType.TryConversion(category,Element);
            var newCount = 1;
            if (newElementsResult == null)
            {
                newCount = argsType.ArrayLength(Element);
                newElementsResult = argsType.Conversion(category, Element.UniqueArray(newCount));
            }

            var result = Element
                .UniqueArray(Count + newCount)
                .Result(category, newElementsResult + oldElementsResult);
            return result;
        }

        internal Result DumpPrintResult(Category category)
        {
            return Void
                .Result
                (category
                 , CreateDumpPrintCode
                 , () => Element.GenericDumpPrintResult(Category.CodeArgs).CodeArgs
                );
        }

        CodeBase CreateDumpPrintCode()
        {
            var elementReference = Element.UniqueReference.Type();
            var argCode = UniqueReference.Type().ArgCode;
            var elementDumpPrint = elementReference.GenericDumpPrintResult(Category.Code).Code;
            var code = CodeBase.DumpPrintText("array(" + Element.DumpPrintText + ",(");
            for(var i = 0; i < Count; i++)
            {
                if(i > 0)
                    code = code.Sequence(CodeBase.DumpPrintText(", "));
                var elemCode = elementDumpPrint.ReplaceArg(elementReference, argCode.AddToReference(Element.Size * i));
                code = code.Sequence(elemCode);
            }
            code = code.Sequence(CodeBase.DumpPrintText("))"));
            return code;
        }

        internal Result SequenceResult(Category category)
        {
            return Element
                .AutomaticDereferenceType
                .UniqueArray(Count)
                .UniqueSequence
                .UniqueReference.Type()
                .Result(category, UniqueReference.Type().ArgResult(category));
        }

        ISuffixFeature ISearchPath<ISuffixFeature, SequenceType>.Convert(SequenceType type)
        {
            if(type.Parent != this)
                return null;
            return Extension.Feature(type.ReferenceConversionResult);
        }
    }
}