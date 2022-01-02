using hw.DebugFormatter;

namespace ReniUI.Formatting;

abstract class Position : DumpableObject
{
    internal sealed class Function : Position { }

    internal sealed class LineBreak : Position
    {
        internal LineBreak()
            : base(true) { }

        internal override Position Combine(Position other)
            => other is Left? this : base.Combine(other);
    }

    internal sealed class IndentAll : Position
    {
        internal IndentAll()
            : base(indent: true) { }

        internal override Position Combine(Position other)
            => other switch
            {
                BeforeToken => new LineBreakAndIndent() //
                , InnerRight => other
                , _ => base.Combine(other)
            };
    }

    internal sealed class IndentAllAndForceLineSplit : Position
    {
        internal IndentAllAndForceLineSplit()
            : base(indent: true, forceLineBreak: true) { }

        internal override Position Combine(Position other)
            => other switch
            {
                BeforeToken => new LineBreakAndIndentAndForceLineBreak() //
                , InnerRight => other
                , _ => base.Combine(other)
            };
    }

    internal sealed class BeforeToken : Position
    {
        internal BeforeToken()
            : base(true) { }

        internal override Position Combine(Position other)
            => other is Right? this : base.Combine(other);
    }

    internal sealed class AfterListToken : Position
    {
        internal AfterListToken()
            : base(true) { }

        internal override Position Combine(Position other)
            => other switch
            {
                Left => this, _ => base.Combine(other)
            };
    }

    internal sealed class AfterColonToken : Position
    {
        internal AfterColonToken()
            : base(true, anchorIndent: true) { }

        internal override Position Combine(Position other)
            => other switch
            {
                Left => new LeftAfterColonToken() //
                , Function => null
                , _ => base.Combine(other)
            };
    }

    internal sealed class Left : Position
    {
        internal override Position Combine(Position other)
            => other is InnerLeft? null : base.Combine(other);
    }

    internal sealed class InnerLeft : Position
    {
        public InnerLeft()
            : base(true) { }

        internal override Position Combine(Position other)
            => other switch
            {
                Left => null
                , IndentAllAndForceLineSplit => new InnerLeftSimple()
                , _ => base.Combine(other)
            };
    }

    internal sealed class Inner : Position
    {
        public Inner()
            : base(true, anchorIndent: true) { }
    }

    internal sealed class InnerRight : Position
    {
        public InnerRight()
            : base(true) { }

        internal override Position Combine(Position other)
            => other switch
            {
                Right => this //
                , AfterListToken => this
                , _ => base.Combine(other)
            };
    }

    internal sealed class Right : Position { }

    internal sealed class Begin : Position
    {
        internal override Position Combine(Position other) => this;
    }

    internal sealed class End : Position
    {
        internal End(bool hasLineBreak)
            : base(hasLineBreak) { }

        internal override Position Combine(Position other) => this;
    }

    sealed class LineBreakAndIndentAndForceLineBreak : Position
    {
        internal LineBreakAndIndentAndForceLineBreak()
            : base(true, true, forceLineBreak: true) { }
    }

    sealed class LineBreakAndIndent : Position
    {
        internal LineBreakAndIndent()
            : base(true, true) { }
    }

    sealed class LeftAfterColonToken : Position
    {
        internal LeftAfterColonToken()
            : base(true) { }
    }

    sealed class InnerLeftSimple : Position
    {
        internal InnerLeftSimple()
            : base(true, true, forceLineBreak: true) { }
    }

    internal readonly bool Indent;
    internal readonly bool AnchorIndent;
    internal readonly bool ForceLineBreak;

    internal bool HasAdditionalLineBreak;
    readonly bool HasLineBreak;

    Position
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

    internal virtual Position Combine(Position other)
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