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

        protected override void SetupPositions(BinaryTreeProxy target) => NotImplementedMethod(target);

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

        protected override void SetupPositions(BinaryTreeProxy target) => NotImplementedMethod(target);

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
    }

    sealed class FlatChildCompound : FlatCompound
    {
        internal static readonly Formatter Instance = new FlatChildCompound();

        protected override void SetupPositions(BinaryTreeProxy target)
        {
            var (left, list, right) = Split1N1(target);
            (left.FlatItem.TokenClass is ILeftBracket).Assert();
            (right.FlatItem.TokenClass is IRightBracket).Assert();
            list.All(item=>item.FlatItem.TokenClass is List).Assert();
            (((CompoundSyntax)target.FlatItem.Syntax).Statements.Length == list.Length).Assert();

            left.SetPosition(Position.Left);
            left.RightNeighbor.SetPosition(Position.InnerLeft);
            left.RightNeighbor.SetPosition(Position.IndentAll);
            (!target.Configuration.LineBreaksBeforeListToken).Assert();

            var configuration = target.Configuration;

            for(var index = 0; index < list.Length; index++)
            {
                var node = list[index];
                var item = node.FlatItem.TokenClass == target.FlatItem.TokenClass? node.Left : node;

                var hasAdditionalLineSplit
                    = configuration.AdditionalLineBreaksForMultilineItems && item.IsLineSplit;

                if(configuration.LineBreaksBeforeListToken)
                    node.SetPosition(Position.BeforeToken);
                else
                {
                    if(node.FlatItem.TokenClass == target.FlatItem.TokenClass)
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

            right.SetPosition(Position.InnerRight);
            right.RightNeighbor.SetPosition(Position.Right);
        }
    }

    sealed class FlatRootCompound : FlatCompound
    {
        internal static readonly Formatter Instance = new FlatRootCompound();

        internal override(BinaryTree begin, BinaryTree end) GetFrameAnchors(IItem target)
            => (target.Anchor.Items.First(), target.Anchor.Items.Last());

        protected override void SetupPositions(BinaryTreeProxy target)
        {
            var (begin, end) = Split2(target);

            begin.RightNeighbor.SetPosition(Position.Begin);
            var hasLineBreak = target.Configuration.LineBreakAtEndOfText ?? target.FlatItem.WhiteSpaces.HasLineBreak;
            end.SetPosition(Position.End[hasLineBreak]);
        }
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

    void BinaryTree.IFormatter.SetupPositions(BinaryTree.IPositionTarget target)
        => SetupPositions((BinaryTreeProxy)target);

    protected internal abstract IEnumerable<Child> GetChildren(IItem target);

    internal virtual(BinaryTree begin, BinaryTree end) GetFrameAnchors(IItem target) => (default, default);

    protected virtual void SetupPositions(BinaryTreeProxy target) => NotImplementedMethod(target);

    static(BinaryTreeProxy, BinaryTreeProxy ) Split2(BinaryTreeProxy target)
    {
        var anchors = target.FlatItem.Syntax.Anchor.Items.Select(target.Convert).ToArray();
        (anchors.Length == 2).Assert();
        return (anchors[0], anchors[1]);
    }

    static(BinaryTreeProxy, BinaryTreeProxy[], BinaryTreeProxy ) Split1N1(BinaryTreeProxy target)
    {
        var anchors = target.FlatItem.Syntax.Anchor.Items.Select(target.Convert).ToArray();
        (anchors.Length >= 2).Assert();

        return (anchors[0], anchors.Skip(1).Take(anchors.Length - 2).ToArray(), anchors.Last());
    }

    static Formatter Create([NotNull] BinaryTree target)
    {
        var syntax = target.Syntax;
        syntax.AssertIsNotNull(() => "Syntax link should be set.");

        if(target != syntax.MainAnchor)
            return null;

        switch(syntax)
        {
            case CompoundSyntax compound:
                return CreateCompound(compound);
            case ExpressionSyntax { Left: null, Right: null }:
            case InfixSyntax { Left: null, Right: null }:
            case PrefixSyntax { Right: null }:
            case SuffixSyntax { Left: null }:
            case TerminalSyntax:
            case DeclarerSyntax.NameSyntax:
            case DeclarerSyntax.TagSyntax:
            case EmptyList:
                return Terminal.Instance;

            case ExpressionSyntax:
            case InfixSyntax:
            case PrefixSyntax:
            case SuffixSyntax:
                return TrainWreck.Instance;
            case DeclarationSyntax:
                return Declaration.Instance;
            case CondSyntax:
                return Conditional.Instance;
            case FunctionSyntax:
                return Function.Instance;
            case DeclarerSyntax.IssueSyntax:
                return DeclarerIssue.Instance;

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