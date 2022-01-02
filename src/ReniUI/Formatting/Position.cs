using hw.DebugFormatter;

namespace ReniUI.Formatting;

abstract class Position : DumpableObject
{
    internal sealed class Function : Position
    {
        internal static readonly Function Instance = new();
        Function() { }
    }

    internal sealed class LineBreak : Position
    {
        internal static readonly LineBreak Instance = new();

        LineBreak()
            : base(true) { }

        internal override Position Combine(Position other)
            => other is Left? this : base.Combine(other);
    }

    internal sealed class IndentAll : Position
    {
        internal static readonly IndentAll Instance = new();

        IndentAll()
            : base(indent: true) { }

        internal override Position Combine(Position other)
            => other switch
            {
                BeforeToken => LineBreakAndIndent.Instance //
                , InnerRight => other
                , _ => base.Combine(other)
            };
    }

    internal sealed class IndentAllAndForceLineSplit : Position
    {
        internal static readonly IndentAllAndForceLineSplit Instance = new();

        IndentAllAndForceLineSplit()
            : base(indent: true, forceLineBreak: true) { }

        internal override Position Combine(Position other)
            => other switch
            {
                BeforeToken => LineBreakAndIndentAndForceLineBreak.Instance //
                , InnerRight => other
                , _ => base.Combine(other)
            };
    }

    internal sealed class BeforeToken : Position
    {
        internal static BeforeToken Instance { get; } = new();

        BeforeToken()
            : base(true) { }

        internal override Position Combine(Position other)
            => other is Right? this : base.Combine(other);
    }

    internal sealed class AfterListToken : Position
    {
        internal static Position Instance { get; } = new AfterListToken();

        AfterListToken()
            : base(true) { }

        internal override Position Combine(Position other)
            => other switch
            {
                Left => this, _ => base.Combine(other)
            };
    }

    internal sealed class AfterListTokenWithAdditionalLineBreak : Position
    {
        internal static Position Instance { get; } = new AfterListTokenWithAdditionalLineBreak();

        AfterListTokenWithAdditionalLineBreak()
            : base(true, hasAdditionalLineBreak:true) { }

        internal override Position Combine(Position other)
            => other switch
            {
                Left => this, _ => base.Combine(other)
            };
    }

    internal sealed class AfterColonToken : Position
    {
        internal static AfterColonToken Instance { get; } = new();

        AfterColonToken()
            : base(true, anchorIndent: true) { }

        internal override Position Combine(Position other)
            => other switch
            {
                Left => LeftAfterColonToken.Instance //
                , Function => null
                , _ => base.Combine(other)
            };
    }

    internal sealed class Left : Position
    {
        internal static Left Instance { get; } = new();

        Left() { }

        internal override Position Combine(Position other)
            => other is InnerLeft? null : base.Combine(other);
    }

    internal sealed class InnerLeft : Position
    {
        internal static InnerLeft Instance { get; } = new();

        InnerLeft()
            : base(true) { }

        internal override Position Combine(Position other)
            => other switch
            {
                Left => null, IndentAllAndForceLineSplit => InnerLeftSimple.Instance, _ => base.Combine(other)
            };
    }

    internal sealed class Inner : Position
    {
        internal static Position Instance { get; } = new Inner();

        Inner()
            : base(true, anchorIndent: true) { }
    }

    internal sealed class InnerWithAdditionalLineBreak : Position
    {
        internal static Position Instance { get; } = new InnerWithAdditionalLineBreak();

        InnerWithAdditionalLineBreak()
            : base(true, anchorIndent: true, hasAdditionalLineBreak: true) { }
    }

    internal sealed class InnerRight : Position
    {
        internal static InnerRight Instance { get; } = new();

        InnerRight()
            : base(true) { }

        internal override Position Combine(Position other)
            => other switch
            {
                Right => this //
                , AfterListToken => this
                , AfterListTokenWithAdditionalLineBreak => this
                , _ => base.Combine(other)
            };
    }

    internal sealed class Right : Position
    {
        internal static Right Instance { get; } = new();

        Right() { }
    }

    internal sealed class Begin : Position
    {
        public static Begin Instance { get; } = new();
        Begin() { }
        internal override Position Combine(Position other) => this;
    }

    internal sealed class End : Position
    {
        End(bool hasLineBreak)
            : base(hasLineBreak) { }

        internal override Position Combine(Position other) => this;

        public static End CreateInstance(bool hasLineBreak) => new(hasLineBreak);
    }

    sealed class LineBreakAndIndentAndForceLineBreak : Position
    {
        public static LineBreakAndIndentAndForceLineBreak Instance { get; } = new();

        LineBreakAndIndentAndForceLineBreak()
            : base(true, true, forceLineBreak: true) { }
    }

    sealed class LineBreakAndIndent : Position
    {
        public static LineBreakAndIndent Instance { get; } = new();

        LineBreakAndIndent()
            : base(true, true) { }
    }

    sealed class LeftAfterColonToken : Position
    {
        public static LeftAfterColonToken Instance { get; } = new();

        LeftAfterColonToken()
            : base(true) { }
    }

    sealed class InnerLeftSimple : Position
    {
        public static InnerLeftSimple Instance { get; } = new();

        InnerLeftSimple()
            : base(true, true, forceLineBreak: true) { }
    }

    internal readonly bool Indent;
    internal readonly bool AnchorIndent;
    internal readonly bool ForceLineBreak;
    internal readonly bool HasAdditionalLineBreak;
    readonly bool HasLineBreak;

    Position
    (
        bool hasLineBreak = false
        , bool indent = false
        , bool anchorIndent = false
        , bool forceLineBreak = false
        , bool hasAdditionalLineBreak = false
    )
    {
        Indent = indent;
        AnchorIndent = anchorIndent;
        HasLineBreak = hasLineBreak;
        ForceLineBreak = forceLineBreak;
        HasAdditionalLineBreak = hasAdditionalLineBreak;
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