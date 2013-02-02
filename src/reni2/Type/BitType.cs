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
using Reni.TokenClasses;

namespace Reni.Type
{
    [Serializable]
    sealed class BitType : TypeBase
        , IFeaturePath<ISuffixFeature, DumpPrintToken>
        , IFeaturePath<ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType>, DumpPrintToken>
        , IFeaturePath<ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType>, Minus>
        , IFeaturePath<ISearchPath<ISearchPath<IPrefixFeature, SequenceType>, ArrayType>, Minus>
        , IFeaturePath<ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType>, Slash>
        , IFeaturePath<ISearchPath<ISearchPath<IPrefixFeature, SequenceType>, ArrayType>, Plus>
        , IFeaturePath<ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType>, Plus>
        , IFeaturePath<ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType>, Star>
        , IFeaturePath<ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType>, Equal>
        , IFeaturePath<ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType>, NotEqual>
        , IFeaturePath<ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType>, Less>
        , IFeaturePath<ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType>, LessEqual>
        , IFeaturePath<ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType>, Greater>
        , IFeaturePath<ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType>, GreaterEqual>
    {
        sealed class PrefixFeature
            : DictionaryEx<int, BitsPrefixFeature>
                , ISearchPath<ISearchPath<IPrefixFeature, SequenceType>, ArrayType>
            , ISearchPath<IPrefixFeature, SequenceType>
        {
            public PrefixFeature(string operation, BitType parent) : base(count => new BitsPrefixFeature(operation, count, parent)) { }
            ISearchPath<IPrefixFeature, SequenceType> ISearchPath<ISearchPath<IPrefixFeature, SequenceType>, ArrayType>.Convert(ArrayType type) { return this; }
            IPrefixFeature ISearchPath<IPrefixFeature, SequenceType>.Convert(SequenceType type) { return this[type.Count]; }
        }

        sealed class BitsPrefixFeature : IPrefixFeature, ISimpleFeature
        {
            readonly string _operation;
            readonly int _count;
            readonly BitType _parent;
            public BitsPrefixFeature(string operation, int count, BitType parent)
            {
                _operation = operation;
                _count = count;
                _parent = parent;
            }
            IMetaFunctionFeature IFeature.MetaFunction { get { return null; } }
            IFunctionFeature IFeature.Function { get { return null; } }
            ISimpleFeature IFeature.Simple { get { return this; } }

            Result ISimpleFeature.Result(Category category)
            {
                return _parent.PrefixResult(category, _operation, _count);
            }
        }

        sealed class OperationFeature
            : DictionaryEx<int, BitsFeature>
                , ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType>
                , ISearchPath<ISuffixFeature, SequenceType>
        {
            public readonly string Operation;
            public OperationFeature(string operation, Func<int, int, int> signature, BitType parent)
                : base(count => new BitsFeature(operation, count, parent, signature)) 
            {
                Operation = operation;
            }
            public OperationFeature(string operation, BitType parent)
                : this(operation, (x, y) => 1, parent)
            {
                Operation = operation;
            }
            ISearchPath<ISuffixFeature, SequenceType> ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType>.Convert(ArrayType type) { return this; }
            ISuffixFeature ISearchPath<ISuffixFeature, SequenceType>.Convert(SequenceType type) { return this[type.Count]; }
        }

        sealed class BitsFeature : ISuffixFeature, IFunctionFeature, IContextReference
        {
            readonly string _operation;
            readonly int _count;
            readonly BitType _parent;
            readonly Func<int, int, int> _signature;
            readonly int _order = CodeArgs.NextOrder++;

            public BitsFeature(string operation, int count, BitType parent, Func<int, int, int> signature)
            {
                _operation = operation;
                _count = count;
                _parent = parent;
                _signature = signature;
            }
            IMetaFunctionFeature IFeature.MetaFunction { get { return null; } }
            IFunctionFeature IFeature.Function { get { return this; } }
            ISimpleFeature IFeature.Simple { get { return null; } }
         
            bool IFunctionFeature.IsImplicit { get { return false; } }
            IContextReference IFunctionFeature.ObjectReference { get { return this; } }
            Result IFunctionFeature.ApplyResult(Category category, TypeBase argsType) { return _parent.ApplyResult(category, _operation, _signature, _count, argsType); }

            Size IContextReference.Size { get { return Root.DefaultRefAlignParam.RefSize; } }
            int IContextReference.Order { get { return _order; } }
        }

        readonly Root _rootContext;
        readonly DictionaryEx<string, OperationFeature> _compareFeatures;
        readonly IDictionary<string, OperationFeature> _operationFeatures;
        readonly DictionaryEx<string, PrefixFeature> _prefixFeatures;

        internal BitType(Root rootContext)
        {
            _compareFeatures = new DictionaryEx<string, OperationFeature>(operation => new OperationFeature(operation, this));
            _prefixFeatures = new DictionaryEx<string, PrefixFeature>(operation => new PrefixFeature(operation, this));
            _operationFeatures = new[]
            {
                new OperationFeature("Plus", BitsConst.PlusSize, this),
                new OperationFeature("Minus", BitsConst.PlusSize, this),
                new OperationFeature("Star", BitsConst.MultiplySize, this),
                new OperationFeature("Slash", BitsConst.DivideSize, this)
            }
                .ToDictionary(o => o.Operation);
            _rootContext = rootContext;
        }

        ISuffixFeature IFeaturePath<ISuffixFeature, DumpPrintToken>.Feature { get { return Extension.Feature(DumpPrintTokenResult); } }
        ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType> IFeaturePath<ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType>, DumpPrintToken>.Feature { get { return Extension.Feature<SequenceType, ArrayType>(DumpPrintTokenResult); } }


        ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType> IFeaturePath<ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType>, Less>.Feature { get { return _compareFeatures["Less"]; } }
        ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType> IFeaturePath<ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType>, LessEqual>.Feature { get { return _compareFeatures["LessEqual"]; } }
        ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType> IFeaturePath<ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType>, Greater>.Feature { get { return _compareFeatures["Greater"]; } }
        ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType> IFeaturePath<ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType>, GreaterEqual>.Feature { get { return _compareFeatures["GreaterEqual"]; } }
        ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType> IFeaturePath<ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType>, Equal>.Feature { get { return _compareFeatures["Equal"]; } }
        ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType> IFeaturePath<ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType>, NotEqual>.Feature { get { return _compareFeatures["NotEqual"]; } }

        ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType> IFeaturePath<ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType>, Minus>.Feature { get { return _operationFeatures["Minus"]; } }
        ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType> IFeaturePath<ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType>, Plus>.Feature { get { return _operationFeatures["Plus"]; } }
        ISearchPath<ISearchPath<IPrefixFeature, SequenceType>, ArrayType> IFeaturePath<ISearchPath<ISearchPath<IPrefixFeature, SequenceType>, ArrayType>, Minus>.Feature { get { return _prefixFeatures["Minus"]; } }
        ISearchPath<ISearchPath<IPrefixFeature, SequenceType>, ArrayType> IFeaturePath<ISearchPath<ISearchPath<IPrefixFeature, SequenceType>, ArrayType>, Plus>.Feature { get { return _prefixFeatures["Plus"]; } }
        ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType> IFeaturePath<ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType>, Slash>.Feature { get { return _operationFeatures["Slash"]; } }
        ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType> IFeaturePath<ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType>, Star>.Feature { get { return _operationFeatures["Star"]; } }

        [DisableDump]
        internal override bool IsDataLess { get { return false; } }
        [DisableDump]
        internal override string DumpPrintText { get { return "bit"; } }
        [DisableDump]
        internal override Root RootContext { get { return _rootContext; } }

        protected override Size GetSize() { return Size.Create(1); }

        internal Result DumpPrintTokenResult(Category category, SequenceType sequenceType, ArrayType arrayType)
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

        internal override void Search(SearchVisitor searchVisitor)
        {
            searchVisitor.Search(this, null);
            if(!searchVisitor.IsSuccessFull)
                base.Search(searchVisitor);
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
        Result ApplyResult(Category category, string operation, Func<int,int,int> signature, int objectBitCount, TypeBase argsType)
        {
            var typedCategory = category.Typed;
            var argsBitCount = argsType.SequenceLength(this);
            var resultBitCount = signature(objectBitCount, argsBitCount);
            var result = UniqueNumber(resultBitCount).Result(category, () => ApplyCode(Size.Create(resultBitCount), operation, objectBitCount, argsBitCount), CodeArgs.Arg);
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


    interface ICompareResult
    {}
}