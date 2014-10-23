using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
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
        internal override bool Hllw { get { return false; } }

        [DisableDump]
        internal override string DumpPrintText { get { return "bit"; } }

        [DisableDump]
        internal override Root RootContext { get { return _rootContext; } }

        protected override Size GetSize() { return Size.Create(1); }

        IPath<IPath<IFeatureImplementation, SequenceType>, ArrayType> ISymbolProvider<DumpPrintToken, IPath<IPath<IFeatureImplementation, SequenceType>, ArrayType>>.Feature(DumpPrintToken tokenClass)
        {
            var feature = Extension
                .Feature<SequenceType, ArrayType>(DumpPrintTokenResult);
            return feature;
        }

        Result DumpPrintTokenResult(Category category, SequenceType sequenceType, ArrayType arrayType)
        {
            Tracer.Assert(sequenceType.Parent == arrayType);
            Tracer.Assert(arrayType.ElementType == this);
            return VoidType
                .Result(category, sequenceType.DumpPrintNumberCode, CodeArgs.Arg);
        }

        protected override string Dump(bool isRecursion) { return GetType().PrettyName(); }

        protected override string GetNodeDump() { return "bit"; }
        internal NumberType UniqueNumber(int bitCount) { return UniqueArray(bitCount).UniqueNumber; }
        internal Result Result(Category category, BitsConst bitsConst)
        {
            return UniqueNumber(bitsConst.Size.ToInt())
                .Result(category, () => CodeBase.BitsConst(bitsConst));
        }

        internal interface IPrefix
        {
            [DisableDump]
            string Name { get; }
        }
    }
}