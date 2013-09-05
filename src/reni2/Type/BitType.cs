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
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.Sequence;

namespace Reni.Type
{
    sealed class BitType
        : TypeBase
            , ISymbolProvider<DumpPrintToken, IPath<IPath<IFeatureImplementation, SequenceType>, ArrayType>>
    {
        readonly Root _rootContext;

        internal BitType(Root rootContext) { _rootContext = rootContext; }

        [DisableDump]
        internal override bool IsDataLess { get { return false; } }

        [DisableDump]
        internal override string DumpPrintText { get { return "bit"; } }

        [DisableDump]
        internal override Root RootContext { get { return _rootContext; } }

        protected override Size GetSize() { return Size.Create(1); }

        IPath<IPath<IFeatureImplementation, SequenceType>, ArrayType> ISymbolProvider<DumpPrintToken, IPath<IPath<IFeatureImplementation, SequenceType>, ArrayType>>.Feature
        {
            get
            {
                var feature = Extension
                    .Feature<SequenceType, ArrayType>(DumpPrintTokenResult);
                return feature;
            }
        }

        Result DumpPrintTokenResult(Category category, SequenceType sequenceType, ArrayType arrayType)
        {
            Tracer.Assert(sequenceType.Parent == arrayType);
            Tracer.Assert(arrayType.ElementType == this);
            return VoidType
                .Result(category, sequenceType.DumpPrintNumberCode, CodeArgs.Arg);
        }

        internal Result DumpPrintTokenResult(Category category)
        {
            return VoidType
                .Result(category, DumpPrintNumberCode, CodeArgs.Arg);
        }

        protected override string Dump(bool isRecursion) { return GetType().PrettyName(); }

        protected override string GetNodeDump() { return "bit"; }
        internal SequenceType UniqueNumber(int bitCount) { return UniqueArray(bitCount).UniqueSequence; }
        internal Result Result(Category category, BitsConst bitsConst)
        {
            return UniqueNumber(bitsConst.Size.ToInt())
                .Result(category, getCode: () => CodeBase.BitsConst(bitsConst));
        }
        internal CodeBase ApplyCode(Size size, string token, int objectBits, int argsBits)
        {
            var objectType = UniqueNumber(objectBits).UniqueAlign;
            var argsType = UniqueNumber(argsBits).UniqueAlign;
            return objectType
                .Pair(argsType).ArgCode
                .BitSequenceOperation(token, size, Size.Create(objectBits).ByteAlignedSize);
        }

        internal interface IOperation
        {
            int Signature(int objectBitCount, int argsBitCount);

            [DisableDump]
            string Name { get; }
        }

        internal interface IPrefix
        {
            [DisableDump]
            string Name { get; }
        }

        Result ApplyResult(Category category, IOperation operation, int objectBitCount, TypeBase argsType)
        {
            var typedCategory = category.Typed;
            var argsBitCount = argsType.SequenceLength(this);
            var resultBitCount = operation.Signature(objectBitCount, argsBitCount);
            var result = UniqueNumber(resultBitCount).Result(category, () => ApplyCode(Size.Create(resultBitCount), operation.Name, objectBitCount, argsBitCount), CodeArgs.Arg);
            var objectResult = UniqueNumber(objectBitCount).UniqueObjectReference(Root.DefaultRefAlignParam).Result(typedCategory);
            var convertedObjectResult = objectResult.BitSequenceOperandConversion(typedCategory);
            var convertedArgsResult = argsType.BitSequenceOperandConversion(typedCategory);
            return result.ReplaceArg(convertedObjectResult + convertedArgsResult);
        }

        Result PrefixResult(Category category, string operation, int objectBitCount)
        {
            var objectType = UniqueNumber(objectBitCount);
            return objectType
                .Result(category, () => objectType.BitSequenceOperation(operation), CodeArgs.Arg)
                .ReplaceArg
                (
                    category1
                        => objectType
                            .UniquePointer
                            .ArgResult(category1.Typed).AutomaticDereferenceResult
                            .Align(Root.DefaultRefAlignParam.AlignBits));
        }
    }
}