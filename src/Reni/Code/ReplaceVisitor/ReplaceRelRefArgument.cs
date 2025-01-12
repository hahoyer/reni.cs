using Reni.Basics;

namespace Reni.Code.ReplaceVisitor
{
    sealed class ReplaceRelRefArgument : ReplaceArgument
    {
        ReplaceRelRefArgument(ResultCache actualArg, Size offset)
            : base(actualArg)
        {
            Offset = offset;
            StopByObjectIds(-9);
        }

        internal ReplaceRelRefArgument(ResultCache actualArg)
            : this(actualArg, Size.Zero) {}

        [EnableDump]
        Size Offset {get;}

        [DisableDump]
        protected override CodeBase ActualCode
            => Offset.IsZero ? ActualArg.Code : ActualArg.Code.GetReferenceWithOffset(Offset);

        protected override Visitor<CodeBase, FiberItem> After(Size size)
            => new ReplaceRelRefArgument(ActualArg, Offset + size);
    }
}