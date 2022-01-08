using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;
using Reni.Helper;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace ReniUI.Formatting;

abstract class Formatter : DumpableObject, BinaryTree.IFormatter
{
    internal sealed class Child : DumpableObject
    {
        internal readonly BinaryTree PrefixAnchor;
        internal readonly IItem FlatItem;
        internal readonly Flag.HasAdditionalIndent HasAdditionalIndent;
        internal readonly Formatter Formatter;

        internal Child
        (
            BinaryTree prefixAnchor
            , IItem flatItem
            , Flag.HasAdditionalIndent hasAdditionalIndent = default
            , Flag.IgnoreBracketLevel ignoreBracketLevel = default
            , Formatter forcedFormatter = default
        )
        {
            FlatItem = flatItem;
            HasAdditionalIndent = hasAdditionalIndent;
            PrefixAnchor = prefixAnchor;
            Formatter = Create(flatItem.Anchor.Main);
            StopByObjectIds();
        }
    }

    internal sealed class BracketLevel : Formatter
    {
        internal static readonly Formatter Instance = new BracketLevel();

        protected internal override IEnumerable<Child> GetChildren(IItem target)
        {
            if(target is EmptyList)
                return new Child[0];
            return T(new Child(null, target));
        }

        internal override(BinaryTree begin, BinaryTree end) GetFrameAnchors(IItem target)
            => (
                target.Anchor.Items.First()
                , target.Anchor.Items.FirstOrDefault(item => item.TokenClass is RightParenthesis)
            );

        protected override void SetupPositions(BinaryTreeProxy[] target) => NotImplementedMethod(target);

        [DisableDump]
        internal bool IsIndentRequired => true;
    }

    internal sealed class ListLevel : Formatter
    {
        internal static readonly Formatter Instance = new ListLevel();

        protected internal override IEnumerable<Child> GetChildren(IItem target)
        {
            if(target is EmptyList)
                return new Child[0];
            return T(new Child(null, target));
        }

        internal override(BinaryTree begin, BinaryTree end) GetFrameAnchors(IItem target)
            => (
                target.Anchor.Items.First()
                , target.Anchor.Items.FirstOrDefault(item => item.TokenClass is RightParenthesis)
            );

        protected override void SetupPositions(BinaryTreeProxy[] target) => NotImplementedMethod(target);

        [DisableDump]
        internal bool IsIndentRequired => true;
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal static class Flag
    {
        internal enum HasAdditionalIndent { False, True }
        internal enum IgnoreBracketLevel { False, True }
    }

    internal abstract class Modifier : DumpableObject
    {
        protected virtual Modifier Combine(Modifier other)
        {
            NotImplementedMethod(other);
            return default;
        }

        public static Modifier operator +(Modifier a, Modifier b)
            => a == null
                ? b
                : b == null
                    ? a
                    : a.Combine(b);
    }


    sealed class Terminal : Formatter
    {
        internal static readonly Formatter Instance = new Terminal();

        protected internal override IEnumerable<Child> GetChildren(IItem target) => new Child[0];

        internal override(BinaryTree begin, BinaryTree end) GetFrameAnchors(IItem target)
        {
            var center = target
                .Anchor
                .Main
                .Chain(item => item.BracketKernel?.Center)
                .Last();
            return (center, null);
        }
    }

    sealed class TrainWreck : Formatter
    {
        internal static readonly Formatter Instance = new TrainWreck();

        protected internal override IEnumerable<Child> GetChildren(IItem target)
        {
            var result = target
                .Chain(node => GetWagon(node, node != target))
                .Select(node => GetCargo(node, node != target))
                .Reverse()
                .ToArray();
            result.All(child => child.FlatItem != target).Assert();
            return result;
        }

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
            if(isTop || current.FlatItem.Formatter == null)
                return current.Left;
            return null;
        }

        static Syntax GetWagon(IItem flatItem, bool checkForBracketLevel)
        {
            if(checkForBracketLevel && HasBrackets(flatItem))
                return null;
            return flatItem switch
            {
                ExpressionSyntax expression => expression.Left
                , SuffixSyntax suffix => suffix.Left
                , InfixSyntax infix => infix.Left
                , _ => null
            };
        }

        static Child GetCargo(IItem node, bool checkForBracketLevel)
        {
            if(checkForBracketLevel && HasBrackets(node))
                return new(null, node);
            var prefixAnchor = node
                .Anchor
                .Items
                .First(item => item.TokenClass is not LeftParenthesis);
            switch(node)
            {
                case ExpressionSyntax target:
                    return new(prefixAnchor, target.Right
                        , target.Left == null? default : Flag.HasAdditionalIndent.True);
                case InfixSyntax target:
                    return new(prefixAnchor, target.Right
                        , target.Left == null? default : Flag.HasAdditionalIndent.True);
                case SuffixSyntax target:
                    return new(prefixAnchor, null, target.Left == null? default : Flag.HasAdditionalIndent.True);
                default:
                    return new(prefixAnchor, null);
            }
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

        protected internal override IEnumerable<Child> GetChildren(IItem target)
            => ((CompoundSyntax)target)
                .Statements
                .Select((node, index) => GetChild(target, node, index))
                .ToArray();


        static Child GetChild(IItem target, IStatementSyntax node, int index)
            => new(index == 0? null : target.Anchor.Items[index], (Syntax)node);
    }

    abstract class CompoundWithCleanup : Formatter
    {
        protected internal override IEnumerable<Child> GetChildren(IItem target)
            => throw new NotImplementedException();
    }

    sealed class Declaration : Formatter
    {
        internal static readonly Formatter Instance = new Declaration();

        protected internal override IEnumerable<Child> GetChildren(IItem target)
        {
            var declarationSyntax = (DeclarationSyntax)target;
            var declarer = declarationSyntax.Declarer;

            if(declarer.Issue != null)
                yield return new(null, declarer.Issue);

            foreach(var tag in declarer.Tags)
                yield return new(null, tag);

            if(declarer.Name != null)
                yield return new(null, declarer.Name);

            yield return new(target.Anchor.Main, declarationSyntax.Value, Flag.HasAdditionalIndent.True);
        }

        protected override void SetupPositions(BinaryTreeProxy[] target)
            => target.Single().RightNeighbor.SetPosition(Position.AfterColonToken);
    }

    sealed class FlatChildCompound : FlatCompound
    {
        internal static readonly Formatter Instance = new FlatChildCompound();
    }

    sealed class FlatRootCompound : FlatCompound
    {
        internal static readonly Formatter Instance = new FlatRootCompound();

        internal override(BinaryTree begin, BinaryTree end) GetFrameAnchors(IItem target)
            => (target.Anchor.Items.First(), target.Anchor.Items.Last());
    }

    sealed class RootCompoundWithCleanup : CompoundWithCleanup
    {
        internal static readonly Formatter Instance = new RootCompoundWithCleanup();

        protected internal override IEnumerable<Child> GetChildren(IItem target)
        {
            NotImplementedMethod(target, "Children", target.DirectChildren, nameof(target.Anchor), target.Anchor);
            return default;
        }

        internal override(BinaryTree begin, BinaryTree end) GetFrameAnchors
            (IItem target)
            => (target.Anchor.Items.First(), target.Anchor.Items.Last());
    }

    sealed class ChildCompoundWithCleanup : CompoundWithCleanup
    {
        internal static readonly Formatter Instance = new ChildCompoundWithCleanup();
    }

    sealed class Conditional : Formatter
    {
        internal static readonly Formatter Instance = new Conditional();

        protected internal override IEnumerable<Child> GetChildren(IItem target)
        {
            var anchorCount = target.Anchor.Items.Length;
            (anchorCount is 1 or 2).Assert();
            var children = new List<Child>
            {
                target.DirectChildren[0] == null? null : new(null, target.DirectChildren[0])
                , new(target.Anchor.Items[0], target.DirectChildren[1])
            };
            if(anchorCount == 2)
                children.Add(new(target.Anchor.Items[1], target.DirectChildren[2]));
            return children;
        }

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

        protected internal override IEnumerable<Child> GetChildren(IItem target)
        {
            if(target.DirectChildren[0] != null)
                yield return new(null, target.DirectChildren[0]);

            yield return new Child(target.Anchor.Items[0], target.DirectChildren[1]);
        }

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
        protected internal override IEnumerable<Child> GetChildren(IItem target) => throw new NotImplementedException();

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

    sealed class Special : Formatter
    {
        internal static readonly Formatter Instance = new Special();

        protected internal override IEnumerable<Child> GetChildren(IItem target)
            => target.SpecialAnchor.GetNodesFromLeftToRight().Select(GetChild);

        static Child GetChild(BinaryTree target) => new(target, null);
    }

    sealed class DeclarerIssue : Formatter
    {
        internal static readonly Formatter Instance = new DeclarerIssue();

        protected internal override IEnumerable<Child> GetChildren(IItem target)
            => target
                .Anchor
                .Items
                .Select(GetChild);

        static Child GetChild(BinaryTree target) => new(target, null);
    }

    void BinaryTree.IFormatter.SetupPositions(BinaryTree.IPositionTarget positionTarget)
    {
        var target = (BinaryTreeProxy)positionTarget;
        var configuration = target.Configuration;
        var (left, center, right) = SplitFrame(target);
        SetupFramePositions(left, right);
        SetupPositions(center);
    }

    protected internal abstract IEnumerable<Child> GetChildren(IItem target);

    internal virtual(BinaryTree begin, BinaryTree end) GetFrameAnchors(IItem target) => (default, default);

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
                return !HasBrackets(syntax) && target.Parent.Formatter == TrainWreck.Instance
                    ? null
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