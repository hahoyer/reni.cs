using hw.DebugFormatter;
using Reni.Parser;

namespace ReniUI.Formatting;

abstract class PositionParent : DumpableObject
{
    internal class Function : PositionParent
    {
        internal Function(BinaryTreeProxy parent)
            : base(parent) { }
    }

    internal class LineBreak : PositionParent
    {
        internal LineBreak(BinaryTreeProxy parent)
            : base(parent, true) { }

        internal override PositionParent Combine(PositionParent other)
            => other is Left? this : base.Combine(other);
    }

    internal class IndentAll : PositionParent
    {
        internal IndentAll(BinaryTreeProxy parent)
            : base(parent, indent: true) { }

        internal override PositionParent Combine(PositionParent other)
            => other switch
            {
                BeforeToken => new LineBreakAndIndent(Parent, other) //
                , InnerRight => other
                , _ => base.Combine(other)
            };
    }

    internal class IndentAllAndForceLineSplit : PositionParent
    {
        internal IndentAllAndForceLineSplit(BinaryTreeProxy parent)
            : base(parent, indent: true, forceLinesSplit: true) { }

    }

    internal class BeforeToken : PositionParent
    {
        internal BeforeToken(BinaryTreeProxy parent)
            : base(parent, true) { }

        internal override PositionParent Combine(PositionParent other)
            => other is Right? this : base.Combine(other);
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
            => other switch
            {
                Left => new LeftAfterColonToken(Parent, other) //
                , Function => null
                , _ => base.Combine(other)
            };
    }

    internal sealed class Left : PositionParent
    {
        public Left(BinaryTreeProxy parent)
            : base(parent) { }

        internal override PositionParent Combine(PositionParent other)
            => other is InnerLeft? null : base.Combine(other);
    }

    internal sealed class InnerLeft : PositionParent
    {
        public InnerLeft(BinaryTreeProxy parent)
            : base(parent, true) { }

        internal override PositionParent Combine(PositionParent other)
            => other switch
            {
                Left => null
                , IndentAll when other.Parent == Parent => new InnerLeftSimple(Parent)
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
                Right => null, AfterListToken => this, _ => base.Combine(other)
            };
    }

    internal sealed class Right : PositionParent
    {
        public Right(BinaryTreeProxy parent)
            : base(parent) { }

        internal override PositionParent Combine(PositionParent other)
            => other is InnerRight? null : base.Combine(other);
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

    class LineBreakAndIndent : PositionParent
    {
        readonly PositionParent Other;

        internal LineBreakAndIndent(BinaryTreeProxy parent, PositionParent other)
            : base(parent, true, true)
            => Other = other;
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

    internal readonly bool Indent;
    internal readonly bool AnchorIndent;
    internal readonly bool ForceLinesSplit;

    internal bool HasAdditionalLineBreak;
    readonly bool HasLineBreak;

    [DisableDump]
    readonly BinaryTreeProxy Parent;


    protected PositionParent
    (
        BinaryTreeProxy parent
        , bool hasLineBreak = false
        , bool indent = false
        , bool anchorIndent = false
        , bool forceLinesSplit = false
    )
    {
        parent.AssertIsNotNull();
        Parent = parent;
        Indent = indent;
        AnchorIndent = anchorIndent;
        HasLineBreak = hasLineBreak;
        ForceLinesSplit = forceLinesSplit;
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