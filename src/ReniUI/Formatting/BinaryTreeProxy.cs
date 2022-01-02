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
    Position LineBreakBehaviour;

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

    [EnableDump(Order = 3.1)]
    [EnableDumpExcept(false)]
    bool ForceLineSplit => LineBreakBehaviour != null && LineBreakBehaviour.ForceLineBreak;

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
            .Indent(LineBreakBehaviour is { AnchorIndent: true }? 1 : 0)
            .ToArray();

    SourcePart Token => FlatItem.FullToken;

    int LineBreakCount => LineBreakBehaviour?.LineBreakCount ?? 0;

    void SetPosition(Position position)
        => LineBreakBehaviour = LineBreakBehaviour == null? position : LineBreakBehaviour.Combine(position);

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
                Left.RightNeighbor.SetPosition(Position.Begin.Instance);
                SetPosition(Position.End.CreateInstance(Configuration.LineBreakAtEndOfText ??
                    FlatItem.WhiteSpaces.HasLineBreak));
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

                    if(hasAdditionalLineSplit && formerItem.LineBreakBehaviour is Position.Inner)
                        formerItem.LineBreakBehaviour = Position.InnerWithAdditionalLineBreak.Instance;

                    var positionParent = hasAdditionalLineSplit
                        ? Position.InnerWithAdditionalLineBreak.Instance
                        : Position.Inner.Instance;
                    chain[index].SetPosition(positionParent);
                }

                return;
            }

            case RightParenthesis:
                if(Left?.Right?.FlatItem.TokenClass is IRightBracket)
                    return; //Bracket cluster

                Left.AssertIsNotNull();
                Left.SetPosition(Position.Left.Instance);
                if(Left.Right != null)
                {
                    Left.RightNeighbor.SetPosition(Position.InnerLeft.Instance);
                    Left.Right.SetPosition(Position.IndentAllAndForceLineSplit.Instance);
                    SetPosition(Position.InnerRight.Instance);
                }

                RightNeighbor.SetPosition(Position.Right.Instance);
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

                    var hasAdditionalLineSplit
                        = Configuration.AdditionalLineBreaksForMultilineItems && item.IsLineSplit;

                    if(Configuration.LineBreaksBeforeListToken)
                        node.SetPosition(Position.BeforeToken.Instance);
                    else
                    {
                        if(node.FlatItem.TokenClass == FlatItem.TokenClass)
                        {
                            var positionParent = hasAdditionalLineSplit
                                ? Position.AfterListTokenWithAdditionalLineBreak.Instance
                                : Position.AfterListToken.Instance;
                            node.RightNeighbor.SetPosition(positionParent);
                        }

                        if(hasAdditionalLineSplit && index > 0)
                        {
                            var neighbor = chain[index - 1].RightNeighbor;
                            (neighbor.LineBreakBehaviour is Position.AfterListToken
                                or Position.AfterListTokenWithAdditionalLineBreak).Assert();
                            neighbor.LineBreakBehaviour = Position.AfterListTokenWithAdditionalLineBreak.Instance;
                        }
                    }
                }

                return;
            }
            case Colon:
                RightNeighbor.SetPosition(Position.AfterColonToken.Instance);
                return;
            case ElseToken:
            case ThenToken:
                SetPosition(Position.BeforeToken.Instance);
                if(Right is not { IsLineSplit: true })
                    return;

                RightNeighbor.SetPosition(Position.LineBreak.Instance);
                Right.SetPosition(Position.IndentAll.Instance);
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

                SetPosition(Position.Function.Instance);
                RightNeighbor.SetPosition(Position.LineBreak.Instance);
                Right.SetPosition(Position.IndentAll.Instance);
                return;
            case IssueTokenClass:
                SetPosition(Position.Left.Instance);
                if(Right != null)
                {
                    RightNeighbor.SetPosition(Position.InnerLeft.Instance);
                    Right.SetPosition(Position.IndentAllAndForceLineSplit.Instance);
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
            .Indent(LineBreakBehaviour is { Indent : true }? 1 : 0)
            .ToArray();
}