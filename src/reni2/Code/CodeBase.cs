using System;
using HWClassLibrary.Debug;
using Reni.Code.ReplaceVisitor;
using Reni.Context;
using Reni.Parser.TokenClass;

namespace Reni.Code
{
    /// <summary>
    /// Base class for code element
    /// </summary>
    internal abstract class Base : ReniObject
    {
        /// <summary>
        /// Creates the bit array op.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="size">The size.</param>
        /// <param name="leftSize">Size of the left.</param>
        /// <returns></returns>
        /// created 26.09.2006 22:31
        internal Base CreateBitSequenceOperation(Defineable name, Size size, Size leftSize)
        {
            return CreateChild(new BitArrayOp(name, size, leftSize, Size - leftSize));
        }

        /// <summary>
        /// Creates the dump print.
        /// </summary>
        /// <param name="leftSize">Size of the left.</param>
        /// <returns></returns>
        /// created 08.01.2007 16:35
        public Base CreateDumpPrint(Size leftSize)
        {
            return CreateChild(new DumpPrint(leftSize, Size - leftSize));
        }

        /// <summary>
        /// Creates the bit array op.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="size">The size.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        /// created 23.09.2006 14:01
        internal Base CreateBitSequenceOperation(Defineable name, Size size, Base right)
        {
            var alignedSize = Size.ByteAlignedSize;
            var alignedRightSize = right.Size.ByteAlignedSize;

            return CreateBitCast(alignedSize)
                .CreateSequence(right.CreateBitCast(alignedRightSize))
                .CreateBitSequenceOperation(name, size, alignedSize);
        }

        public Base CreateAssign(RefAlignParam refAlignParam, Base source)
        {
            var alignedSize = source.Size.ByteAlignedSize;
            return CreateSequence(source.CreateBitCast(alignedSize))
                .CreateAssign(refAlignParam, alignedSize);
        }

        /// <summary>
        /// Creates the numeric opx.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// created 02.02.2007 23:49
        public Base CreateBitSequenceOperation(Defineable name)
        {
            var alignedSize = Size.ByteAlignedSize;

            return CreateBitCast(alignedSize)
                .CreateChild(new BitArrayPrefixOp(name, alignedSize))
                .CreateBitCast(Size);
        }

        /// <summary>
        /// Creates the dump print text.
        /// </summary>
        /// <param name="dumpPrintText">The dump print text.</param>
        /// <returns></returns>
        /// created 08.01.2007 17:48
        public static Base CreateDumpPrintText(string dumpPrintText)
        {
            return CreateLeaf(new DumpPrintText(dumpPrintText));
        }

        /// <summary>
        /// Creates the dump print.
        /// </summary>
        /// <returns></returns>
        /// created 08.01.2007 16:33
        public Base CreateDumpPrint()
        {
            var alignedSize = Size.ByteAlignedSize;

            return CreateBitCast(alignedSize)
                .CreateDumpPrint(alignedSize);
        }

        /// <summary>
        /// Creates the then else.
        /// </summary>
        /// <param name="thenCode">The then code.</param>
        /// <param name="elseCode">The else code.</param>
        /// <returns></returns>
        /// created 09.01.2007 03:42
        public Base CreateThenElse(Base thenCode, Base elseCode)
        {
            return new ThenElse(this, thenCode, elseCode);
        }

        /// <summary>
        /// Creates the top ref.
        /// </summary>
        /// <param name="refAlignParam">The ref align param.</param>
        /// <returns></returns>
        /// created 23.09.2006 14:02
        public static Base CreateTopRef(RefAlignParam refAlignParam)
        {
            return CreateLeaf(new TopRef(refAlignParam, Size.Create(0)));
        }

        /// <summary>
        /// Creates the args ref code.
        /// </summary>
        /// <param name="refAlignParam">The ref align param.</param>
        /// <returns></returns>
        /// created 03.01.2007 22:20
        public static Base CreateFrameRef(RefAlignParam refAlignParam)
        {
            return CreateLeaf(new FrameRef(refAlignParam, Size.Create(0)));
        }

        private static Base CreateLeaf(LeafElement leafElement)
        {
            return new Leaf(leafElement);
        }

        /// <summary>
        /// Creates the child.
        /// </summary>
        /// <param name="leafElement">The leaf element.</param>
        /// <returns></returns>
        /// created 06.10.2006 00:20
        public virtual Base CreateChild(LeafElement leafElement)
        {
            return new Child(this, leafElement);
        }

        /// <summary>
        /// Serializes this instance.
        /// </summary>
        /// <param name="frameSize">Size of the args.</param>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        /// created 23.09.2006 14:05
        public Container Serialize(Size frameSize, string description)
        {
            var container = new Container(MaxSize, frameSize, description);
            Visit(container);
            return container;
        }

        /// <summary>
        /// Gets the size of the max.
        /// </summary>
        /// <value>The size of the max.</value>
        /// created 23.09.2006 14:13
        [DumpData(false)]
        public virtual Size MaxSize
        {
            get { return Size; }
        }

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        /// created 23.09.2006 14:15
        public virtual Size Size
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        /// created 23.09.2006 14:23
        [DumpData(false)]
        public virtual bool IsEmpty
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is relative reference.that needs offset in argreplace
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is relative reference; otherwise, <c>false</c>.
        /// </value>
        /// created 23.09.2006 22:12
        [DumpExcept(false)]
        public bool IsRelativeReference
        {
            get { return RefAlignParam != null; }
        }

        /// <summary>
        /// Creates the plusop.
        /// </summary>
        /// <param name="refAlignParam">The ref align param.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        /// created 23.09.2006 14:20
        public Base CreateRefPlus(RefAlignParam refAlignParam, Size right)
        {
            return CreateChild(new RefPlus(refAlignParam, right));
        }

        /// <summary>
        /// Creates the unref.
        /// </summary>
        /// <param name="refAlignParam">The ref align param.</param>
        /// <param name="targetSize">The size.</param>
        /// <returns></returns>
        /// created 23.09.2006 14:27
        public Base CreateDereference(RefAlignParam refAlignParam, Size targetSize)
        {
            return CreateChild(new Dereference(refAlignParam, targetSize, targetSize));
        }

        /// <summary>
        /// Creates the bit cast.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        /// created 23.09.2006 14:22
        public Base CreateBitCast(Size size)
        {
            if (Size == size)
                return this;
            return CreateChild(new BitCast(Size, size, Size));
        }

        /// <summary>
        /// Creates a sequence. If one of the elements is empty, just the other one is returned
        /// </summary>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        /// created 23.09.2006 14:23
        public Base CreateSequence(Base right)
        {
            if (IsEmpty)
                return right;
            if (right.IsEmpty)
                return this;
            return new Pair(this, right);
        }

        /// <summary>
        /// Creates the bit array.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        /// created 23.09.2006 14:24
        public static Base CreateBitArray(Size size, BitsConst t)
        {
            return CreateLeaf(new BitArray(size, t));
        }

        /// <summary>
        /// Creates the bit array.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        /// created 12.01.2007 21:10
        public static Base CreateBitArray(BitsConst t)
        {
            return CreateBitArray(t.Size, t);
        }

        /// <summary>
        /// Creates the void.
        /// </summary>
        /// <returns></returns>
        /// created 08.11.2006 00:33
        public static Base CreateVoid()
        {
            return CreateLeaf(BitArray.CreateVoid());
        }

        /// <summary>
        /// Creates the arg.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        /// created 23.09.2006 14:26
        public static Base CreateArg(Size size)
        {
            return new Arg(size);
        }

        /// <summary>
        /// Creates the context ref.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// created 16.10.2006 22:15
        public static Base CreateContextRef<C>(C context) where C : Context.Base
        {
            return new ContextRef<C>(context);
        }

        /// <summary>
        /// Uses the with arg.
        /// </summary>
        /// <param name="argCode">The arg code.</param>
        /// <returns></returns>
        /// created 23.09.2006 14:29
        public Base UseWithArg(Base argCode)
        {
            var result = argCode.IsRelativeReference
                             ? Visit(new ReplaceRelRefArg(argCode, argCode.RefAlignParam))
                             : Visit(new ReplaceAbsoluteArg(argCode));
            return result ?? this;
        }

        /// <summary>
        /// Replaces the context ref.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="replacement">The replacement.</param>
        /// <returns></returns>
        public Base ReplaceRelativeContextRef<C>(C context, Base replacement) where C : Context.Base
        {
            var result = Visit(new ReplaceRelativeContextRef<C>(context, replacement));
            if (result != null)
                return result;
            return this;
        }

        /// <summary>
        /// Replaces the context ref.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="replacement">The replacement.</param>
        /// <returns></returns>
        public Base ReplaceAbsoluteContextRef<C>(C context, Base replacement) where C : Context.Base
        {
            var result = Visit(new ReplaceAbsoluteContextRef<C>(context, replacement));
            if (result != null)
                return result;
            return this;
        }

        /// <summary>
        /// Gets the ref align param.
        /// </summary>
        /// <value>The ref align param.</value>
        /// created 19.10.2006 19:46
        [DumpData(false)]
        public virtual RefAlignParam RefAlignParam
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the code for pending.visits
        /// </summary>
        /// <value>The pending.</value>
        /// created 24.01.2007 22:30
        public static Base Pending
        {
            get { return new Pending(); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is pending.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is pending; otherwise, <c>false</c>.
        /// </value>
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

        /// <summary>
        /// Visitor to replace parts of code
        /// </summary>
        /// <param name="actual"></param>
        /// <returns></returns>
        public Result Visit<Result>(Visitor<Result> actual)
        {
            return VirtVisit(actual);
        }

        /// <summary>
        /// Visitor to replace parts of code, overridable version.
        /// </summary>
        /// <param name="actual">The actual.</param>
        /// <returns></returns>
        public virtual Result VirtVisit<Result>(Visitor<Result> actual)
        {
            NotImplementedMethod(actual);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the statement end from intermediate storage.
        /// </summary>
        /// <param name="finalResult">The final result.</param>
        /// <param name="destructor">The destructor.</param>
        /// <param name="mover">The mover.</param>
        /// <returns></returns>
        /// created 20.05.2007 12:52 on HAHOYER-DELL by hh
        public Base CreateStatementEndFromIntermediateStorage(Base finalResult, Base destructor, Base mover)
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

        /// <summary>
        /// Creates the call.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="resultSize">Size of the result.</param>
        /// <returns></returns>
        /// created 06.11.2006 23:59
        public Base CreateCall(int index, Size resultSize)
        {
            return CreateChild(new Call(index, resultSize, Size));
        }

        /// <summary>
        /// Serializes this instance.
        /// </summary>
        /// <returns></returns>
        /// created 26.11.2006 17:01
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

        private Base CreateAssign(RefAlignParam refAlignParam, Size size)
        {
            return CreateChild(new Assign(refAlignParam, size));
        }

        public static LeafElement CreateRecursiveCall(Size refsSize)
        {
            return new RecursiveCallCandidate(refsSize);
        }

        public Base TryReplacePrimitiveRecursivity(int functionIndex)
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

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        /// created 05.10.2006 23:40
        public override Size Size
        {
            get { return Size.Zero; }
        }

        /// <summary>
        /// Gets the size of the delta.
        /// </summary>
        /// <value>The size of the delta.</value>
        /// created 10.10.2006 00:21
        public override Size DeltaSize
        {
            get { return _refAlignParam.RefSize + _size; }
        }

        /// <summary>
        /// Formats this instance.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <returns></returns>
        /// created 07.10.2006 21:11
        protected override string Format(StorageDescriptor start)
        {
            return start.Assign(_refAlignParam, _size);
        }
    }

    internal class Pending : Base
    {
        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        /// created 23.09.2006 14:15
        public override Size Size
        {
            get { return Size.Pending; }
        }

        /// <summary>
        /// Visitor to replace parts of code, overridable version.
        /// </summary>
        /// <param name="actual">The actual.</param>
        /// <returns></returns>
        public override Result VirtVisit<Result>(Visitor<Result> actual)
        {
            throw new UnexpectedVisitOfPending();
        }

        /// <summary>
        /// Gets a value indicating whether this instance is pending.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is pending; otherwise, <c>false</c>.
        /// </value>
        public override bool IsPending
        {
            get { return true; }
        }
    }

    internal class UnexpectedVisitOfPending : Exception
    {
    }
}