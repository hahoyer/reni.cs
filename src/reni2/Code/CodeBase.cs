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
        internal Size Size { get { return GetSize(); } }

        [Node, DumpData(false)]
        internal Size MaxSize { get { return GetMaxSize(); } }

        [Node, DumpData(false), SmartNode]
        internal List<IRefInCode> Refs { get { return GetRefs().Data; } }

        [DumpData(false)]
        internal virtual bool IsEmpty { get { return false; } }

        [DumpExcept(false)]
        internal bool IsRelativeReference { get { return RefAlignParam != null; } }

        [DumpData(false)]
        internal virtual RefAlignParam RefAlignParam { get { return null; } }

        [DumpExcept(false)]
        internal virtual bool IsPending { get { return false; } }

        [Node, DumpData(false)]
        internal List<LeafElement> Serial { get { return Serialize(true).Data; } }

        internal static CodeBase Pending { get { return new Pending(); } }

        internal protected virtual Size GetMaxSize() { return Size; }

        internal protected virtual Size GetSize()
        {
            NotImplementedMethod();
            return null;
        }

        internal virtual Refs GetRefs() { return Reni.Refs.None(); }

        internal CodeBase CreateBitSequenceOperation(Defineable name, Size size, Size leftSize) { return CreateChild(new BitArrayOp(name, size, leftSize, Size - leftSize)); }

        public CodeBase CreateDumpPrint(Size leftSize) { return CreateChild(new DumpPrint(leftSize, Size - leftSize)); }

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

        public static CodeBase CreateTopRef(RefAlignParam refAlignParam) { return CreateLeaf(new TopRef(refAlignParam, Size.Zero)); }

        internal static CodeBase CreateInternalRef(RefAlignParam refAlignParam, CodeBase code, CodeBase destructorCode) { return new InternalRef(refAlignParam, code, destructorCode); }

        public static CodeBase CreateTopRef(RefAlignParam refAlignParam, Size offset) { return CreateLeaf(new TopRef(refAlignParam, offset)); }

        public static CodeBase CreateFrameRef(RefAlignParam refAlignParam) { return CreateLeaf(new FrameRef(refAlignParam, Size.Create(0))); }

        private static CodeBase CreateLeaf(LeafElement leafElement) { return new Leaf(leafElement); }

        public virtual CodeBase CreateChild(LeafElement leafElement) { return new Child(this, leafElement); }

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

        public CodeBase ReplaceRelativeContextRef<C>(C context, CodeBase replacement) where C : IRefInCode
        {
            var result = Visit(new ReplaceRelativeContextRef<C>(context, replacement));
            if(result != null)
                return result;
            return this;
        }

        public CodeBase ReplaceAbsoluteContextRef<C>(C context, CodeBase replacement) where C : IRefInCode
        {
            var result = Visit(new ReplaceAbsoluteContextRef<C>(context, replacement));
            if(result != null)
                return result;
            return this;
        }

        public Result Visit<Result>(Visitor<Result> actual) { return VirtVisit(actual); }

        public virtual Result VirtVisit<Result>(Visitor<Result> actual)
        {
            NotImplementedMethod(actual);
            throw new NotImplementedException();
        }

        public CodeBase CreateStatementEndFromIntermediateStorage(CodeBase finalResult, CodeBase destructor,
                                                                  CodeBase mover)
        {
            if(destructor.IsEmpty && mover.IsEmpty)
            {
                if(Size.IsZero) // No temp storage 
                    return finalResult; // Just return final result

                var alignedThis = CreateBitCast(Size.ByteAlignedSize);
                var sequencedResult = alignedThis.CreateSequence(finalResult);
                var result = sequencedResult.CreateChild(new StatementEnd(finalResult.Size, Size.ByteAlignedSize));
                return result;
            }
            NotImplementedMethod(finalResult, destructor, mover);
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

        internal virtual CodeBase CreateStatement()
        {
            var internalRefSequenceVisitor = new InternalRefSequenceVisitor();
            var newCode = Visit(internalRefSequenceVisitor);
            if(newCode == null)
                newCode = this;
            // Gap is used to avoid overlapping of internal and final location of result, so Copy/Destruction can be used to move result.
            var gap = CreateVoid();
            if (internalRefSequenceVisitor.Code.Size > Size.Zero && internalRefSequenceVisitor.Code.Size < Size)
                gap = CreateBitArray(Size - internalRefSequenceVisitor.Code.Size, BitsConst.None());
            var result = internalRefSequenceVisitor.Code
                .CreateSequence(gap)
                .CreateSequence(newCode)
                .CreateSequence(internalRefSequenceVisitor.DestructorCode)
                .UseWithArg(CreateTopRef(internalRefSequenceVisitor.RefAligmParam));

            NotImplementedMethod("result", result);
            return this;
        }
    }

    internal class InternalRefSequenceVisitor : Base
    {
        private readonly SimpleCache<CodeBase> _codeCache = new SimpleCache<CodeBase>();
        [Node, DumpData(true)]
        private Sequence<InternalRef> _data = HWString.Sequence<InternalRef>();

        [DumpData(false)]
        public CodeBase Code { get { return _codeCache.Find(()=>_data.Apply1(x => x.Code).Serialize(CodeBase.CreateVoid())); } }

        public CodeBase DestructorCode
        {
            get
            {
                var size = Size.Zero;
                return _data.Apply1
                (
                    delegate(InternalRef x)
                    {
                        size += x.Code.Size;
                        return x.DestructorCode.UseWithArg(InternalRefCode(x.RefAlignParam, size));
                    }
                ).Serialize(CodeBase.CreateVoid());
            }
        }

        public RefAlignParam RefAligmParam { get { return _data.ToArray()[0].RefAlignParam; } }

        internal override CodeBase InternalRef(InternalRef visitedObject)
        {
            var newVisitedObject = visitedObject;
            var newCode = visitedObject.Code.Visit(this);
            if(newCode != null)
                newVisitedObject = new InternalRef(visitedObject.RefAlignParam, newCode, visitedObject.DestructorCode);
            _data += newVisitedObject;
            _codeCache.Value = null;
            return InternalRefCode(newVisitedObject.RefAlignParam, Code.Size);
        }

        private static CodeBase InternalRefCode(RefAlignParam refAlignParam, Size size)
        {
            return new Arg(refAlignParam.RefSize).CreateRefPlus(refAlignParam, size*(-1));
        }
    }

    internal class InternalRefCode : CodeBase
    {
        private readonly RefAlignParam _refAlignParam;

        public InternalRefCode(RefAlignParam refAlignParam) { _refAlignParam = refAlignParam; }

        internal protected override Size GetSize() { return _refAlignParam.RefSize; }
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

        [DumpData(true)]
        private readonly Size _size;

        public Assign(RefAlignParam refAlignParam, Size size)
        {
            _refAlignParam = refAlignParam;
            _size = size;
        }

        protected override Size GetSize() { return Size.Zero; }
        protected override Size GetInputSize() { return _refAlignParam.RefSize*2; }
        protected override string Format(StorageDescriptor start) { return start.Assign(_refAlignParam, _size); }
    }

    internal class Pending : CodeBase, IIconKeyProvider
    {
        internal Pending() { StopByObjectId(711); }

        internal protected override Size GetSize() { return Size.Pending; }
        internal override bool IsPending { get { return true; } }

        public override Result VirtVisit<Result>(Visitor<Result> actual) { throw new UnexpectedVisitOfPending(); }

        /// <summary>
        /// Gets the icon key.
        /// </summary>
        /// <value>The icon key.</value>
        public string IconKey { get { return "Pending"; } }
    }

    internal class UnexpectedVisitOfPending : Exception {}
}