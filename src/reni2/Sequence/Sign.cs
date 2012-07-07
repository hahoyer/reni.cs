using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;
using Reni.Code;
using Reni.Feature;
using Reni.Type;

namespace Reni.Sequence
{
    internal sealed class Sign :
        SequenceOfBitOperation,
        ISearchPath<ISearchPath<ISearchPath<IPrefixFeature, SequenceType>,Type.Array>, Bit>,
        ISequenceOfBitPrefixOperation
    {
        ISearchPath<ISearchPath<IPrefixFeature, SequenceType>, Type.Array> ISearchPath<ISearchPath<ISearchPath<IPrefixFeature, SequenceType>, Type.Array>, Bit>.Convert(Bit type) { return new OperationPrefixFeature(type, this); }

        [DisableDump]
        public string CSharpNameOfDefaultOperation { get { return Name; } }

        [DisableDump]
        string ISequenceOfBitPrefixOperation.DataFunctionName { get { return DataFunctionName; } }

        public Result SequenceOperationResult(Category category, Size objSize) { throw new NotImplementedException(); }

        protected override int ResultSize(int objSize, int argSize) { return BitsConst.PlusSize(objSize, argSize); }
    }
}