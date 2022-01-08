using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using hw.DebugFormatter;
using JetBrains.Annotations;

namespace ReniUI.Formatting;

abstract class Position : DumpableObject
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal static class Flag
    {
        internal enum AnchorIndent { False, True }
        internal enum ForceLineBreak { False, True }
        internal enum Indent { False, True }
        internal enum LineBreaks { False = 0, Simple = 1, Extended = 2 }
    }

    static class Classes
    {
        internal sealed class LineBreak : Position
        {
            internal LineBreak()
                : base(lineBreaks: Flag.LineBreaks.Simple) { }

            protected override Position Combine(Position other)
                => other == LeftBracketOuter
                    ? this
                    : other == IndentAll
                        ? LineBreakAndIndent
                        : base.Combine(other);
        }

        internal sealed class IndentAll : Position
        {
            internal IndentAll()
                : base(indent: Flag.Indent.True) { }

            protected override Position Combine(Position other)
                => other == BeforeToken
                    ? LineBreakAndIndent
                    : other == RightBracketInner
                        ? other
                        : other == LeftCoupling
                            ? LeftCouplingWithIndent
                            : base.Combine(other);
        }

        internal sealed class IndentAllAndForceLineSplit : Position
        {
            internal IndentAllAndForceLineSplit()
                : base(indent: Flag.Indent.True, forceLineBreak: Flag.ForceLineBreak.True) { }

            protected override Position Combine(Position other)
                => other == BeforeToken
                    ? LineBreakAndIndentAndForceLineBreak
                    : other == RightBracketInner
                        ? other
                        : base.Combine(other);
        }

        internal sealed class BeforeToken : Position
        {
            internal BeforeToken()
                : base(lineBreaks: Flag.LineBreaks.Simple) { }

            protected override Position Combine(Position other)
                => other == RightBracketOuter? this : base.Combine(other);
        }

        internal sealed class AfterListToken : Position
        {
            internal AfterListToken()
                : base(lineBreaks: Flag.LineBreaks.Simple) { }

            protected override Position Combine(Position other)
                => other == LeftBracketOuter? this : base.Combine(other);
        }

        internal sealed class AfterListTokenWithAdditionalLineBreak : Position
        {
            internal AfterListTokenWithAdditionalLineBreak()
                : base(lineBreaks: Flag.LineBreaks.Extended) { }

            protected override Position Combine(Position other)
                => other == LeftBracketOuter? this : base.Combine(other);
        }

        internal sealed class AfterColonToken : Position
        {
            internal AfterColonToken()
                : base(lineBreaks: Flag.LineBreaks.Simple) { }

            protected override Position Combine(Position other)
                => other == LeftBracketOuter
                    ? LeftAfterColonToken
                    : other == Function
                        ? null
                        : other == IndentAll
                            ? AfterColonTokenWithIndentAll
                            : base.Combine(other);
        }

        internal sealed class LeftBracketOuter : Position
        {
            protected override Position Combine(Position other)
                => other is LeftBracketInner? null : base.Combine(other);
        }

        internal sealed class LeftBracketInner : Position
        {
            internal LeftBracketInner()
                : base(lineBreaks: Flag.LineBreaks.Simple) { }

            protected override Position Combine(Position other)
                => other == LeftBracketOuter
                    ? null
                    : other == IndentAllAndForceLineSplit
                        ? InnerLeftWithIndentAllAndForceLineSplit
                        : other == IndentAll
                            ? InnerLeftWithIndentAll
                            : base.Combine(other);
        }

        internal sealed class RightBracketInner : Position
        {
            internal RightBracketInner()
                : base(lineBreaks: Flag.LineBreaks.Simple) { }

            protected override Position Combine(Position other)
                => other == RightBracketOuter ||
                    other == AfterListToken ||
                    other == AfterListTokenWithAdditionalLineBreak
                        ? this
                        : base.Combine(other);
        }

        internal sealed class Begin : Position
        {
            protected override Position Combine(Position other) => this;
        }

        internal sealed class End : Position
        {
            internal End(Flag.LineBreaks lineBreaks)
                : base(lineBreaks: lineBreaks) { }

            protected override Position Combine(Position other) => this;
        }

        internal sealed class LeftCouplingWithIndent : Position
        {
            internal LeftCouplingWithIndent()
                : base(lineBreaks: Flag.LineBreaks.Simple, indent: Flag.Indent.True) { }

            protected override Position Combine(Position other)
                => other == RightBracketOuter? this : base.Combine(other);
        }

        internal sealed class LeftCoupling : Position
        {
            internal LeftCoupling()
                : base(lineBreaks: Flag.LineBreaks.Simple) { }

            protected override Position Combine(Position other)
                => other == RightBracketOuter? this : base.Combine(other);
        }

        internal sealed class RightCoupling : Position
        {
            internal RightCoupling()
                : base(lineBreaks: Flag.LineBreaks.Simple) { }

            protected override Position Combine(Position other)
                => other == LeftBracketOuter? this : base.Combine(other);
        }

        internal sealed class AfterColonTokenWithIndentAll : Position
        {
            internal AfterColonTokenWithIndentAll()
                : base(lineBreaks: Flag.LineBreaks.Simple
                    , indent: Flag.Indent.True
                    , anchorIndent: Flag.AnchorIndent.True) { }

            protected override Position Combine(Position other)
                => other == Function? other : base.Combine(other);
        }

        internal sealed class Simple : Position
        {
            public Simple
            (
                string tag,
                Flag.LineBreaks lineBreaks = default
                , Flag.Indent indent = default
                , Flag.AnchorIndent anchorIndent = default
                , Flag.ForceLineBreak forceLineBreak = default
            )
                : base(tag, lineBreaks, indent, anchorIndent, forceLineBreak) { }
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

    internal static readonly Position LeftBracketOuter = new Classes.LeftBracketOuter();
    internal static readonly Position LeftBracketInner = new Classes.LeftBracketInner();
    internal static readonly Position RightBracketInner = new Classes.RightBracketInner();
    internal static readonly Position RightBracketOuter = new Classes.Simple("RightBracketOuter");

    internal static readonly Dictionary<bool, Position> End
        = new()
        {
            [true] = new Classes.End(Flag.LineBreaks.Simple)
            , [false] = new Classes.End(Flag.LineBreaks.False)
        };

    internal static readonly Position Function = new Classes.Simple("Function");

    internal static readonly Position LeftCoupling = new Classes.LeftCoupling();
    internal static readonly Position RightCoupling = new Classes.RightCoupling();

    static readonly Position LeftCouplingWithIndent = new Classes.LeftCouplingWithIndent();

    static readonly Position LineBreakAndIndentAndForceLineBreak
        = new Classes.Simple("LineBreakAndIndentAndForceLineBreak"
            , Flag.LineBreaks.Simple
            , Flag.Indent.True
            , forceLineBreak: Flag.ForceLineBreak.True);

    static readonly Position LineBreakAndIndent
        = new Classes.Simple("LineBreakAndIndent", Flag.LineBreaks.Simple, Flag.Indent.True);

    static readonly Position LeftAfterColonToken = new Classes.Simple("LeftAfterColonToken", Flag.LineBreaks.Simple);
    static readonly Position AfterColonTokenWithIndentAll = new Classes.AfterColonTokenWithIndentAll();

    static readonly Position InnerLeftWithIndentAllAndForceLineSplit
        = new Classes.Simple("InnerLeftWithIndentAllAndForceLineSplit"
            , Flag.LineBreaks.Simple
            , Flag.Indent.True
            , forceLineBreak: Flag.ForceLineBreak.True);

    static readonly Position InnerLeftWithIndentAll
        = new Classes.Simple("InnerLeftWithIndentAll"
            , Flag.LineBreaks.Simple
            , Flag.Indent.True);

    static int NextObjectId;

    [UsedImplicitly]
    internal readonly string Tag;

    internal readonly Flag.Indent Indent;
    internal readonly Flag.AnchorIndent AnchorIndent;
    internal readonly Flag.ForceLineBreak ForceLineBreak;
    internal readonly Flag.LineBreaks LineBreaks;

    Position
    (
        string tag = null,
        Flag.LineBreaks lineBreaks = default
        , Flag.Indent indent = default
        , Flag.AnchorIndent anchorIndent = default
        , Flag.ForceLineBreak forceLineBreak = default
    )
        : base(NextObjectId++)
    {
        Tag = tag ?? GetType().Name;

        LineBreaks = lineBreaks;
        Indent = indent;
        AnchorIndent = anchorIndent;
        ForceLineBreak = forceLineBreak;
    }

    protected virtual Position Combine(Position other)
    {
        NotImplementedMethod(other);
        return default;
    }

    protected override string GetNodeDump()
        => $"{Tag}({LineBreaks:d}" +
            $"{(ForceLineBreak == default? "" : "!")}" +
            $"{(Indent == default? "" : ">")}{(AnchorIndent == default? "" : "-")})";

    public static Position operator +(Position first, Position other)
        => first == null
            ? other
            : other == null
                ? first
                : first.Combine(other);
}