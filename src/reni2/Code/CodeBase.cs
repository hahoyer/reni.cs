using System;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Code.ReplaceVisitor;
using Reni.Context;
using Reni.Parser.TokenClass;

namespace Reni.Code
{
    [Serializable]
    internal abstract class CodeBase : ReniObject, IIconKeyProvider, Sequence<CodeBase>.ICombiner<CodeBase>
    {
        [Node]
        internal Size Size { get { return SizeImplementation; } }

        [Node, DumpData(false)]
        internal Size MaxSize { get { return MaxSizeImplementation; } }

        [Node, DumpData(false), SmartNode]
        internal List<IRefInCode> RefsData { get { return Refs.Data; } }

        [DumpData(false)]
        internal Refs Refs { get { return RefsImplementation; } }

        [DumpData(false)]
        internal virtual bool IsEmpty { get { return false; } }

        [DumpExcept(false)]
        internal bool IsRelativeReference { get { return RefAlignParam != null; } }

        [DumpData(false)]
        internal virtual RefAlignParam RefAlignParam { get { return null; } }

        [Node, DumpData(false)]
        internal List<LeafElement> Serial { get { return Serialize(true).Data; } }

        protected virtual Size MaxSizeImplementation { get { return Size; } }

        protected virtual Size SizeImplementation
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        internal virtual Refs RefsImplementation { get { return Refs.None(); } }

        internal CodeBase CreateBitSequenceOperation(Defineable name, Size size, Size leftSize) { return CreateChild(new BitArrayOp(name, size, leftSize, Size - leftSize)); }

        private CodeBase CreateDumpPrint(Size leftSize) { return CreateChild(new DumpPrint(leftSize, Size - leftSize)); }

        public CodeBase CreateAssignment(RefAlignParam refAlignParam, Size size)
        {
            var alignedSize = size.ByteAlignedSize;
            return CreateChild(new Assign(refAlignParam, alignedSize));
        }

        public CodeBase CreateBitSequenceOperation(Defineable name, Size size) { return CreateChild(new BitArrayPrefixOp(name, size, Size)); }

        public static CodeBase CreateDumpPrintText(string dumpPrintText) { return CreateLeaf(new DumpPrintText(dumpPrintText)); }

        public CodeBase CreateDumpPrint()
        {
            var alignedSize = Size.ByteAlignedSize;

            return CreateBitCast(alignedSize)
                .CreateDumpPrint(alignedSize);
        }

        public CodeBase CreateThenElse(CodeBase thenCode, CodeBase elseCode) { return new ThenElse(this, thenCode, elseCode); }

        internal static CodeBase CreateTopRef(RefAlignParam refAlignParam) { return CreateLeaf(new TopRef(refAlignParam, Size.Zero)); }

        internal static CodeBase CreateInternalRef(RefAlignParam refAlignParam, CodeBase code, CodeBase destructorCode) { return new InternalRef(refAlignParam, code, destructorCode); }

        internal static CodeBase CreateTopRef(RefAlignParam refAlignParam, Size offset) { return CreateLeaf(new TopRef(refAlignParam, offset)); }

        internal static CodeBase CreateFrameRef(RefAlignParam refAlignParam) { return CreateLeaf(new FrameRef(refAlignParam, Size.Create(0))); }

        private static CodeBase CreateLeaf(LeafElement leafElement) { return new Leaf(leafElement); }

        internal virtual CodeBase CreateChild(LeafElement leafElement) { return new Child(this, leafElement); }

        public CodeBase CreateChildren(LeafElement[] leafElements)
        {
            var result = this;
            for(var i = 0; i < leafElements.Length; i++)
                result = result.CreateChild(leafElements[i]);
            return result;
        }

        public Container Serialize(Size frameSize, string description, bool isInternal)
        {
            var container = new Container(MaxSize, frameSize, description, isInternal);
            Visit(container);
            return container;
        }

        public CodeBase CreateRefPlus(RefAlignParam refAlignParam, Size right) { return CreateChild(new RefPlus(refAlignParam, right)); }

        public CodeBase CreateDereference(RefAlignParam refAlignParam, Size targetSize) { return CreateChild(new Dereference(refAlignParam, targetSize)); }

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

        public CodeBase UseWithArg(CodeBase argCode)
        {
            var result = argCode.IsRelativeReference
                             ? Visit(new ReplaceRelRefArg(argCode, argCode.RefAlignParam))
                             : Visit(new ReplaceAbsoluteArg(argCode));
            return result ?? this;
        }

        /// <summary>
        /// Replaces appearences of context in code tree. 
        /// Assumes, that replacement requires offset alignment when walking along code tree
        /// </summary>
        /// <typeparam name="C"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="replacement">The replacement.</param>
        /// <returns></returns>
        public CodeBase ReplaceRelativeContextRef<C>(C context, CodeBase replacement) where C : IRefInCode
        {
            var result = Visit(new ReplaceRelativeContextRef<C>(context, replacement));
            if(result != null)
                return result;
            return this;
        }

        /// <summary>
        /// Replaces appearences of context in code tree. 
        /// Assumes, that replacement isn't a reference, that changes when walking along the code tree
        /// </summary>
        /// <typeparam name="C"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="replacement">The replacement.</param>
        /// <returns></returns>
        public CodeBase ReplaceAbsoluteContextRef<C>(C context, CodeBase replacement) where C : IRefInCode
        {
            var result = Visit(new ReplaceAbsoluteContextRef<C>(context, replacement));
            if(result != null)
                return result;
            return this;
        }

        public Result Visit<Result>(Visitor<Result> actual) { return VisitImplementation(actual); }

        public virtual Result VisitImplementation<Result>(Visitor<Result> actual)
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

        internal CodeBase Align() { return CreateBitCast(Size.ByteAlignedSize); }

        /// <summary>
        /// Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        string IIconKeyProvider.IconKey { get { return "Code"; } }

        internal CodeBase CreateStatement(CodeBase copier, RefAlignParam refAlignParam) { return new InternalRefSequenceVisitor().CreateStatement(this, copier, refAlignParam); }

        internal CodeBase CreateStatementEnd(CodeBase copier, RefAlignParam refAlignParam, Size resultSize)
        {
            var intermediateSize = Size - resultSize;
            if(intermediateSize.IsZero)
                return this;

            var result = this;
            if(!resultSize.IsZero)
            {
                result = result
                    .CreateChild(new StatementEnd(resultSize, intermediateSize))
                    .CreateSequence(
                    copier.UseWithArg(InternalRefSequenceVisitor.InternalRefCode(refAlignParam, resultSize)));
            }

            return result.CreateChild(new Drop(Size, resultSize));
        }
    }

    internal class InternalRefSequenceVisitor : Base
    {
        private readonly SimpleCache<CodeBase> _codeCache = new SimpleCache<CodeBase>();

        [Node, DumpData(true)]
        private List<InternalRef> _data = new List<InternalRef>();

        [DumpData(false)]
        public CodeBase Code
        {
            get
            {
                return
                    _codeCache.Find(
                        () => HWString.Sequence<InternalRef>(_data).Apply1(x => x.Code).Serialize(CodeBase.CreateVoid()));
            }
        }

        public CodeBase DestructorCode
        {
            get
            {
                var size = Size.Zero;
                return HWString.Sequence<InternalRef>(_data).Apply1
                    (
                    delegate(InternalRef x)
                    {
                        size += x.Code.Size;
                        return x.DestructorCode.UseWithArg(InternalRefCode(x.RefAlignParam, size));
                    }
                    )
                    .Serialize(CodeBase.CreateVoid());
            }
        }

        internal override CodeBase InternalRef(InternalRef visitedObject)
        {
            var newVisitedObject = visitedObject;
            var newCode = visitedObject.Code.Visit(this);
            if(newCode != null)
                newVisitedObject = new InternalRef(visitedObject.RefAlignParam, newCode, visitedObject.DestructorCode);
            var offset = Find(newVisitedObject);
            _codeCache.Value = null;
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

        internal static CodeBase InternalRefCode(RefAlignParam refAlignParam, Size size) { return new Arg(refAlignParam.RefSize).CreateRefPlus(refAlignParam, size*(-1)); }

        internal CodeBase CreateStatement(CodeBase body, CodeBase copier, RefAlignParam refAlignParam)
        {
            var trace = body.ObjectId == -1658;
            StartMethodDumpWithBreak(trace, body, copier, refAlignParam);
            var newBody = body.Visit(this);
            if(newBody == null)
                newBody = body;
            var alignedBody = newBody.Align();
            var resultSize = alignedBody.Size;
            var alignedInternal = Code.Align();
            // Gap is used to avoid overlapping of internal and final location of result, so Copy/Destruction can be used to move result.
            var gap = CodeBase.CreateVoid();
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

    internal class InternalRefCode : CodeBase
    {
        private readonly RefAlignParam _refAlignParam;

        public InternalRefCode(RefAlignParam refAlignParam) { _refAlignParam = refAlignParam; }

        protected override Size SizeImplementation { get { return _refAlignParam.RefSize; } }
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

    internal class UnexpectedVisitOfPending : Exception {}
}