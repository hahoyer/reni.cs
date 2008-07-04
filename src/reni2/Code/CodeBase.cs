using System;
using HWClassLibrary.Debug;
using Reni.Code.ReplaceVisitor;
using Reni.Context;
using Reni.Parser.TokenClass;

namespace Reni.Code
{
    internal abstract class CodeBase : ReniObject
    {
        [DumpData(false)]
        public virtual Size MaxSize
        {
            get { return Size; }
        }

        public virtual Size Size
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        [DumpData(false)]
        public virtual bool IsEmpty
        {
            get { return false; }
        }

        [DumpExcept(false)]
        public bool IsRelativeReference
        {
            get { return RefAlignParam != null; }
        }

        [DumpData(false)]
        public virtual RefAlignParam RefAlignParam
        {
            get { return null; }
        }

        public static CodeBase Pending
        {
            get { return new Pending(); }
        }

        [DumpExcept(false)]
        public virtual bool IsPending
        {
            get { return false; }
        }

        [DumpData(false)]
        public virtual Refs Refs
        {
            get { return Refs.None(); }
        }

        internal CodeBase CreateBitSequenceOperation(Defineable name, Size size, Size leftSize)
        {
            return CreateChild(new BitArrayOp(name, size, leftSize, Size - leftSize));
        }

        public CodeBase CreateDumpPrint(Size leftSize)
        {
            return CreateChild(new DumpPrint(leftSize, Size - leftSize));
        }

        public CodeBase CreateAssign(RefAlignParam refAlignParam, CodeBase source)
        {
            var alignedSize = source.Size.ByteAlignedSize;
            return CreateSequence(source.CreateBitCast(alignedSize))
                .CreateAssign(refAlignParam, alignedSize);
        }

        public CodeBase CreateBitSequenceOperation(Defineable name)
        {
            var alignedSize = Size.ByteAlignedSize;

            return CreateBitCast(alignedSize)
                .CreateChild(new BitArrayPrefixOp(name, alignedSize))
                .CreateBitCast(Size);
        }

        public static CodeBase CreateDumpPrintText(string dumpPrintText)
        {
            return CreateLeaf(new DumpPrintText(dumpPrintText));
        }

        public CodeBase CreateDumpPrint()
        {
            var alignedSize = Size.ByteAlignedSize;

            return CreateBitCast(alignedSize)
                .CreateDumpPrint(alignedSize);
        }

        public CodeBase CreateThenElse(CodeBase thenCode, CodeBase elseCode)
        {
            return new ThenElse(this, thenCode, elseCode);
        }

        public static CodeBase CreateTopRef(RefAlignParam refAlignParam)
        {
            return CreateLeaf(new TopRef(refAlignParam, Size.Zero));
        }

        public static CodeBase CreateTopRef(RefAlignParam refAlignParam, Size offset)
        {
            return CreateLeaf(new TopRef(refAlignParam, offset));
        }

        public static CodeBase CreateFrameRef(RefAlignParam refAlignParam)
        {
            return CreateLeaf(new FrameRef(refAlignParam, Size.Create(0)));
        }

        private static CodeBase CreateLeaf(LeafElement leafElement)
        {
            return new Leaf(leafElement);
        }

        public virtual CodeBase CreateChild(LeafElement leafElement)
        {
            return new Child(this, leafElement);
        }

        public CodeBase CreateChilds(LeafElement[] leafElements)
        {
            var result = this;
            for (var i = 0; i < leafElements.Length; i++)
                result = CreateChild(leafElements[i]);
            return result;
        }

        public Container Serialize(Size frameSize, string description)
        {
            var container = new Container(MaxSize, frameSize, description);
            Visit(container);
            return container;
        }

        public CodeBase CreateRefPlus(RefAlignParam refAlignParam, Size right)
        {
            return CreateChild(new RefPlus(refAlignParam, right));
        }

        public CodeBase CreateDereference(RefAlignParam refAlignParam, Size targetSize)
        {
            return CreateChild(new Dereference(refAlignParam, targetSize));
        }

        public CodeBase CreateBitCast(Size size)
        {
            if (Size == size)
                return this;
            return CreateChild(new BitCast(Size, size, Size));
        }

        public CodeBase CreateSequence(CodeBase right)
        {
            if (IsEmpty)
                return right;
            if (right.IsEmpty)
                return this;
            return new Pair(this, right);
        }

        public static CodeBase CreateBitArray(Size size, BitsConst t)
        {
            return CreateLeaf(new BitArray(size, t));
        }

        public static CodeBase CreateBitArray(BitsConst t)
        {
            return CreateBitArray(t.Size, t);
        }

        public static CodeBase CreateVoid()
        {
            return CreateLeaf(BitArray.CreateVoid());
        }

        public static CodeBase CreateArg(Size size)
        {
            return new Arg(size);
        }

        public static CodeBase CreateContextRef<C>(C context) where C : ContextBase
        {
            return new ContextRef<C>(context);
        }

        public CodeBase UseWithArg(CodeBase argCode)
        {
            var result = argCode.IsRelativeReference
                             ? Visit(new ReplaceRelRefArg(argCode, argCode.RefAlignParam))
                             : Visit(new ReplaceAbsoluteArg(argCode));
            return result ?? this;
        }

        public CodeBase ReplaceRelativeContextRef<C>(C context, CodeBase replacement) where C : ContextBase
        {
            var result = Visit(new ReplaceRelativeContextRef<C>(context, replacement));
            if (result != null)
                return result;
            return this;
        }

        public CodeBase ReplaceAbsoluteContextRef<C>(C context, CodeBase replacement) where C : ContextBase
        {
            var result = Visit(new ReplaceAbsoluteContextRef<C>(context, replacement));
            if (result != null)
                return result;
            return this;
        }

        public Result Visit<Result>(Visitor<Result> actual)
        {
            return VirtVisit(actual);
        }

        public virtual Result VirtVisit<Result>(Visitor<Result> actual)
        {
            NotImplementedMethod(actual);
            throw new NotImplementedException();
        }

        public CodeBase CreateStatementEndFromIntermediateStorage(CodeBase finalResult, CodeBase destructor,
                                                                  CodeBase mover)
        {
            if (destructor.IsEmpty && mover.IsEmpty)
            {
                if (Size.IsZero) // No temp storage 
                    return finalResult; // Just return final result

                var alignedSize = Size.ByteAlignedSize;
                var alignedThis = CreateBitCast(alignedSize);
                var sequencedResult = alignedThis.CreateSequence(finalResult);
                var result = sequencedResult.CreateChild(new StatementEnd(finalResult.Size, alignedSize));
                return result;
            }
            NotImplementedMethod(finalResult, destructor, mover);
            throw new NotImplementedException();
        }

        public CodeBase CreateCall(int index, Size resultSize)
        {
            return CreateChild(new Call(index, resultSize, Size));
        }

        public Container Serialize()
        {
            try
            {
                return Serialize(Size.Create(0), "");
            }
            catch (Container.UnexpectedContextRefInContainer e)
            {
                DumpMethodWithBreak("UnexpectedContextRefInContainer " + e.VisitedObject.Dump());
                throw;
            }
        }

        private CodeBase CreateAssign(RefAlignParam refAlignParam, Size size)
        {
            return CreateChild(new Assign(refAlignParam, size));
        }

        public static LeafElement CreateRecursiveCall(Size refsSize)
        {
            return new RecursiveCallCandidate(refsSize);
        }

        public CodeBase TryReplacePrimitiveRecursivity(int functionIndex)
        {
            if (!Size.IsZero)
                return this;

            var newResult = Visit(new ReplacePrimitiveRecursivity(functionIndex));
            return newResult ?? this;
        }

        internal virtual BitsConst Evaluate()
        {
            NotImplementedMethod();
            return null;
        }

        internal CodeBase Align()
        {
            return CreateBitCast(Size.ByteAlignedSize);
        }
    }

    internal class Assign : LeafElement
    {
        [DumpData(true)] private readonly RefAlignParam _refAlignParam;
        [DumpData(true)] private readonly Size _size;

        public Assign(RefAlignParam refAlignParam, Size size)
        {
            _refAlignParam = refAlignParam;
            _size = size;
        }

        public override Size Size
        {
            get { return Size.Zero; }
        }

        public override Size DeltaSize
        {
            get { return _refAlignParam.RefSize + _size; }
        }

        protected override string Format(StorageDescriptor start)
        {
            return start.Assign(_refAlignParam, _size);
        }
    }

    internal class Pending : CodeBase
    {
        public override Size Size
        {
            get { return Size.Pending; }
        }

        public override bool IsPending
        {
            get { return true; }
        }

        public override Result VirtVisit<Result>(Visitor<Result> actual)
        {
            throw new UnexpectedVisitOfPending();
        }
    }

    internal class UnexpectedVisitOfPending : Exception
    {
    }
}