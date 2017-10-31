using System.Linq;
using hw.Helper;
using Reni.Basics;
using Reni.Feature;
using Reni.Type;

namespace Reni.Code
{
    /// <summary>
    ///     Then-Else construct
    /// </summary>
    sealed class ThenElse : FiberItem
    {
        static int _nextId;

        static Size CondSize => Size.Bit;

        [Node]
        internal readonly CodeBase ElseCode;

        [Node]
        internal readonly CodeBase ThenCode;

        internal ThenElse(CodeBase thenCode, CodeBase elseCode)
            : base(_nextId++)
        {
            ThenCode = thenCode.AssertNotNull();
            ElseCode = elseCode.AssertNotNull();
        }

        internal override Size InputSize => CondSize;
        internal override Size OutputSize => ThenCode.Size;
        internal override bool HasArg => ThenCode.HasArg || ElseCode.HasArg;

        protected override CodeArgs GetRefsImplementation() => ThenCode.Exts.Sequence(ElseCode.Exts);

        internal override FiberItem[] TryToCombineBack(BitCast preceding)
        {
            if(preceding.InputSize == preceding.OutputSize)
                return null;
            return new FiberItem[]
            {
                new BitCast(preceding.InputSize, preceding.InputSize, Size.Create(1)),
                new ThenElse(ThenCode, ElseCode)
            };
        }

        protected override Size GetAdditionalTemporarySize()
            => ThenCode.TemporarySize.Max(ElseCode.TemporarySize).Max(OutputSize) - OutputSize;

        protected override TFiber VisitImplementation<TResult, TFiber>
            (Visitor<TResult, TFiber> actual) => actual.ThenElse(this);

        internal override void Visit(IVisitor visitor)
            => visitor.ThenElse(CondSize, ThenCode, ElseCode);

        internal FiberItem ReCreate(CodeBase newThen, CodeBase newElse)
            => new ThenElse(newThen ?? ThenCode, newElse ?? ElseCode);

        internal TypeBase Visit(Visitor<TypeBase, TypeBase> actual)
            => new[]
                {
                    ThenCode,
                    ElseCode
                }
                .Select(item => item?.Visit(actual))
                .DistinctNotNull();
    }
}