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
using Reni.Context;
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.Sequence;
using Reni.TokenClasses;

namespace Reni.Type
{
    sealed class ArrayType
        : TypeBase
        , IRepeaterType
            , IFunctionFeature
        , IFeatureImplementation
    {
        [Node]
        internal readonly TypeBase ElementType;
        [Node]
        internal readonly int Count;

        readonly SimpleCache<RepeaterAccessType> _arrayAccessTypeCache;
        readonly SimpleCache<EnableArrayOverSizeType> _enableArrayOverSizeTypeCache;
        readonly SimpleCache<SequenceType> _sequenceCache;
        readonly SimpleCache<TextItemsType> _textItemsCache;

        public ArrayType(TypeBase elementType, int count)
        {
            ElementType = elementType;
            Count = count;
            Tracer.Assert(count > 0);
            Tracer.Assert(elementType.ReferenceType == null);
            Tracer.Assert(!elementType.IsDataLess);
            _arrayAccessTypeCache = new SimpleCache<RepeaterAccessType>(() => new RepeaterAccessType(this));
            _enableArrayOverSizeTypeCache = new SimpleCache<EnableArrayOverSizeType>(() => new EnableArrayOverSizeType(this));
            _sequenceCache = new SimpleCache<SequenceType>(() => new SequenceType(this));
            _textItemsCache = new SimpleCache<TextItemsType>(() => new TextItemsType(this));
        }

        TypeBase IRepeaterType.ElementType { get { return ElementType; } }
        Size IRepeaterType.IndexSize { get { return IndexSize; } }

        [Node]
        [DisableDump]
        internal SequenceType UniqueSequence { get { return _sequenceCache.Value; } }
        [Node]
        [DisableDump]
        internal TextItemsType UniqueTextItemsType { get { return _textItemsCache.Value; } }
        [Node]
        [DisableDump]
        internal EnableArrayOverSizeType EnableArrayOverSizeType { get { return _enableArrayOverSizeTypeCache.Value; } }
        [DisableDump]
        internal override bool IsDataLess { get { return Count == 0 || ElementType.IsDataLess; } }
        [DisableDump]
        public override TypeBase ArrayElementType { get { return ElementType; } }
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
                (
                    category,
                    c => InternalConstructorResult(c, argsType)
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
                .Select(i => ElementConstructorResult(category, constructorResult, i, indexType))
                .Aggregate(VoidType.Result(category), (c, n) => n + c);
            return Result(category, elements);
        }

        Result ElementConstructorResult(Category category, Result elementConstructorResult, int i, TypeBase indexType)
        {
            var resultForArg = indexType
                .Result(category.Typed, () => CodeBase.BitsConst(indexType.Size, BitsConst.Convert(i)));
            return elementConstructorResult
                .ReplaceArg(resultForArg)
                .Conversion(ElementAccessType)
                .ObviousExactConversion(ElementType)
                & category;
        }

        TypeBase ElementAccessType { get { return ElementType.TypeForArrayElement; } }

        [DisableDump]
        internal override sealed Root RootContext { get { return ElementType.RootContext; } }
        [DisableDump]
        IContextReference ObjectReference { get { return UniquePointerType; } }
        [DisableDump]
        internal TypeBase IndexType { get { return RootContext.BitType.UniqueNumber(IndexSize.ToInt()); } }
        Size IndexSize { get { return Size.AutoSize(Count).Align(Root.DefaultRefAlignParam.AlignBits); } }

        IMetaFunctionFeature IFeatureImplementation.MetaFunction { get { return null; } }
        IFunctionFeature IFeatureImplementation.Function { get { return this; } }
        ISimpleFeature IFeatureImplementation.Simple { get { return null; } }
        
        bool IFunctionFeature.IsImplicit { get { return false; } }
        IContextReference IFunctionFeature.ObjectReference { get { return ObjectReference; } }

        protected override string GetNodeDump() { return ElementType.NodeDump + "*" + Count; }

        internal Result ConcatArraysResult(Category category, IContextReference objectReference, TypeBase argsType) { return InternalConcatArrays(category, objectReference, argsType); }
        internal Result ConcatArraysFromReference(Category category, IContextReference objectReference, TypeBase argsType) { return InternalConcatArrays(category, objectReference, argsType); }

        internal Result InternalConcatArrays(Category category, IContextReference objectReference, TypeBase argsType)
        {
            var oldElementsResult = UniquePointer
                .Result(category.Typed, objectReference).DereferenceResult;

            var isElementArg = argsType.IsConvertable(ElementAccessType);
            var newCount = isElementArg ? 1 : argsType.ArrayLength(ElementAccessType);
            var newElementsResultRaw
                = isElementArg
                    ? argsType.Conversion(category.Typed, ElementAccessType)
                    : argsType.Conversion(category.Typed, ElementType.UniqueArray(newCount));

            var newElementsResult = newElementsResultRaw.DereferencedAlignedResult();
            var result = ElementType
                .UniqueArray(Count + newCount)
                .Result(category, newElementsResult + oldElementsResult);
            return result;
        }

        internal Result DumpPrintTokenResult(Category category)
        {
            return VoidType
                .Result
                (
                    category,
                    CreateDumpPrintCode,
                    () => ElementType.GenericDumpPrintResult(Category.CodeArgs).CodeArgs
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

        internal Result SequenceTokenResult(Category category)
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

        Result IFunctionFeature.ApplyResult(Category category, TypeBase argsType) { return ApplyResult(category, argsType); }
        Result ApplyResult(Category category, TypeBase argsType)
        {
            var objectResult = UniquePointer
                .Result(category, ObjectReference);

            var argsResult = argsType
                .Conversion(category.Typed, IndexType)
                .DereferencedAlignedResult();

            var result = _arrayAccessTypeCache
                .Value
                .Result(category, objectResult + argsResult);

            return result;
        }

        internal Result EnableArrayOverSizeResult(Category category)
        {
            return EnableArrayOverSizeType
                .Result
                (
                    category,
                    () => PointerKind.ArgCode.DePointer(Size),
                    CodeArgs.Arg
                );
        }
    }
}