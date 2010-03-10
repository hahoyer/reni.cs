using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
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
        [DumpData(false)]
        internal Size Size { get { return SizeImplementation; } }

        [Node, DumpData(false)]
        internal Size MaxSize { get { return MaxSizeImplementation; } }

        [Node, DumpData(false), SmartNode]
        internal List<IRefInCode> RefsData { get { return Refs.Data; } }

        [DumpData(false)]
        internal Refs Refs { get { return RefsImplementation; } }

        [DumpData(false)]
        internal virtual bool IsEmpty { get { return false; } }

        [DumpData(false)]
        private bool IsRelativeReference { get { return RefAlignParam != null; } }

        [DumpData(false)]
        internal virtual RefAlignParam RefAlignParam { get { return null; } }

        [Node, DumpData(false), UsedImplicitly]
        internal List<LeafElement> Serial { get { return Serialize(true).Data; } }

        [DumpData(false)]
        protected virtual Size MaxSizeImplementation { get { return Size; } }

        [DumpData(false)]
        protected virtual Size SizeImplementation
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        [DumpData(false)]
        internal virtual Refs RefsImplementation { get { return Refs.None(); } }

        private CodeBase CreateBitSequenceOperation(ISequenceOfBitBinaryOperation name, Size size, Size leftSize) { return CreateChild(new BitArrayBinaryOp(name, size, leftSize, Size - leftSize)); }

        private CodeBase CreateDumpPrint(Size leftSize) { return CreateChild(new DumpPrintOperation(leftSize, Size - leftSize)); }

        public CodeBase CreateAssignment(RefAlignParam refAlignParam, Size size)
        {
            var alignedSize = size.ByteAlignedSize;
            return CreateChild(new Assign(refAlignParam, alignedSize));
        }

        private CodeBase CreateBitSequenceOperation(ISequenceOfBitPrefixOperation feature, Size size) { return CreateChild(new BitArrayPrefixOp(feature, size, Size)); }

        public static CodeBase CreateDumpPrintText(string dumpPrintText) { return CreateLeaf(new DumpPrintText(dumpPrintText)); }

        public CodeBase CreateThenElse(CodeBase thenCode, CodeBase elseCode) { return new ThenElse(this, thenCode, elseCode); }

        internal static CodeBase CreateTopRef(RefAlignParam refAlignParam) { return CreateLeaf(new TopRef(refAlignParam, Size.Zero)); }

        internal static CodeBase CreateInternalRef(RefAlignParam refAlignParam, CodeBase code, CodeBase destructorCode) { return new InternalRef(refAlignParam, code, destructorCode); }

        internal static CodeBase CreateTopRef(RefAlignParam refAlignParam, Size offset) { return CreateLeaf(new TopRef(refAlignParam, offset)); }

        internal static CodeBase CreateFrameRef(RefAlignParam refAlignParam) { return CreateLeaf(new FrameRef(refAlignParam, Size.Create(0))); }

        private static CodeBase CreateLeaf(LeafElement leafElement) { return new Leaf(leafElement); }

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

        public CodeBase CreateRefPlus(RefAlignParam refAlignParam, Size right) { return CreateChild(new RefPlus(refAlignParam, right)); }

        public CodeBase CreateDereference(RefAlignParam refAlignParam, Size targetSize)
        {
            Tracer.Assert(Size == refAlignParam.RefSize);
            return CreateChild(new Dereference(refAlignParam, targetSize));
        }

        public CodeBase CreateBitCast(Size size)
        {
            if(Size == size)
                return this;
            return CreateChild(new BitCast(size, Size, Size));
        }

        public CodeBase CreateSequence(CodeBase right)
        {
            if(IsEmpty)
                return right;
            if(right.IsEmpty)
                return this;
            return new Pair(this, right);
        }                                                                                                                                                  

        public static CodeBase CreateBitArray(Size size, BitsConst t) { return CreateLeaf(new BitArray(size, t)); }

        public static CodeBase CreateBitArray(BitsConst t) { return CreateBitArray(t.Size, t); }

        public static CodeBase CreateVoid() { return CreateLeaf(BitArray.CreateVoid()); }

        public static CodeBase CreateArg(Size size) { return new Arg(size); }

        public static CodeBase CreateContextRef(IRefInCode context) { return new RefCode(context); }

        internal CodeBase UseWithArg(CodeBase argCode)
        {
            var result = argCode.IsRelativeReference
                             ? Visit(new ReplaceRelRefArg(argCode))
                             : Visit(new ReplaceAbsoluteArg(argCode));
            return result ?? this;
        }


        [DumpData(false)]
        internal bool HasArg { get { return Visit(new HasArgVisitor()); } }

        /// <summary>
        /// Replaces appearences of context in code tree. 
        /// Assumes, that replacement requires offset alignment when walking along code tree
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="replacement">The replacement.</param>
        /// <returns></returns>
        public CodeBase ReplaceRelativeContextRef<TContext>(TContext context, Func<CodeBase> replacement) where TContext : IRefInCode
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
        public CodeBase ReplaceAbsoluteContextRef<TContext>(TContext context, Func<CodeBase> replacement) where TContext : IRefInCode
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

        internal CodeBase Align(int alignBits) { return CreateBitCast(Size.NextPacketSize(alignBits)); }
        internal CodeBase Align() { return Align(BitsConst.SegmentAlignBits); }

        /// <summary>
        /// Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        string IIconKeyProvider.IconKey { get { return "Code"; } }

        public override string NodeDump { get { return base.NodeDump + " Size=" + Size; } }

        internal CodeBase CreateStatement(CodeBase copier, RefAlignParam refAlignParam)
        {
            return new InternalRefSequenceVisitor().CreateStatement(this, copier, refAlignParam);
        }

        internal CodeBase CreateStatementEnd(CodeBase copier, RefAlignParam refAlignParam, Size resultSize)
        {
            var intermediateSize = Size - resultSize;
            if(intermediateSize.IsZero)
                return this;

            var result = this;
            if(!resultSize.IsZero)
                result = result
                    .CreateChild(new StatementEnd(resultSize, intermediateSize))
                    .CreateSequence(
                    copier.UseWithArg(InternalRefSequenceVisitor.InternalRefCode(refAlignParam, resultSize)));

            return result.CreateChild(new Drop(Size, resultSize));
        }

        internal static CodeBase CreateBitSequenceOperation(Size size, ISequenceOfBitPrefixOperation feature, Size objSize)
        {
            return TypeBase.CreateBit
                .CreateSequence((objSize.ByteAlignedSize).ToInt())
                .CreateArgCode()
                .CreateBitSequenceOperation(feature, size);
        }

        internal static CodeBase CreateBitSequenceOperation(Size size, ISequenceOfBitBinaryOperation token, int objBits, int argsBits)
        {
            var objSize = Size.Create(objBits);
            var argsSize = Size.Create(argsBits);
            return TypeBase.CreateBit.CreateSequence((objSize.ByteAlignedSize + argsSize.ByteAlignedSize).ToInt())
                .CreateArgCode()
                .CreateBitSequenceOperation(token, size, objSize.ByteAlignedSize);
        }

        internal static CodeBase CreateBitSequenceDumpPrint(int objSize)
        {
            var alignedSize = Size.Create(objSize).ByteAlignedSize;
            return TypeBase.CreateBit.CreateSequence(alignedSize.ToInt())
                .CreateArgCode()
                .CreateDumpPrint(alignedSize);
        }
    }

    internal class InternalRefSequenceVisitor : Base
    {
        private readonly SimpleCache<CodeBase> _codeCache;

        [Node, DumpData(true)]
        private readonly List<InternalRef> _data = new List<InternalRef>();

        public InternalRefSequenceVisitor() { _codeCache = new SimpleCache<CodeBase>(Convert); }

        private CodeBase Convert()
        {
            return _data
                .Select(x => x.Code)
                .Aggregate(CodeBase.CreateVoid(), (a, b) => a.CreateSequence(b));
        }

        [DumpData(false)]
        private CodeBase Code { get { return _codeCache.Value; } }

        private CodeBase DestructorCode
        {
            get
            {
                var size = Size.Zero;
                return
                    _data
                        .Select(x => x.AccompayningDestructorCode(ref size))
                        .Aggregate(CodeBase.CreateVoid(), (a, b) => a.CreateSequence(b));
            }
        }

        internal override CodeBase InternalRef(InternalRef visitedObject)
        {
            var newVisitedObject = ReVisit(visitedObject) ?? visitedObject;
            var offset = Find(newVisitedObject);
            _codeCache.Reset();
            return InternalRefCode(newVisitedObject.RefAlignParam, offset);
        }

        private Size Find(InternalRef internalRef)
        {
            var result = Size.Zero;
            var i = 0;
            for(; i < _data.Count && _data[i] != internalRef; i++)
                result += _data[i].Code.Size;
            if(i == _data.Count)
                _data.Add(internalRef);
            return result + internalRef.Code.Size;
        }

        internal static CodeBase InternalRefCode(RefAlignParam refAlignParam, Size size)
        {
            return CodeBase.CreateArg(refAlignParam.RefSize).CreateRefPlus(refAlignParam, size*(-1));
        }

        internal CodeBase CreateStatement(CodeBase body, CodeBase copier, RefAlignParam refAlignParam)
        {
            Tracer.Assert(!body.HasArg);
            var trace = body.ObjectId == -268;
            StartMethodDumpWithBreak(trace, body, copier, refAlignParam);
            var newBody = body.Visit(this) ?? body;
            var alignedBody = newBody.Align();
            var resultSize = alignedBody.Size;
            var alignedInternal = Code.Align();
            // Gap is used to avoid overlapping of internal and final location of result, so Copy/Destruction can be used to move result.
            var gap = CodeBase .CreateVoid();
            if(!copier.IsEmpty && alignedInternal.Size > Size.Zero && alignedInternal.Size < resultSize)
                gap = CodeBase.CreateBitArray(resultSize - alignedInternal.Size, BitsConst.None());
            var result = alignedInternal
                .CreateSequence(gap)
                .CreateSequence(alignedBody)
                .CreateSequence(DestructorCode)
                .CreateStatementEnd(copier, refAlignParam, resultSize)
                .UseWithArg(CodeBase.CreateTopRef(refAlignParam));
            return ReturnMethodDump(trace, result);
        }
    }

    internal class Drop : LeafElement
    {
        private readonly Size _beforeSize;
        private readonly Size _afterSize;

        public Drop(Size beforeSize, Size afterSize)
        {
            _beforeSize = beforeSize;
            _afterSize = afterSize;
        }

        public override string NodeDump { get { return base.NodeDump + " BeforeSize=" + _beforeSize + " AfterSize=" + _afterSize; } }

        protected override Size GetSize() { return _afterSize; }

        protected override Size GetInputSize() { return _beforeSize; }

        protected override string Format(StorageDescriptor start) { return ""; }
    }

    internal interface IRefInCode
    {
        Size RefSize { get; }
        RefAlignParam RefAlignParam { get; }
        bool IsChildOf(ContextBase contextBase);
        string Dump();
    }

    [Serializable]
    internal class Assign : LeafElement
    {
        [DumpData(true)]
        private readonly RefAlignParam _refAlignParam;

        [DumpData(true), Node]
        private readonly Size _targetSize;

        public Assign(RefAlignParam refAlignParam, Size targetSize)
        {
            _refAlignParam = refAlignParam;
            _targetSize = targetSize;
        }

        protected override Size GetSize() { return Size.Zero; }

        protected override Size GetInputSize() { return _refAlignParam.RefSize*2; }

        protected override string Format(StorageDescriptor start) { return start.CreateAssignment(_refAlignParam, _targetSize); }
    }

    internal abstract class UnexpectedVisitOfPending : Exception
    {
    }
}