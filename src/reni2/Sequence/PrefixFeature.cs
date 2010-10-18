using System;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Feature;
using Reni.Parser.TokenClass;
using Reni.Type;

namespace Reni.Sequence
{
    internal class PrefixFeature : ReniObject, IFeature, IPrefixFeature
    {
        private readonly Type.Sequence _parent;
        private readonly ISequenceOfBitPrefixOperation _definable;

        protected internal PrefixFeature(Type.Sequence parent, ISequenceOfBitPrefixOperation definable)
        {
            _parent = parent;
            _definable = definable;
        }

        IFeature IPrefixFeature.Feature { get { return this; } }

        TypeBase IFeature.DefiningType() { return _parent; }

        Result IFeature.Apply(Category category)
        {
            return Apply(category, _parent.UnrefSize)
                .ReplaceArg(_parent.ConvertToBitSequence(category));
        }

        private Result Apply(Category category, Size objSize)
        {
            var type = TypeBase.Number(objSize.ToInt());
            return type.Result(category,
                                     () => CodeBase.BitSequenceOperation(type.Size, _definable, objSize));
        }
    }

    internal class Feature : FeatureBase
    {
        public Feature(ISequenceOfBitBinaryOperation definable)
            : base(definable)
        {
        }

        internal override TypeBase ResultType(int objSize, int argsSize)
        {
            return TypeBase.Number(Definable.ResultSize(objSize, argsSize));
        }
    }
    internal class CompareFeature : FeatureBase
    {
        public CompareFeature(ISequenceOfBitBinaryOperation definable)
            : base(definable)
        {
        }

        internal override TypeBase ResultType(int objSize, int argsSize)
        {
            return TypeBase.Bit;
        }
    }

    internal abstract class SequenceOfBitOperation :
        Defineable,
        ISearchPath<ISearchPath<IFeature, Type.Sequence>, Bit>,
        ISequenceOfBitBinaryOperation
    {
        ISearchPath<IFeature, Type.Sequence> ISearchPath<ISearchPath<IFeature, Type.Sequence>, Bit>.Convert(Bit type)
        {
            if (IsCompareOperator)
                return new CompareFeature(this);
            return new Feature(this);
        }

        [DumpExcept(true)]
        bool ISequenceOfBitBinaryOperation.IsCompareOperator { get { return IsCompareOperator; } }
        [IsDumpEnabled(false)]
        string ISequenceOfBitBinaryOperation.DataFunctionName { get { return DataFunctionName; } }
        [IsDumpEnabled(false)]
        string ISequenceOfBitBinaryOperation.CSharpNameOfDefaultOperation { get { return CSharpNameOfDefaultOperation; } }

        int ISequenceOfBitBinaryOperation.ResultSize(int objBitCount, int argBitCount) { return ResultSize(objBitCount, argBitCount); }

        protected abstract int ResultSize(int objSize, int argSize);

        [IsDumpEnabled(false)]
        protected virtual string CSharpNameOfDefaultOperation { get { return Name; } }
        [DumpExcept(true)]
        protected virtual bool IsCompareOperator { get { return false; } }
    }

    internal sealed class Sign :
        SequenceOfBitOperation,
        ISearchPath<ISearchPath<IPrefixFeature, Type.Sequence>, Bit>,
        ISequenceOfBitPrefixOperation
    {
        ISearchPath<IPrefixFeature, Type.Sequence> ISearchPath<ISearchPath<IPrefixFeature, Type.Sequence>, Bit>.Convert(Bit type) { return new SequenceOperationPrefixFeature(type, this); }

        [IsDumpEnabled(false)]
        string ISequenceOfBitPrefixOperation.CSharpNameOfDefaultOperation { get { return Name; } }
        [IsDumpEnabled(false)]
        string ISequenceOfBitPrefixOperation.DataFunctionName { get { return DataFunctionName; } }
        public Result SequenceOperationResult(Category category, Size objSize) { throw new NotImplementedException(); }

        protected override int ResultSize(int objSize, int argSize) { return BitsConst.PlusSize(objSize, argSize); }
    }

    internal sealed class Star : SequenceOfBitOperation
    {
        protected override int ResultSize(int objSize, int argSize) { return BitsConst.MultiplySize(objSize, argSize); }
    }

    internal sealed class Slash : SequenceOfBitOperation
    {
        protected override int ResultSize(int objSize, int argSize) { return BitsConst.DivideSize(objSize, argSize); }
    }
}