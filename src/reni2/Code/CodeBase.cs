using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Code.ReplaceVisitor;
using Reni.Context;
using Reni.Type;

namespace Reni.Code
{
    [Serializable]
    internal abstract class
        CodeBase : ReniObject, IIconKeyProvider
    {
        protected CodeBase(int objectId)
            : base(objectId) { }

        protected CodeBase() { }

        [Node]
        [IsDumpEnabled(false)]
        internal Size Size { get { return GetSize(); } }

        [Node, IsDumpEnabled(false)]
        internal Size MaxSize { get { return MaxSizeImplementation; } }

        [Node, IsDumpEnabled(false), SmartNode]
        internal List<IReferenceInCode> RefsData { get { return Refs.Data; } }

        [IsDumpEnabled(false)]
        internal Refs Refs { get { return RefsImplementation; } }

        [IsDumpEnabled(false)]
        internal virtual bool IsEmpty { get { return false; } }

        [IsDumpEnabled(false)]
        protected virtual bool IsRelativeReference { get { return RefAlignParam != null; } }

        [IsDumpEnabled(false)]
        internal virtual RefAlignParam RefAlignParam { get { return null; } }

        [IsDumpEnabled(false)]
        protected virtual Size MaxSizeImplementation { get { return Size; } }

        protected abstract Size GetSize();

        [IsDumpEnabled(false)]
        internal virtual Refs RefsImplementation { get { return Refs.None(); } }

        internal CodeBase CreateAssignment(RefAlignParam refAlignParam, Size size)
        {
            var alignedSize = size.ByteAlignedSize;
            return CreateFiber(new Assign(refAlignParam, alignedSize));
        }

        internal static CodeBase DumpPrintText(string dumpPrintText) { return new DumpPrintText(dumpPrintText); }

        internal CodeBase CreateThenElse(CodeBase thenCode, CodeBase elseCode) { return CreateFiber(new ThenElse(thenCode, elseCode)); }

        internal static CodeBase TopRef(RefAlignParam refAlignParam, string reason) { return new TopRef(refAlignParam, Size.Zero, reason); }

        internal CodeBase LocalReference(RefAlignParam refAlignParam, CodeBase destructorCode) { return new LocalReference(refAlignParam, this, destructorCode); }

        internal static CodeBase TopRef(RefAlignParam refAlignParam, Size offset, string reason) { return new TopRef(refAlignParam, offset, reason); }

        internal static CodeBase FrameRef(RefAlignParam refAlignParam, string reason) { return new FrameRef(refAlignParam, Size.Create(0), reason); }

        internal abstract CodeBase CreateFiber(FiberItem subsequentElement);

        internal CodeBase AddToReference(RefAlignParam refAlignParam, Size right, string reason) { return CreateFiber(new RefPlus(refAlignParam, right, reason)); }

        internal CodeBase Dereference(RefAlignParam refAlignParam, Size targetSize)
        {
            Tracer.Assert(Size == refAlignParam.RefSize);
            return CreateFiber(new Dereference(refAlignParam, targetSize, targetSize));
        }

        internal CodeBase CreateBitCast(Size size)
        {
            if(Size == size)
                return this;
            return CreateFiber(new BitCast(size, Size, Size));
        }

        internal virtual CodeBase Sequence(params CodeBase[] data)
        {
            var resultData = new List<CodeBase>();
            resultData.AddRange(AsList());
            foreach(var codeBase in data)
                resultData.AddRange(codeBase.AsList());
            switch(resultData.Count)
            {
                case 0:
                    return this;
                case 1:
                    return resultData[0];
            }
            return new List(resultData);
        }

        protected virtual IEnumerable<CodeBase> AsList() { return new[]{this}; }

        internal static CodeBase BitsConst(Size size, BitsConst t) { return new BitArray(size, t); }
        internal static CodeBase BitsConst(BitsConst t) { return BitsConst(t.Size, t); }
        internal static CodeBase Void() { return BitArray.Void(); }
        internal static CodeBase Arg(Size size) { return new Arg(size); }
        internal static CodeBase ReferenceInCode(IReferenceInCode reference) { return new ReferenceCode(reference); }

        internal CodeBase ReplaceArg(CodeBase argCode)
        {
            try
            {
                var result = argCode.IsRelativeReference
                                 ? Visit(new ReplaceRelRefArg(argCode))
                                 : Visit(new ReplaceAbsoluteArg(argCode));
                return result ?? this;
            }
            catch(ReplaceArg.SizeException sizeException)
            {
                DumpWithBreak(true, "this", this, "sizeException", sizeException);
                throw;
            }
        }

        [IsDumpEnabled(false)]
        internal bool HasArg { get { return Visit(new HasArgVisitor()); } }

        /// <summary>
        /// Replaces appearences of context in code tree.                                                               
        /// Assumes, that replacement requires offset alignment when walking along code tree
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="replacement">The replacement.</param>
        /// <returns></returns>
        internal CodeBase ReplaceRelative<TContext>(TContext context, Func<CodeBase> replacement)
            where TContext : IReferenceInCode
        {
            var result = Visit(new ReplaceRelativeContextRef<TContext>(context, replacement));
            if(result != null)
                return result;
            return this;
        }

        /// <summary>
        /// Replaces appearences of context in code tree. 
        /// Assumes, that replacement isn't a reference, that changes when walking along the code tree
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="replacement">The replacement.</param>
        /// <returns></returns>
        internal CodeBase ReplaceAbsolute<TContext>(TContext context, Func<CodeBase> replacement)
            where TContext : IReferenceInCode
        {
            var result = Visit(new ReplaceAbsoluteContextRef<TContext>(context, replacement));
            if(result != null)
                return result;
            return this;
        }

        internal TResult Visit<TResult>(Visitor<TResult> actual) { return VisitImplementation(actual); }

        protected virtual TResult VisitImplementation<TResult>(Visitor<TResult> actual)
        {
            NotImplementedMethod(actual);
            throw new NotImplementedException();
        }

        internal CodeBase CreateCall(int index, Size resultSize) { return CreateFiber(new Call(index, resultSize, Size)); }

        internal static FiberItem CreateRecursiveCall(Size refsSize) { return new RecursiveCallCandidate(refsSize); }

        internal CodeBase TryReplacePrimitiveRecursivity(int functionIndex)
        {
            if(!Size.IsZero)
                return this;

            var newResult = Visit(new ReplacePrimitiveRecursivity(functionIndex));
            return newResult ?? this;
        }

        internal virtual BitsConst Evaluate()
        {
            NotImplementedMethod();
            return null;
        }

        internal CodeBase Align(int alignBits = Reni.BitsConst.SegmentAlignBits) { return CreateBitCast(Size.NextPacketSize(alignBits)); }

        /// <summary>
        /// Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        string IIconKeyProvider.IconKey { get { return "Code"; } }

        [IsDumpEnabled(false)]
        public override string NodeDump { get { return base.NodeDump + " Size=" + Size; } }

        internal CodeBase LocalBlock(CodeBase copier, RefAlignParam refAlignParam) { return new LocalReferenceSequenceVisitor().LocalBlock(this, copier, refAlignParam); }

        internal CodeBase CreateLocalBlockEnd(CodeBase copier, RefAlignParam refAlignParam, Size resultSize)
        {
            var intermediateSize = Size - resultSize;
            if(intermediateSize.IsZero)
                return this;

            var result = this;
            if(!resultSize.IsZero)
                result = result.CreateFiber(new LocalBlockEnd(resultSize, intermediateSize))
                    .Sequence
                    (
                        copier.ReplaceArg(LocalReferenceCode(refAlignParam, resultSize, "CreateLocalBlockEnd")));

            return result.CreateFiber(new Drop(Size, resultSize));
        }

        internal static CodeBase BitSequenceOperation
            (Size size, ISequenceOfBitPrefixOperation feature, Size objSize)
        {
            return TypeBase.Bit
                .Sequence((objSize.ByteAlignedSize).ToInt())
                .ArgCode()
                .BitSequenceOperation(feature, size);
        }

        internal static CodeBase BitSequenceOperation
            (Size size, ISequenceOfBitBinaryOperation token, int objBits, int argsBits)
        {
            var objSize = Size.Create(objBits);
            var argsSize = Size.Create(argsBits);
            return TypeBase.Bit.Sequence((objSize.ByteAlignedSize + argsSize.ByteAlignedSize).ToInt())
                .ArgCode()
                .BitSequenceOperation(token, size, objSize.ByteAlignedSize);
        }

        internal static CodeBase BitSequenceDumpPrint(int objSize)
        {
            var alignedSize = Size.Create(objSize).ByteAlignedSize;
            return TypeBase.Bit.Sequence(alignedSize.ToInt())
                .ArgCode()
                .DumpPrint(alignedSize);
        }

        internal CodeBase SequenceOfTwo(CodeBase right)
        {
            if(IsEmpty)
                return right;
            if(right.IsEmpty)
                return this;
            return List.Create(this, right);
        }

        private CodeBase BitSequenceOperation(ISequenceOfBitBinaryOperation name, Size size, Size leftSize) { return CreateFiber(new BitArrayBinaryOp(name, size, leftSize, Size - leftSize)); }
        private CodeBase DumpPrint(Size leftSize) { return CreateFiber(new DumpPrintOperation(leftSize, Size - leftSize)); }
        private CodeBase BitSequenceOperation(ISequenceOfBitPrefixOperation feature, Size size) { return CreateFiber(new BitArrayPrefixOp(feature, size, Size)); }

        internal static CodeBase LocalReferenceCode(RefAlignParam refAlignParam, Size size, string reason)
        {
            return Arg(refAlignParam.RefSize)
                .AddToReference(refAlignParam, size*(-1), reason);
        }

        internal CodeBase CreateFiber(IEnumerable<FiberItem> subsequentElement)
        {
            return subsequentElement
                .Aggregate(this, (current, fiberItem) => current.CreateFiber(fiberItem));
        }
    }

    internal abstract class UnexpectedVisitOfPending : Exception
    {
    }

    internal static class CodeBaseExtender
    {
        internal static CodeBase ToSequence(this IEnumerable<CodeBase> x) { return x.Aggregate(CodeBase.Void(), (code, result) => code.Sequence(result)); }
    }
}