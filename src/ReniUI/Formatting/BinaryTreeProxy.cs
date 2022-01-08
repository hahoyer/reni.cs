using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Helper;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace ReniUI.Formatting;

sealed class BinaryTreeProxy
    : TreeWithParentExtended<BinaryTreeProxy, BinaryTree>
        , ValueCache.IContainer
        , BinaryTree.IPositionTarget
{
    static readonly BinaryTreeProxy YetUnknown = new();

    [DisableDump]
    public readonly Configuration Configuration;

    [EnableDump(Order = 2)]
    [EnableDumpExcept(null)]
    internal Position LineBreakBehaviour;

    BinaryTreeProxy(BinaryTree target, Configuration configuration, BinaryTreeProxy parent)
        : base(target, parent)
    {
        Configuration = configuration;
        Formatter.SetFormatters(target);
        StopByObjectIds();
    }

    BinaryTreeProxy()
        : base(null, null) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    ValueCache ValueCache.IContainer.Cache { get; } = new();

    protected override string Dump(bool isRecursion) => FlatItem == null? "?" : base.Dump(isRecursion);

    protected override BinaryTreeProxy Create(BinaryTree child)
        => child == null? null : new(child, Configuration, this);

    [EnableDump(Order = -3)]
    string MainPosition
        => FlatItem.SourcePart.GetDumpAroundCurrent() + " " + FlatItem.TokenClass.GetType().PrettyName();

    [EnableDump(Order = 3)]
    [EnableDumpExcept(false)]
    internal bool IsLineSplit => GetIsLineSplit(FlatItem, Configuration, ForceLineSplit);

    [EnableDump(Order = 3.1)]
    [EnableDumpExcept(false)]
    bool ForceLineSplit => LineBreakBehaviour != null && LineBreakBehaviour.ForceLineBreak != default;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    bool HasAlreadyLineBreakOrIsTooLong => GetHasAlreadyLineBreakOrIsTooLong(FlatItem, Configuration);

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
    internal BinaryTreeProxy Left => DirectChildren == null? YetUnknown : DirectChildren[0];

    [EnableDump(Order = 5)]
    [EnableDumpExcept(null)]
    BinaryTreeProxy Right => DirectChildren == null? YetUnknown : DirectChildren[1];

    ISourcePartEdit[] AnchorEdits
        => FlatItem
            .GetWhiteSpaceEdits(Configuration, LineBreakCount, Token.LogDump())
            .Indent(LineBreakBehaviour == null || LineBreakBehaviour .AnchorIndent == default? 0 : 1)
            .ToArray();

    SourcePart Token => FlatItem.FullToken;

    int LineBreakCount => (int)(LineBreakBehaviour?.LineBreaks ?? Position.Flag.LineBreaks.False);

    static bool GetIsLineSplit(BinaryTree binaryTree, Configuration configuration, bool forceLineSplit)
    {
        if(binaryTree.TokenClass is ILeftBracket)
            return GetIsLineSplit(binaryTree.Parent, configuration, false);
        return GetHasAlreadyLineBreakOrIsTooLong(binaryTree, configuration) || forceLineSplit;
    }

    static bool GetHasAlreadyLineBreakOrIsTooLong(BinaryTree binaryTree, Configuration configuration)
    {
        if(binaryTree == null)
            return false;
        var lineLength = binaryTree.GetFlatLength(configuration.EmptyLineLimit != 0);
        return lineLength == null || lineLength > configuration.MaxLineLength;
    }

    internal void SetPosition(Position position)
        => LineBreakBehaviour += position;

    internal void SetupPositions()
    {
        if(!IsLineSplit)
            return;

        SetupMainPositions();
        Left?.SetupPositions();
        Right?.SetupPositions();
    }

    internal BinaryTreeProxy Convert(BinaryTree target)
        => FlatItem == target
            ? this
            : target.FullToken.End <= FlatItem.FullToken.Start
                ? Left?.Convert(target)
                : Right?.Convert(target);

    void SetupMainPositions()
    {
        FlatItem.Formatter?.SetupPositions(this);
        return;

        switch(FlatItem.Syntax)
        {
            case ExpressionSyntax { Left: null, Right: null }:
            case InfixSyntax { Left: null, Right: null }:
            case PrefixSyntax { Right: null }:
            case SuffixSyntax { Left: null }:
            case TerminalSyntax:
            case DeclarerSyntax.NameSyntax:
            case DeclarerSyntax.TagSyntax:
            case EmptyList:
                return;

            case ExpressionSyntax:
            case InfixSyntax:
            case PrefixSyntax:
            case SuffixSyntax:
                SetupTrainWreck();
                return;
        }

        switch(FlatItem.TokenClass)
        {
            case LeftParenthesis:
            case BeginOfText:
            case EndOfText:
                return;

            case RightParenthesis:
                if(Left?.Right?.FlatItem.TokenClass is IRightBracket)
                    return; //Bracket cluster

                Left.AssertIsNotNull();
                Left.SetPosition(Position.Left);
                if(Left.Right != null)
                {
                    Left.RightNeighbor.SetPosition(Position.InnerLeft);
                    Left.Right.SetPosition(Position.IndentAllAndForceLineSplit);
                    SetPosition(Position.InnerRight);
                }

                RightNeighbor.SetPosition(Position.Right);
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
                        node.SetPosition(Position.BeforeToken);
                    else
                    {
                        if(node.FlatItem.TokenClass == FlatItem.TokenClass)
                        {
                            var positionParent = hasAdditionalLineSplit
                                ? Position.AfterListTokenWithAdditionalLineBreak
                                : Position.AfterListToken;
                            node.RightNeighbor.SetPosition(positionParent);
                        }

                        if(hasAdditionalLineSplit && index > 0)
                        {
                            var neighbor = chain[index - 1].RightNeighbor;
                            (neighbor.LineBreakBehaviour == Position.AfterListToken //
                                    ||
                                    neighbor.LineBreakBehaviour == Position.AfterListTokenWithAdditionalLineBreak)
                                .Assert();
                            neighbor.LineBreakBehaviour = Position.AfterListTokenWithAdditionalLineBreak;
                        }
                    }
                }

                return;
            }
            case Colon:
                RightNeighbor.SetPosition(Position.AfterColonToken);
                return;
            case ElseToken:
            case ThenToken:
                SetPosition(Position.BeforeToken);
                if(Right is not { IsLineSplit: true })
                    return;

                RightNeighbor.SetPosition(Position.LineBreak);
                Right.SetPosition(Position.IndentAll);
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

                SetPosition(Position.Function);
                RightNeighbor.SetPosition(Position.LineBreak);
                return;
            case IssueTokenClass:
                SetPosition(Position.Left);
                if(Right != null)
                {
                    RightNeighbor.SetPosition(Position.InnerLeft);
                    Right.SetPosition(Position.IndentAllAndForceLineSplit);
                }

                return;
        }


        (FlatItem.FullToken.NodeDump + " " + FlatItem.TokenClass.GetType().Name).Log();
        Tracer.TraceBreak();
    }

    void SetupTrainWreck()
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

            if(hasAdditionalLineSplit && formerItem.LineBreakBehaviour == Position.Inner)
                formerItem.LineBreakBehaviour = Position.InnerWithAdditionalLineBreak;

            var positionParent = hasAdditionalLineSplit
                ? Position.InnerWithAdditionalLineBreak
                : Position.Inner;
            chain[index].SetPosition(positionParent);
        }
    }

    internal static BinaryTreeProxy Create(BinaryTree target, Configuration configuration)
        => new(target, configuration, null);

    ISourcePartEdit[] GetEdits()
        => T(AnchorEdits, ChildrenEdits)
            .ConcatMany()
            .Indent(LineBreakBehaviour == null || LineBreakBehaviour.Indent == default ? 0 : 1)
            .ToArray();
}