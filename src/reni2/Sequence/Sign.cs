using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Feature;
using Reni.Type;

namespace Reni.Sequence
{
    internal sealed class Sign :
        SequenceOfBitOperation,
        ISearchPath<ISearchPath<IPrefixFeature, BaseType>, Bit>,
        ISequenceOfBitPrefixOperation
    {
        ISearchPath<IPrefixFeature, BaseType> ISearchPath<ISearchPath<IPrefixFeature, BaseType>, Bit>.Convert(Bit type) { return new OperationPrefixFeature(type, this); }

        [DisableDump]
        string ISequenceOfBitPrefixOperation.CSharpNameOfDefaultOperation { get { return Name; } }

        [DisableDump]
        string ISequenceOfBitPrefixOperation.DataFunctionName { get { return DataFunctionName; } }

        public Result SequenceOperationResult(Category category, Size objSize) { throw new NotImplementedException(); }

        protected override int ResultSize(int objSize, int argSize) { return BitsConst.PlusSize(objSize, argSize); }
    }
}