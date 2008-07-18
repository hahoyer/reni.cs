using System;
using HWClassLibrary.Debug;

namespace Reni.Code
{
    /// <summary>
    /// Then-Else construct
    /// </summary>
    internal sealed class ThenElse : CodeBase
    {
        private static int _nextId;

        private readonly int _thenElseObjectId = _nextId++;
        private readonly CodeBase _condCode;
        private readonly CodeBase _thenCode;
        private readonly CodeBase _elseCode;

        /// <summary>
        /// Gets the code.of condition
        /// </summary>
        /// <value>The cond code.</value>
        /// created 27.01.2007 11:33
        public CodeBase CondCode { get { return _condCode; } }
        /// <summary>
        /// Gets the code.then-branch
        /// </summary>
        /// <value>The then code.</value>
        /// created 27.01.2007 11:34
        public CodeBase ThenCode { get { return _thenCode; } }
        /// <summary>
        /// Gets the code.of else-branch
        /// </summary>
        /// <value>The else code.</value>
        /// created 27.01.2007 11:34
        public CodeBase ElseCode { get { return _elseCode; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThenElse"/> class.
        /// </summary>
        /// <param name="condCode">The cond code.</param>
        /// <param name="thenCode">The then code.</param>
        /// <param name="elseCode">The else code.</param>
        /// created 09.01.2007 04:42
        public ThenElse(CodeBase condCode, CodeBase thenCode, CodeBase elseCode)
        {
            _condCode = condCode;
            _thenCode = thenCode;
            _elseCode = elseCode;
        }

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        /// created 05.10.2006 23:40
        public override Size Size { get { return _thenCode.Size; } }

        /// <summary>
        /// Visitor to replace parts of code, overridable version.
        /// </summary>
        /// <param name="actual">The actual.</param>
        /// <returns></returns>
        public override Result VirtVisit<Result>(Visitor<Result> actual)
        {
            return actual.ThenElseVisit(this);
        }

        public int ThenElseObjectId { get { return _thenElseObjectId; } }

        /// <summary>
        /// Gets the size of the max.
        /// </summary>
        /// <value>The size of the max.</value>
        /// created 23.09.2006 14:13
        public override Size MaxSize
        {
            get
            {
                var cSize = _condCode.MaxSize;
                var tSize = _thenCode.MaxSize;
                var eSize = _elseCode.MaxSize;
                return cSize.Max(tSize).Max(eSize);
            }
        }
    }

    internal sealed class EndCondional : LeafElement
    {
        [DumpData(true)]
        private readonly int _thenElseObjectId;

        public EndCondional(int thenElseObjectId)
        {
            _thenElseObjectId = thenElseObjectId;
        }

        /// <summary>
        /// Gets the size of the delta.
        /// </summary>
        /// <value>The size of the delta.</value>
        /// created 10.10.2006 00:21
        public override Size DeltaSize { get { return Size.Zero; } }

        /// <summary>
        /// Formats this instance.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <returns></returns>
        /// created 07.10.2006 21:11
        protected override string Format(StorageDescriptor start)
        {
            return start.EndCondional(_thenElseObjectId);
        }

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        /// created 05.10.2006 23:40
        public override Size Size { get { return Size.Zero; } }
    }

    internal sealed class Else : LeafElement
    {
        [DumpData(true)]
        private readonly int _thenElseObjectId;
        [DumpData(true)]
        private readonly Size _thenSize;

        public Else(int thenElseObjectId, Size thenSize)
        {
            _thenElseObjectId = thenElseObjectId;
            _thenSize = thenSize;
        }

        /// <summary>
        /// Gets the size of the delta.
        /// </summary>
        /// <value>The size of the delta.</value>
        /// created 10.10.2006 00:21
        public override Size DeltaSize { get { return _thenSize; } }

        /// <summary>
        /// Formats this instance.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <returns></returns>
        /// created 07.10.2006 21:11
        protected override string Format(StorageDescriptor start)
        {
            return start.Else(_thenElseObjectId);
        }

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        /// created 05.10.2006 23:40
        public override Size Size { get { return Size.Zero; } }
    }

    internal sealed class Then : LeafElement
    {
        internal readonly int ThenElseObjectId;
        internal readonly Size CondSize;

        public Then(int thenElseObjectId, Size condSize)
        {
            ThenElseObjectId = thenElseObjectId;
            CondSize = condSize;
        }

        public override Size DeltaSize { get { return CondSize; } }
        public override Size Size { get { return Size.Zero; } }

        internal override LeafElement TryToCombineBack(BitCast precedingElement)
        {
            if(precedingElement.Size == CondSize)
                return new Then(ThenElseObjectId, precedingElement.TargetSize);
            return null;
        }

        internal override LeafElement TryToCombineBack(BitArrayOp precedingElement)
        {
            if(precedingElement.Size == CondSize)
                return new BitArrayOpThen(this, precedingElement);
            return null;
        }

        protected override string Format(StorageDescriptor start)
        {
            return start.Then(ThenElseObjectId, CondSize);
        }
    }

    internal class BitArrayOpThen : LeafElement
    {
        [DumpData(true)]
        private readonly BitArrayOp _bitArrayOp;
        [DumpData(true)]
        private readonly Then _thenCode;

        public BitArrayOpThen(Then thenCode, BitArrayOp bitArrayOp)
        {
            _thenCode = thenCode;
            _bitArrayOp = bitArrayOp;
        }

        public override Size Size { get { return _bitArrayOp.DeltaSize; } }
        public override Size DeltaSize { get { return _thenCode.DeltaSize + _bitArrayOp.DeltaSize; } }

        protected override string Format(StorageDescriptor start)
        {
            return start.BitArrayOpThen(_bitArrayOp.OpToken, _bitArrayOp.LeftSize, _bitArrayOp.RightSize, _thenCode.ThenElseObjectId, _thenCode.CondSize);
        }
    }
}