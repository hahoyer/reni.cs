using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace ReniUI.Formatting;

abstract class Formatter : DumpableObject, BinaryTree.IFormatter
{
    sealed class TrainWreckPart : Formatter, ITrainWreckPart
    {
        internal static readonly Formatter Instance = new TrainWreckPart();
        protected override void SetupPositions(BinaryTreeProxy[] target) { }
    }

    interface ITrainWreckPart { }

    sealed class TrainWreck : Formatter, ITrainWreckPart
    {
        internal static readonly Formatter Instance = new TrainWreck();

        protected override void SetupPositions(BinaryTreeProxy[] target)
        {
            var items = GetWagons(target.Single());
            var isTop = true;
            foreach(var item in items)
            {
                SetPositionForItem(item, isTop);
                isTop = false;
            }
        }

        static void SetPositionForItem(BinaryTreeProxy item, bool isTop)
        {
            if(!isTop)
                item.SetPosition(Position.LeftCoupling);
            if(item.Right == null)
                return;
            if(!item.IsLineSplitRight)
                return;
            item.RightNeighbor.SetPosition(Position.RightCoupling);
            if(item.Right.IsLineSplit)
                return;
            item.Right.SetPosition(Position.IndentAll);
        }

        static BinaryTreeProxy[] GetWagons(BinaryTreeProxy target)
            => target
                .Chain(item => GetWagon(item, target == item))
                .Reverse()
                .ToArray();

        static BinaryTreeProxy GetWagon(BinaryTreeProxy current, bool isTop)
        {
            if(isTop || current.FlatItem.Formatter == TrainWreckPart.Instance)
                return current.Left;
            return null;
        }
    }

    abstract class FlatCompound : Formatter
    {
        protected override void SetupPositions(BinaryTreeProxy[] list)
        {
            if(!list.Any())
                return;

            var configuration = list.First().Configuration;
            list.All(item => item.FlatItem.TokenClass is List).Assert();
            var last = list.Last();
            if(last.Right != null)
                list = T(list, T(last.Right)).ConcatMany().ToArray();

            (((CompoundSyntax)list.First().FlatItem.Syntax).Statements.Length == list.Length).Assert();

            for(var index = 0; index < list.Length; index++)
            {
                var node = list[index];
                var item = node.FlatItem.TokenClass is List? node.Left : node;

                var hasAdditionalLineSplit
                    = configuration.AdditionalLineBreaksForMultilineItems && item.IsLineSplit;

                if(configuration.LineBreaksBeforeListToken)
                    node.SetPosition(Position.BeforeToken);
                else
                {
                    if(node.FlatItem.TokenClass is List)
                    {
                        var positionParent = hasAdditionalLineSplit
                            ? Position.AfterListTokenWithAdditionalLineBreak
                            : Position.AfterListToken;
                        node.RightNeighbor.SetPosition(positionParent);
                    }

                    if(hasAdditionalLineSplit && index > 0)
                    {
                        var neighbor = list[index - 1].RightNeighbor;
                        (neighbor.LineBreakBehaviour == Position.AfterListToken //
                                ||
                                neighbor.LineBreakBehaviour == Position.AfterListTokenWithAdditionalLineBreak)
                            .Assert();
                        neighbor.LineBreakBehaviour = Position.AfterListTokenWithAdditionalLineBreak;
                    }
                }
            }
        }
    }

    abstract class CompoundWithCleanup : Formatter { }

    sealed class Declaration : Formatter
    {
        internal static readonly Formatter Instance = new Declaration();

        protected override void SetupPositions(BinaryTreeProxy[] targets)
        {
            var target = targets.Single();
            target.RightNeighbor.SetPosition(Position.AfterColonToken);

            if(target.Left.IsLineSplit)
                NotImplementedMethod(target.Left);
        }
    }

    sealed class FlatChildCompound : FlatCompound
    {
        internal static readonly Formatter Instance = new FlatChildCompound();
    }

    sealed class FlatRootCompound : FlatCompound
    {
        internal static readonly Formatter Instance = new FlatRootCompound();
    }

    sealed class RootCompoundWithCleanup : CompoundWithCleanup
    {
        internal static readonly Formatter Instance = new RootCompoundWithCleanup();
    }

    sealed class ChildCompoundWithCleanup : CompoundWithCleanup
    {
        internal static readonly Formatter Instance = new ChildCompoundWithCleanup();
    }

    sealed class Conditional : Formatter
    {
        internal static readonly Formatter Instance = new Conditional();

        protected override void SetupPositions(BinaryTreeProxy[] target)
        {
            foreach(var item in target)
            {
                item.SetPosition(Position.BeforeToken);
                if(item.Right != null && item.Right.IsLineSplit)
                {
                    item.RightNeighbor.SetPosition(Position.LineBreak);
                    item.Right.SetPosition(Position.IndentAll);
                }
            }
        }
    }

    sealed class Function : Formatter
    {
        internal static readonly Formatter Instance = new Function();

        protected override void SetupPositions(BinaryTreeProxy[] targets)
        {
            var target = targets.Single();

            if(target.Left != null)
            {
                NotImplementedMethod(target);
                return;
            }

            if(!target.Right.IsLineSplit)
                return;

            target.SetPosition(Position.Function);
            target.RightNeighbor.SetPosition(Position.LineBreak);
        }
    }

    sealed class Issue : Formatter
    {
        internal static readonly Formatter Instance = new Issue();

        protected override void SetupPositions(BinaryTreeProxy[] targets)
        {
            var target = targets.First();
            target.SetPosition(Position.Left);
            if(target.Right == null)
                return;

            target.RightNeighbor.SetPosition(Position.InnerLeft);
            target.Right.SetPosition(Position.IndentAllAndForceLineSplit);
        }
    }

    sealed class DeclarerIssue : Formatter
    {
        internal static readonly Formatter Instance = new DeclarerIssue();
    }

    void BinaryTree.IFormatter.SetupPositions(BinaryTree.IPositionTarget positionTarget)
    {
        var target = (BinaryTreeProxy)positionTarget;
        var (left, center, right) = SplitFrame(target);
        SetupFramePositions(left, right);
        SetupPositions(center);
    }

    protected virtual void SetupPositions(BinaryTreeProxy[] target) => NotImplementedMethod(target, "", "");

    static void SetupFramePositions(BinaryTreeProxy[] left, BinaryTreeProxy[] right)
    {
        if(!left.Any())
            return;

        var configuration = left.First().Configuration;
        if(left.First().FlatItem.TokenClass is BeginOfText)
        {
            var begin = left.Single();
            var end = right.Single();

            begin.RightNeighbor.SetPosition(Position.Begin);
            var hasLineBreak = configuration.LineBreakAtEndOfText ?? end.FlatItem.WhiteSpaces.HasLineBreak;
            end.SetPosition(Position.End[hasLineBreak]);
            return;
        }

        left.First().SetPosition(Position.Left);
        left.Last().RightNeighbor.SetPosition(Position.InnerLeft);
        left.Last().Right.SetPosition(Position.IndentAll);
        (!configuration.LineBreaksBeforeListToken).Assert();

        right.First().SetPosition(Position.InnerRight);
        right.Last().RightNeighbor.SetPosition(Position.Right);
    }

    static(BinaryTreeProxy[], BinaryTreeProxy[], BinaryTreeProxy[]) SplitFrame(BinaryTreeProxy target)
    {
        var anchors = target.FlatItem.Syntax.Anchor.Items.Select(target.Convert).ToArray();
        var left = anchors.TakeWhile(item => item.FlatItem.TokenClass is ILeftBracket).ToArray();
        var center = anchors.Skip(left.Length).Take(anchors.Length - 2 * left.Length).ToArray();
        var right = anchors.Skip(left.Length + center.Length).ToArray();
        center.All(item => item.FlatItem.TokenClass is not ILeftBracket or IRightBracket).Assert();
        right.All(item => item.FlatItem.TokenClass is IRightBracket).Assert();

        return (left, center, right);
    }

    static Formatter Create([NotNull] BinaryTree target)
    {
        var syntax = target.Syntax;
        syntax.AssertIsNotNull(() => "Syntax link should be set.");

        if(target != syntax.MainAnchor)
            return null;

        if(target.Left == null && target.Right == null)
            return null;

        if(target.TokenClass is IssueTokenClass)
            return Issue.Instance;

        switch(syntax)
        {
            case CompoundSyntax compound:
                return CreateCompound(compound);

            case ExpressionSyntax:
            case InfixSyntax:
            case PrefixSyntax:
            case SuffixSyntax:
            case TerminalSyntax:
                return !HasBrackets(syntax) && target.Parent.Syntax.MainAnchor.Formatter is ITrainWreckPart
                    ? TrainWreckPart.Instance
                    : TrainWreck.Instance;
            case DeclarationSyntax:
                return Declaration.Instance;
            case CondSyntax:
                return Conditional.Instance;
            case FunctionSyntax:
                return Function.Instance;
            case DeclarerSyntax.IssueSyntax:
                return DeclarerIssue.Instance;
            case EmptyList:
            case DeclarerSyntax.NameSyntax:
            case DeclarerSyntax.TagSyntax:
                return null;

            default:
                NotImplementedFunction(syntax);
                return default;
        }
    }

    static bool HasBrackets(IItem flatItem)
        => flatItem?.Anchor?.Items?.FirstOrDefault()?.TokenClass is LeftParenthesis;

    static Formatter CreateCompound(CompoundSyntax compound)
    {
        if(compound.Anchor.Items.FirstOrDefault()?.TokenClass is BeginOfText)
            return compound.CleanupSection == null
                ? FlatRootCompound.Instance
                : RootCompoundWithCleanup.Instance;

        return compound.CleanupSection == null? FlatChildCompound.Instance : ChildCompoundWithCleanup.Instance;
    }

    internal static void SetFormatters(BinaryTree target)
    {
        if(target == null)
            return;

        target.Formatter = Create(target);
        SetFormatters(target.Left);
        SetFormatters(target.Right);
    }
}