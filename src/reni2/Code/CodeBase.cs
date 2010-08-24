using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using JetBrains.Annotations;
using Reni.Code.ReplaceVisitor;
using Reni.Context;
using Reni.Type;

namespace Reni.Code
{
    [Serializable]
    internal abstract class
        CodeBase : ReniObject, IIconKeyProvider
    {
        protected CodeBase(int objectId):base(objectId) {  }
        protected CodeBase(){ }

        [Node]
        [IsDumpEnabled(false)]
        internal Size Size { get { return SizeImplementation; } }

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

        [Node, IsDumpEnabled(false), UsedImplicitly]
        internal List<LeafElement> Serial { get { return Serialize(true).Data; } }

        [IsDumpEnabled(false)]
        protected virtual Size MaxSizeImplementation { get { return Size; } }

        [IsDumpEnabled(false)]
        protected virtual Size SizeImplementation
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        [IsDumpEnabled(false)]
        internal virtual Refs RefsImplementation { get { return Refs.None(); } }

        private CodeBase CreateBitSequenceOperation(ISequenceOfBitBinaryOperation name, Size size, Size leftSize) { return CreateChild(new BitArrayBinaryOp(name, size, leftSize, Size - leftSize)); }

        private CodeBase CreateDumpPrint(Size leftSize) { return CreateChild(new DumpPrintOperation(leftSize, Size - leftSize)); }

        public CodeBase CreateAssignment(RefAlignParam refAlignParam, Size size)
        {
            var alignedSize = size.ByteAlignedSize;
            return CreateChild(new Assign(refAlignParam, alignedSize));
        }

        private CodeBase CreateBitSequenceOperation(ISequenceOfBitPrefixOperation feature, Size size) { return CreateChild(new BitArrayPrefixOp(feature, size, Size)); }

        public static CodeBase DumpPrintText(string dumpPrintText) { return Leaf(new DumpPrintText(dumpPrintText)); }

        public CodeBase CreateThenElse(CodeBase thenCode, CodeBase elseCode) { return new ThenElse(this, thenCode, elseCode); }

        internal static CodeBase CreateTopRef(RefAlignParam refAlignParam) { return Leaf(new TopRef(refAlignParam, Size.Zero)); }

        internal CodeBase LocalReference(RefAlignParam refAlignParam, CodeBase destructorCode) { return new LocalReference(refAlignParam, this, destructorCode); }

        internal static CodeBase CreateTopRef(RefAlignParam refAlignParam, Size offset) { return Leaf(new TopRef(refAlignParam, offset)); }

        internal static CodeBase CreateFrameRef(RefAlignParam refAlignParam) { return Leaf(new FrameRef(refAlignParam, Size.Create(0))); }

        private static CodeBase Leaf(StartingLeafElement leafElement) { return new Leaf(leafElement); }

        internal virtual CodeBase CreateChild(LeafElement leafElement) { return new Child(this, leafElement); }

        public CodeBase CreateChildren(IEnumerable<LeafElement> leafElements)
        {
            return leafElements
                .Aggregate(this, (current, t) => current.CreateChild(t));
        }

        public Container Serialize(Size frameSize, string description, bool isInternal)
        {
            var container = new Container(MaxSize, frameSize, description, isInternal);
            Visit(container);
            return container;
        }

        public CodeBase AddToReference(RefAlignParam refAlignParam, Size right, string reason)
        {
            return
                CreateChild(new RefPlus(refAlignParam, right, reason));
        }

        public CodeBase Dereference(RefAlignParam refAlignParam, Size targetSize)
        {
            Tracer.Assert(Size == refAlignParam.RefSize);
            return CreateChild(new Dereference(refAlignParam, targetSize,targetSize));
        }

        public CodeBase CreateBitCast(Size size)
        {
            if(Size == size)
                return this;
            return CreateChild(new BitCast(size, Size, Size));
        }

        private CodeBase SequenceOfTwo(CodeBase right)
        {
            if(IsEmpty)
                return right;
            if(right.IsEmpty)
                return this;
            return new Pair(this, right);
        }

        internal CodeBase Sequence(params CodeBase[] more)
        {
            return more.Aggregate(this, (current, t) => current.SequenceOfTwo(t));
        }

        public static CodeBase BitsConst(Size size, BitsConst t) { return Leaf(new BitArray(size, t)); }

        public static CodeBase BitsConst(BitsConst t) { return BitsConst(t.Size, t); }

        public static CodeBase Void() { return Leaf(BitArray.Void()); }

        public static CodeBase Arg(Size size) { return new Arg(size); }

        public static CodeBase ReferenceInCode(IReferenceInCode reference) { return new ReferenceCode(reference); }

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
                DumpWithBreak(true,"this",this,"sizeException",sizeException);
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
        public CodeBase ReplaceRelative<TContext>(TContext context, Func<CodeBase> replacement) where TContext : IReferenceInCode
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
        public CodeBase ReplaceAbsolute<TContext>(TContext context, Func<CodeBase> replacement) where TContext : IReferenceInCode
        {
            var result = Visit(new ReplaceAbsoluteContextRef<TContext>(context, replacement));
            if(result != null)
                return result;
            return this;
        }

        public TResult Visit<TResult>(Visitor<TResult> actual) { return VisitImplementation(actual); }

        protected virtual TResult VisitImplementation<TResult>(Visitor<TResult> actual)
        {
            NotImplementedMethod(actual);
            throw new NotImplementedException();
        }

        public CodeBase CreateCall(int index, Size resultSize) { return CreateChild(new Call(index, resultSize, Size)); }

        internal Container Serialize(bool isInternal)
        {
            try
            {
                return Serialize(Size.Create(0), "", isInternal);
            }
            catch(Container.UnexpectedContextRefInContainer e)
            {
                DumpMethodWithBreak("UnexpectedContextRefInContainer " + e.VisitedObject.Dump(), isInternal);
                throw;
            }
        }

        public static LeafElement CreateRecursiveCall(Size refsSize) { return new RecursiveCallCandidate(refsSize); }

        public CodeBase TryReplacePrimitiveRecursivity(int functionIndex)
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

        internal CodeBase CreateLocalBlock(CodeBase copier, RefAlignParam refAlignParam)
        {
            return new LocalReferenceSequenceVisitor().CreateLocalBlock(this, copier, refAlignParam);
        }

        internal CodeBase CreateLocalBlockEnd(CodeBase copier, RefAlignParam refAlignParam, Size resultSize)
        {
            var intermediateSize = Size - resultSize;
            if(intermediateSize.IsZero)
                return this;

            var result = this;
            if(!resultSize.IsZero)
                result = result
                    .CreateChild(new LocalBlockEnd(resultSize, intermediateSize))
                    .Sequence(
                    copier.ReplaceArg(LocalReferenceSequenceVisitor.LocalReferenceCode(refAlignParam, resultSize, "CreateLocalBlockEnd")));

            return result.CreateChild(new Drop(Size, resultSize));
        }

        internal static CodeBase CreateBitSequenceOperation(Size size, ISequenceOfBitPrefixOperation feature, Size objSize)
        {
            return TypeBase.Bit
                .Sequence((objSize.ByteAlignedSize).ToInt())
                .ArgCode()
                .CreateBitSequenceOperation(feature, size);
        }

        internal static CodeBase CreateBitSequenceOperation(Size size, ISequenceOfBitBinaryOperation token, int objBits, int argsBits)
        {
            var objSize = Size.Create(objBits);
            var argsSize = Size.Create(argsBits);
            return TypeBase.Bit.Sequence((objSize.ByteAlignedSize + argsSize.ByteAlignedSize).ToInt())
                .ArgCode()
                .CreateBitSequenceOperation(token, size, objSize.ByteAlignedSize);
        }

        internal static CodeBase CreateBitSequenceDumpPrint(int objSize)
        {
            var alignedSize = Size.Create(objSize).ByteAlignedSize;
            return TypeBase.Bit.Sequence(alignedSize.ToInt())
                .ArgCode()
                .CreateDumpPrint(alignedSize);
        }
    }

    internal abstract class UnexpectedVisitOfPending : Exception
    {
    }

    internal abstract class StartingLeafElement: LeafElement
    {
        protected sealed override Size GetInputSize() { return Size.Zero; }
    }

    internal static class CodeBaseExtender
    {
        internal static CodeBase ToSequence(this IEnumerable<CodeBase> x) { return x.Aggregate(CodeBase.Void(), (code, result) => code.Sequence(result)); }
    }
}