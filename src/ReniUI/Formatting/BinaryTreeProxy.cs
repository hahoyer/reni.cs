using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Helper;
using Reni.TokenClasses;

namespace ReniUI.Formatting;

sealed class BinaryTreeProxy : TreeWithParentExtended<BinaryTreeProxy, BinaryTree>, ValueCache.IContainer
{
    [DisableDump]
    readonly Configuration Configuration;

    [EnableDump(Order = 2)]
    [EnableDumpExcept(null)]
    PositionParent PositionParent;

    BinaryTreeProxy(BinaryTree flatItem, Configuration configuration, BinaryTreeProxy parent)
        : base(flatItem, parent)
    {
        Configuration = configuration;
        StopByObjectIds();
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    ValueCache ValueCache.IContainer.Cache { get; } = new();

    protected override BinaryTreeProxy Create
        (BinaryTree child) => child == null? null : new(child, Configuration, this);

    [EnableDump(Order = -3)]
    string MainPosition
        => FlatItem.SourcePart.GetDumpAroundCurrent() + " " + FlatItem.TokenClass.GetType().PrettyName();

    [EnableDump(Order = 3)]
    [EnableDumpExcept(false)]
    internal bool IsLineSplit
    {
        get
        {
            if(FlatItem.TokenClass is ILeftBracket)
                return Parent.IsLineSplit;
            return HasAlreadyLineBreakOrIsTooLong || ForceLineSplit;
        }
    }

    [EnableDump(Order = 3)]
    [EnableDumpExcept(false)]
    bool ForceLineSplit => PositionParent != null && PositionParent.ForceLineBreak;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    bool HasAlreadyLineBreakOrIsTooLong
    {
        get
        {
            if(FlatItem == null)
                return false;
            var lineLength = FlatItem.GetFlatLength(Configuration.EmptyLineLimit != 0);
            return lineLength == null || lineLength > Configuration.MaxLineLength;
        }
    }

    [DisableDump]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal ISourcePartEdit[] Edits => GetEdits();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    ISourcePartEdit[] ChildrenEdits
    {
        get
        {
            StopByObjectIds();
            return DirectChildren
                .Where(item => item != null)
                .SelectMany(item => item.Edits)
                .ToArray();
        }
    }

    [EnableDump(Order = 4)]
    [EnableDumpExcept(null)]
    BinaryTreeProxy Left => DirectChildren[0];

    [EnableDump(Order = 5)]
    [EnableDumpExcept(null)]
    BinaryTreeProxy Right => DirectChildren[1];

    ISourcePartEdit[] AnchorEdits
        => FlatItem
            .GetWhiteSpaceEdits(Configuration, LineBreakCount, Token.LogDump())
            .Indent(PositionParent is { AnchorIndent: true }? 1 : 0)
            .ToArray();

    SourcePart Token => FlatItem.FullToken;

    int LineBreakCount => PositionParent?.LineBreakCount ?? 0;

    void SetPosition(PositionParent positionParent)
        => PositionParent = PositionParent == null? positionParent : PositionParent.Combine(positionParent);

    internal void SetupPositions()
    {
        if(!IsLineSplit)
            return;

        SetupMainPositions();
        Left?.SetupPositions();
        Right?.SetupPositions();
    }

    void SetupMainPositions()
    {
        switch(FlatItem.TokenClass)
        {
            case LeftParenthesis:
            case BeginOfText:
                return;
            case EndOfText:
                (Left.FlatItem.TokenClass is BeginOfText).Assert();
                Left.RightNeighbor.SetPosition(new PositionParent.Begin(this));
                SetPosition(new PositionParent.End(this
                    , Configuration.LineBreakAtEndOfText ?? FlatItem.WhiteSpaces.HasLineBreak));
                return;
            case Definable:
            {
                if(Parent.FlatItem.TokenClass is Definable)
                    return;

                var chain = this.Chain(item => item.Left).Reverse().ToArray();
                for(var index = 1; index < chain.Length; index++)
                {
                    var formerItem = chain[index - 1];
                    var hasAdditionalLineSplit = Configuration.AdditionalLineBreaksForMultilineItems &&
                        formerItem.Right != null &&
                        formerItem.Right.IsLineSplit;

                    if(hasAdditionalLineSplit && formerItem.PositionParent is PositionParent.Inner inner)
                        inner.HasAdditionalLineBreak = true;

                    var positionParent = new PositionParent.Inner(this);
                    if(hasAdditionalLineSplit)
                        positionParent.HasAdditionalLineBreak = true;
                    chain[index].SetPosition(positionParent);
                }

                return;
            }

            case RightParenthesis:
                if(Left?.Right?.FlatItem.TokenClass is IRightBracket)
                    return; //Bracket cluster

                Left.AssertIsNotNull();
                Left.SetPosition(new PositionParent.Left(this));
                if(Left.Right != null)
                {
                    Left.RightNeighbor.SetPosition(new PositionParent.InnerLeft(this));
                    Left.Right.SetPosition(new PositionParent.IndentAllAndForceLineSplit(this));
                    SetPosition(new PositionParent.InnerRight(this));
                }

                RightNeighbor.SetPosition(new PositionParent.Right(this));
                return;

            case List:
            {
                if(Parent.FlatItem.TokenClass == FlatItem.TokenClass)
                    return;
                var chain = this
                    .Chain(item => item.FlatItem.TokenClass == FlatItem.TokenClass? item.Right : null).ToArray();

                for(var index = 0; index < chain.Length; index++)
                {
                    var node = chain[index];

                    var item = node.FlatItem.TokenClass == FlatItem.TokenClass? node.Left : node;

                    var hasAdditionalLineSplit = Configuration.AdditionalLineBreaksForMultilineItems &&
                        item.IsLineSplit;

                    if(Configuration.LineBreaksBeforeListToken)
                        node.SetPosition(new PositionParent.BeforeToken(this));
                    else
                    {
                        if(node.FlatItem.TokenClass == FlatItem.TokenClass)
                        {
                            var positionParent = new PositionParent.AfterListToken(this);
                            node.RightNeighbor.SetPosition(positionParent);

                            if(hasAdditionalLineSplit)
                                positionParent.HasAdditionalLineBreak = true;
                        }

                        if(hasAdditionalLineSplit && index > 0)
                            chain[index - 1].RightNeighbor.PositionParent.HasAdditionalLineBreak = true;
                    }
                }

                return;
            }
            case Colon:
                RightNeighbor.SetPosition(new PositionParent.AfterColonToken(this));
                return;
            case ElseToken:
            case ThenToken:
                SetPosition(new PositionParent.BeforeToken(this));
                if(Right is not { IsLineSplit: true })
                    return;

                RightNeighbor.SetPosition(new PositionParent.LineBreak(this));
                Right.SetPosition(new PositionParent.IndentAll(this));
                return;

            case Function:
                if(Left != null)
                {
                    (FlatItem.FullToken.NodeDump + " " + FlatItem.TokenClass.GetType().Name).Log();
                    Tracer.TraceBreak();
                    return;
                }

                if(!Right.IsLineSplit)
                    return;

                SetPosition(new PositionParent.Function(this));
                RightNeighbor.SetPosition(new PositionParent.LineBreak(this));
                Right.SetPosition(new PositionParent.IndentAll(this));
                return;
            case IssueTokenClass:
                SetPosition(new PositionParent.Left(this));
                if(Right != null)
                {
                    RightNeighbor.SetPosition(new PositionParent.InnerLeft(this));
                    Right.SetPosition(new PositionParent.IndentAllAndForceLineSplit(this));
                }

                return;


            default:
                (FlatItem.FullToken.NodeDump + " " + FlatItem.TokenClass.GetType().Name).Log();
                Tracer.TraceBreak();
                return;
        }
    }

    internal static BinaryTreeProxy Create(BinaryTree target, Configuration configuration)
        => new(target, configuration, null);

    ISourcePartEdit[] GetEdits()
        => T(AnchorEdits, ChildrenEdits)
            .ConcatMany()
            .Indent(PositionParent is { Indent : true }? 1 : 0)
            .ToArray();
}