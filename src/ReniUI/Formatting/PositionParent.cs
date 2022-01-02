using hw.DebugFormatter;

namespace ReniUI.Formatting;

abstract class PositionParent : DumpableObject
{
    internal sealed class Function : PositionParent { }

    internal sealed class LineBreak : PositionParent
    {
        internal LineBreak()
            : base(true) { }

        internal override PositionParent Combine(PositionParent other)
            => other is Left? this : base.Combine(other);
    }

    internal sealed class IndentAll : PositionParent
    {
        internal IndentAll()
            : base(indent: true) { }

        internal override PositionParent Combine(PositionParent other)
            => other switch
            {
                BeforeToken => new LineBreakAndIndent() //
                , InnerRight => other
                , _ => base.Combine(other)
            };
    }

    internal sealed class IndentAllAndForceLineSplit : PositionParent
    {
        internal IndentAllAndForceLineSplit()
            : base(indent: true, forceLineBreak: true) { }

        internal override PositionParent Combine(PositionParent other)
            => other switch
            {
                BeforeToken => new LineBreakAndIndentAndForceLineBreak() //
                , InnerRight => other
                , _ => base.Combine(other)
            };
    }

    internal sealed class BeforeToken : PositionParent
    {
        internal BeforeToken()
            : base(true) { }

        internal override PositionParent Combine(PositionParent other)
            => other is Right? this : base.Combine(other);
    }

    internal sealed class AfterListToken : PositionParent
    {
        internal AfterListToken()
            : base(true) { }

        internal override PositionParent Combine(PositionParent other)
            => other switch
            {
                Left => this, _ => base.Combine(other)
            };
    }

    internal sealed class AfterColonToken : PositionParent
    {
        internal AfterColonToken()
            : base(true, anchorIndent: true) { }

        internal override PositionParent Combine(PositionParent other)
            => other switch
            {
                Left => new LeftAfterColonToken() //
                , Function => null
                , _ => base.Combine(other)
            };
    }

    internal sealed class Left : PositionParent
    {
        internal override PositionParent Combine(PositionParent other)
            => other is InnerLeft? null : base.Combine(other);
    }

    internal sealed class InnerLeft : PositionParent
    {
        public InnerLeft()
            : base(true) { }

        internal override PositionParent Combine(PositionParent other)
            => other switch
            {
                Left => null
                , IndentAllAndForceLineSplit when other.Parent == Parent => new InnerLeftSimple()
                , _ => base.Combine(other)
            };
    }

    internal sealed class Inner : PositionParent
    {
        public Inner()
            : base(true, anchorIndent: true) { }
    }

    internal sealed class InnerRight : PositionParent
    {
        public InnerRight()
            : base(true) { }

        internal override PositionParent Combine(PositionParent other)
            => other switch
            {
                Right => this //
                , AfterListToken => this
                , _ => base.Combine(other)
            };
    }

    internal sealed class Right : PositionParent { }

    internal sealed class Begin : PositionParent
    {
        internal override PositionParent Combine(PositionParent other) => this;
    }

    internal sealed class End : PositionParent
    {
        internal End(bool hasLineBreak)
            : base(hasLineBreak) { }

        internal override PositionParent Combine(PositionParent other) => this;
    }

    sealed class LineBreakAndIndentAndForceLineBreak : PositionParent
    {
        internal LineBreakAndIndentAndForceLineBreak()
            : base(true, true, forceLineBreak: true) { }
    }

    sealed class LineBreakAndIndent : PositionParent
    {
        internal LineBreakAndIndent()
            : base(true, true) { }
    }

    sealed class LeftAfterColonToken : PositionParent
    {
        internal LeftAfterColonToken()
            : base(true) { }
    }

    sealed class InnerLeftSimple : PositionParent
    {
        internal InnerLeftSimple()
            : base(true, true, forceLineBreak: true) { }
    }

    internal readonly bool Indent;
    internal readonly bool AnchorIndent;
    internal readonly bool ForceLineBreak;

    internal bool HasAdditionalLineBreak;
    readonly bool HasLineBreak;

    readonly BinaryTreeProxy Parent;

    PositionParent
    (
        bool hasLineBreak = false
        , bool indent = false
        , bool anchorIndent = false
        , bool forceLineBreak = false
    )
    {
        Indent = indent;
        AnchorIndent = anchorIndent;
        HasLineBreak = hasLineBreak;
        ForceLineBreak = forceLineBreak;
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
}