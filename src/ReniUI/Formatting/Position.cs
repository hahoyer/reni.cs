using System.Collections.Generic;
using hw.DebugFormatter;
using JetBrains.Annotations;

namespace ReniUI.Formatting;

abstract class Position : DumpableObject
{
    static class Classes
    {
        internal sealed class LineBreak : Position
        {
            internal LineBreak()
                : base(hasLineBreak: true) { }

            protected override Position Combine(Position other)
                => other == Left
                    ? this
                    : other == IndentAll
                        ? LineBreakAndIndent
                        : base.Combine(other);
        }

        internal sealed class IndentAll : Position
        {
            internal IndentAll()
                : base(indent: true) { }

            protected override Position Combine(Position other)
                => other == BeforeToken
                    ? LineBreakAndIndent
                    : other == InnerRight
                        ? other
                        : other == Inner
                            ? InnerWithIndent
                            : base.Combine(other);
        }

        internal sealed class IndentAllAndForceLineSplit : Position
        {
            internal IndentAllAndForceLineSplit()
                : base(indent: true, forceLineBreak: true) { }

            protected override Position Combine(Position other)
                => other == BeforeToken
                    ? LineBreakAndIndentAndForceLineBreak
                    : other == InnerRight
                        ? other
                        : base.Combine(other);
        }

        internal sealed class BeforeToken : Position
        {
            internal BeforeToken()
                : base(hasLineBreak: true) { }

            protected override Position Combine(Position other)
                => other == Right? this : base.Combine(other);
        }

        internal sealed class AfterListToken : Position
        {
            internal AfterListToken()
                : base(hasLineBreak: true) { }

            protected override Position Combine(Position other)
                => other == Left? this : base.Combine(other);
        }

        internal sealed class AfterListTokenWithAdditionalLineBreak : Position
        {
            internal AfterListTokenWithAdditionalLineBreak()
                : base(hasLineBreak: true, hasAdditionalLineBreak: true) { }

            protected override Position Combine(Position other)
                => other == Left? this : base.Combine(other);
        }

        internal sealed class AfterColonToken : Position
        {
            internal AfterColonToken()
                : base(hasLineBreak: true, anchorIndent: true) { }

            protected override Position Combine(Position other)
                => other == Left
                    ? LeftAfterColonToken
                    : other == Function
                        ? null
                        : base.Combine(other);
        }

        internal sealed class Left : Position
        {
            protected override Position Combine(Position other)
                => other is InnerLeft? null : base.Combine(other);
        }

        internal sealed class InnerLeft : Position
        {
            internal InnerLeft()
                : base(hasLineBreak: true) { }

            protected override Position Combine(Position other)
                => other == Left
                    ? null
                    : other == IndentAllAndForceLineSplit
                        ? InnerLeftSimple
                        : base.Combine(other);
        }

        internal sealed class InnerRight : Position
        {
            internal InnerRight()
                : base(hasLineBreak: true) { }

            protected override Position Combine(Position other)
                => other == Right ||
                    other == AfterListToken ||
                    other == AfterListTokenWithAdditionalLineBreak
                        ? this
                        : base.Combine(other);
        }

        internal sealed class InnerWithAdditionalLineBreak : Position
        {
            internal InnerWithAdditionalLineBreak()
                : base(hasLineBreak: true, anchorIndent: true, hasAdditionalLineBreak: true) { }

            protected override Position Combine(Position other)
                => other == InnerRight? other : base.Combine(other);
        }

        internal sealed class InnerWithIndent : Position
        {
            internal InnerWithIndent()
                : base(hasLineBreak: true, anchorIndent: true, indent: true) { }

            protected override Position Combine(Position other)
                => other == Right? this : base.Combine(other);
        }

        internal sealed class Inner : Position
        {
            internal Inner()
                : base(hasLineBreak: true, anchorIndent: true) { }

            protected override Position Combine(Position other)
                => other == InnerRight? other 
                    :other == Right? LineBreak
                    : base.Combine(other);
        }

        internal sealed class Begin : Position
        {
            protected override Position Combine(Position other) => this;
        }

        internal sealed class End : Position
        {
            internal End(bool hasLineBreak)
                : base(hasLineBreak: hasLineBreak) { }

            protected override Position Combine(Position other) => this;
        }

        internal sealed class Simple : Position
        {
            public Simple
            (
                string tag,
                bool hasLineBreak = false
                , bool indent = false
                , bool anchorIndent = false
                , bool forceLineBreak = false
                , bool hasAdditionalLineBreak = false
            )
                : base(tag, hasLineBreak, indent, anchorIndent, forceLineBreak, hasAdditionalLineBreak) { }
        }
    }

    internal static readonly Position Begin = new Classes.Begin();
    internal static readonly Position LineBreak = new Classes.LineBreak();
    internal static readonly Position IndentAll = new Classes.IndentAll();
    internal static readonly Position IndentAllAndForceLineSplit = new Classes.IndentAllAndForceLineSplit();
    internal static readonly Position BeforeToken = new Classes.BeforeToken();
    internal static readonly Position AfterListToken = new Classes.AfterListToken();

    internal static readonly Position AfterListTokenWithAdditionalLineBreak
        = new Classes.AfterListTokenWithAdditionalLineBreak();

    internal static readonly Position AfterColonToken = new Classes.AfterColonToken();
    internal static readonly Position Left = new Classes.Left();
    internal static readonly Position InnerLeft = new Classes.InnerLeft();
    internal static readonly Position Inner = new Classes.Inner();

    internal static readonly Position InnerWithAdditionalLineBreak
        = new Classes.InnerWithAdditionalLineBreak();

    internal static readonly Position InnerRight = new Classes.InnerRight();
    internal static readonly Position Right = new Classes.Simple("Right");

    internal static readonly Dictionary<bool, Position> End
        = new()
        {
            [true] = new Classes.End(true)
            , [false] = new Classes.End(false)
        };

    internal static readonly Position Function = new Classes.Simple("Function");

    static readonly Position InnerWithIndent = new Classes.InnerWithIndent();

    static readonly Position LineBreakAndIndentAndForceLineBreak
        = new Classes.Simple("LineBreakAndIndentAndForceLineBreak", true, true, forceLineBreak: true);

    static readonly Position LineBreakAndIndent = new Classes.Simple("LineBreakAndIndent", true, true);
    static readonly Position LeftAfterColonToken = new Classes.Simple("LeftAfterColonToken", true);

    static readonly Position InnerLeftSimple
        = new Classes.Simple("InnerLeftSimple", true, true, forceLineBreak: true);

    static int NextObjectId;

    [UsedImplicitly]
    internal readonly string Tag;

    internal readonly bool Indent;
    internal readonly bool AnchorIndent;
    internal readonly bool ForceLineBreak;
    readonly bool HasAdditionalLineBreak;
    readonly bool HasLineBreak;

    Position
    (
        string tag = null,
        bool hasLineBreak = false
        , bool indent = false
        , bool anchorIndent = false
        , bool forceLineBreak = false
        , bool hasAdditionalLineBreak = false
    )
        : base(NextObjectId++)
    {
        Tag = tag ?? GetType().Name;
        Indent = indent;
        AnchorIndent = anchorIndent;
        HasLineBreak = hasLineBreak;
        ForceLineBreak = forceLineBreak;
        HasAdditionalLineBreak = hasAdditionalLineBreak;
    }

    protected virtual Position Combine(Position other)
    {
        NotImplementedMethod(other);
        return default;
    }

    protected override string GetNodeDump()
        => $"{Tag}({LineBreakCount}" +
            $"{(ForceLineBreak? "!" : "")}" +
            $"{(Indent? ">" : "")}{(AnchorIndent? "-" : "")})";

    internal int LineBreakCount
        => HasLineBreak
            ? HasAdditionalLineBreak
                ? 2
                : 1
            : 0;

    public static Position operator +(Position first, Position other)
        => first == null
            ? other
            : other == null
                ? first
                : first.Combine(other);
}