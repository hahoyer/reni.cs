using hw.DebugFormatter;
using Reni.Parser;

namespace ReniUI.Formatting;

abstract class PositionParent : DumpableObject
{
    internal class BracketKernel : PositionParent
    {
        internal BracketKernel(BinaryTreeProxy parent)
            : base(parent, indent: true) { }

        internal override PositionParent Combine(PositionParent other)
            => other is InnerRight? other : base.Combine(other);
    }

    internal class BeforeListToken : PositionParent
    {
        internal BeforeListToken(BinaryTreeProxy parent)
            : base(parent, true) { }
    }

    internal class AfterListToken : PositionParent
    {
        internal AfterListToken(BinaryTreeProxy parent)
            : base(parent, true) { }
    }

    internal class AfterColonToken : PositionParent
    {
        internal AfterColonToken(BinaryTreeProxy parent)
            : base(parent, true, anchorIndent: true) { }

        internal override PositionParent Combine(PositionParent other)
            => other is Left? new LeftAfterColonToken(Parent, other) : base.Combine(other);
    }

    internal sealed class Left : PositionParent
    {
        public Left(BinaryTreeProxy parent)
            : base(parent) { }

        internal override PositionParent Combine(PositionParent other)
            => other is InnerLeft? new BracketClusterLeft(Parent, other.Parent) : base.Combine(other);
    }

    internal sealed class InnerLeft : PositionParent
    {
        public InnerLeft(BinaryTreeProxy parent)
            : base(parent, true) { }

        internal override PositionParent Combine(PositionParent other)
            => other switch
            {
                Left => new InnerBracketClusterLeft(Parent, other.Parent)
                , BracketKernel when other.Parent == Parent => new InnerLeftSimple(Parent)
                , _ => base.Combine(other)
            };
    }

    internal sealed class Inner : PositionParent
    {
        public Inner(BinaryTreeProxy parent)
            : base(parent, true, anchorIndent: true) { }
    }

    internal sealed class InnerRight : PositionParent
    {
        public InnerRight(BinaryTreeProxy parent)
            : base(parent, true) { }

        internal override PositionParent Combine(PositionParent other)
            => other switch
            {
                Right => new InnerBracketClusterRight(Parent, other.Parent)
                , AfterListToken => this
                , _ => base.Combine(other)
            };
    }

    internal sealed class Right : PositionParent
    {
        public Right(BinaryTreeProxy parent)
            : base(parent) { }

        internal override PositionParent Combine(PositionParent other)
            => other is InnerRight? new BracketClusterRight(Parent, other.Parent) : base.Combine(other);
    }

    internal class Begin : PositionParent
    {
        internal Begin(BinaryTreeProxy parent)
            : base(parent) { }

        internal override PositionParent Combine(PositionParent other) => this;
    }

    internal class End : PositionParent
    {
        internal End(BinaryTreeProxy parent, bool hasLineBreak)
            : base(parent, hasLineBreak) { }

        internal override PositionParent Combine(PositionParent other) => this;
    }

    class LeftAfterColonToken : PositionParent
    {
        readonly PositionParent Other;

        internal LeftAfterColonToken(BinaryTreeProxy parent, PositionParent other)
            : base(parent, true)
            => Other = other;
    }

    sealed class InnerLeftSimple : PositionParent
    {
        internal InnerLeftSimple(BinaryTreeProxy parent)
            : base(parent, true, true) { }
    }

    class BracketClusterLeft : PositionParent
    {
        readonly BinaryTreeProxy OtherParent;

        internal BracketClusterLeft(BinaryTreeProxy parent, BinaryTreeProxy otherParent)
            : base(parent)
            => OtherParent = otherParent;
    }

    class InnerBracketClusterLeft : PositionParent
    {
        readonly BinaryTreeProxy OtherParent;

        internal InnerBracketClusterLeft(BinaryTreeProxy parent, BinaryTreeProxy otherParent)
            : base(parent)
            => OtherParent = otherParent;
    }

    class BracketClusterRight : PositionParent
    {
        readonly BinaryTreeProxy OtherParent;

        internal BracketClusterRight(BinaryTreeProxy parent, BinaryTreeProxy otherParent)
            : base(parent)
            => OtherParent = otherParent;
    }

    class InnerBracketClusterRight : PositionParent
    {
        readonly BinaryTreeProxy OtherParent;

        internal InnerBracketClusterRight(BinaryTreeProxy parent, BinaryTreeProxy otherParent)
            : base(parent)
            => OtherParent = otherParent;
    }

    internal readonly bool Indent;
    internal readonly bool AnchorIndent;

    internal bool HasAdditionalLineBreak;
    readonly bool HasLineBreak;

    [DisableDump]
    readonly BinaryTreeProxy Parent;


    protected PositionParent
        (BinaryTreeProxy parent, bool hasLineBreak = false, bool indent = false, bool anchorIndent = false)
    {
        parent.AssertIsNotNull();
        Parent = parent;
        Indent = indent;
        AnchorIndent = anchorIndent;
        HasLineBreak = hasLineBreak;
    }

    internal virtual PositionParent Combine(PositionParent other)
    {
        NotImplementedMethod(other);
        return default;
    }


    internal int LineBreakCount
        => HasLineBreak
            ? HasAdditionalLineBreak
                ? 2
                : 1
            : 0;

    [EnableDump]
    internal ITokenClass TokenClass => Parent.FlatItem.TokenClass;
}