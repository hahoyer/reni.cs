using System;
using System.Collections.Generic;
using System.Linq;
using hw.Forms;
using Reni.Basics;

namespace Reni.Code
{
    /// <summary>
    ///     Then-Else construct
    /// </summary>
    sealed class ThenElse : FiberItem
    {
        static int _nextId;

        [Node]
        readonly Size _condSize;

        [Node]
        internal readonly CodeBase ThenCode;

        [Node]
        internal readonly CodeBase ElseCode;

        internal ThenElse(CodeBase thenCode, CodeBase elseCode)
            : this(Size.Create(1), thenCode, elseCode)
        {}

        ThenElse(Size condSize, CodeBase thenCode, CodeBase elseCode)
            : base(_nextId++)
        {
            _condSize = condSize;
            ThenCode = thenCode;
            ElseCode = elseCode;
        }

        protected override CodeArgs GetRefsImplementation() { return ThenCode.Exts.Sequence(ElseCode.Exts); }

        internal override FiberItem[] TryToCombineBack(BitCast preceding)
        {
            if(preceding.InputSize == preceding.OutputSize)
                return null;
            return new FiberItem[]
            {
                new BitCast(preceding.InputSize, preceding.InputSize, Size.Create(1)),
                new ThenElse(preceding.InputSize, ThenCode, ElseCode)
            };
        }

        internal override Size InputSize { get { return _condSize; } }
        internal override Size OutputSize { get { return ThenCode.Size; } }
        internal override bool HasArg { get { return ThenCode.HasArg || ElseCode.HasArg; } }
        protected override Size GetAdditionalTemporarySize()
        {
            return ThenCode.TemporarySize.Max(ElseCode.TemporarySize).Max(OutputSize) - OutputSize;
        }
        protected override FiberItem VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.ThenElse(this); }
        internal override void Visit(IVisitor visitor) { visitor.ThenElse(_condSize, ThenCode, ElseCode); }
        internal FiberItem ReCreate(CodeBase newThen, CodeBase newElse)
        {
            return new ThenElse(_condSize, newThen ?? ThenCode, newElse ?? ElseCode);
        }
    }
}