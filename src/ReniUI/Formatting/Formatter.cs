using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Helper;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    abstract class Formatter : DumpableObject
    {
        internal sealed class Child : DumpableObject
        {
            internal readonly BinaryTree PrefixAnchor;
            internal readonly IItem FlatItem;
            internal readonly bool HasAdditionalIndent;
            internal readonly Formatter Formatter;

            internal Child
            (
                BinaryTree prefixAnchor
                , IItem flatItem
                , bool hasAdditionalIndent
                , bool checkForBracketLevel = true
                , Formatter forcedFormatter = null
            )
            {
                FlatItem = flatItem;
                HasAdditionalIndent = hasAdditionalIndent;
                PrefixAnchor = prefixAnchor;
                Formatter = Create(flatItem, checkForBracketLevel);
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
                return T(new Child(null, target, false, false));
            }

            internal override(BinaryTree begin, BinaryTree end) GetFrameAnchors(IItem target)
                => (
                    target.Anchor.Items.First()
                    , target.Anchor.Items.FirstOrDefault(item => item.TokenClass is RightParenthesis)
                );

            internal override void SetupLineBreaks(SyntaxTreeProxy target)
            {
                SetupLineBreaksForChildren(target, hasLineBreakAtTop: true);
                target.Anchors.End?.EnsureLineBreaks(1);
            }

            [DisableDump]
            internal override bool IsIndentRequired => true;
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

            internal override void SetupLineBreaks(SyntaxTreeProxy target)
            {
                // Why this was there?:
                // target.Anchors.Prefix.AssertIsNull(target.Dump);
                target.Anchors.Begin.AssertIsNotNull(target.Dump);
                target.Anchors.Begin.EnsureLineBreaks(1);
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

            internal override void SetupLineBreaks(SyntaxTreeProxy target)
                => SetupLineBreaksForChildren(target, putLineBreaksBeforePrefix: true);

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
                    return new(null, node, false);
                var prefixAnchor = node
                    .Anchor
                    .Items
                    .First(item => item.TokenClass is not LeftParenthesis);
                switch(node)
                {
                    case ExpressionSyntax target:
                        return new(prefixAnchor, target.Right, target.Left != null);
                    case InfixSyntax target:
                        return new(prefixAnchor, target.Right, target.Left != null);
                    case SuffixSyntax target:
                        return new(prefixAnchor, null, target.Left != null);
                    default:
                        return new(prefixAnchor, null, false);
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
                => new(index == 0? null : target.Anchor.Items[index], (Syntax)node, false);
        }

        abstract class CompoundWithCleanup : Formatter
        {
            protected internal override IEnumerable<Child> GetChildren(IItem target)
                => throw new NotImplementedException();

            internal override void SetupLineBreaks(SyntaxTreeProxy target) => throw new NotImplementedException();
        }

        sealed class Declaration : Formatter
        {
            internal static readonly Formatter Instance = new Declaration();

            protected internal override IEnumerable<Child> GetChildren(IItem target)
            {
                var declarationSyntax = (DeclarationSyntax)target;
                var declarer = declarationSyntax.Declarer;

                if(declarer.Issue != null)
                    yield return new(null, declarer.Issue, false);

                foreach(var tag in declarer.Tags)
                    yield return new(null, tag, false);

                if(declarer.Name != null)
                    yield return new(null, declarer.Name, false);

                yield return new(target.Anchor.Main, declarationSyntax.Value, true);
            }

            internal override void SetupLineBreaks(SyntaxTreeProxy target)
                => SetupLineBreaksForChildren(target
                    , putLineBreaksBeforePrefix: target.Configuration.LineBreaksBeforeDeclarationToken
                    , useAdditionalLineBreaksForMultilineItems: false
                );
        }

        sealed class FlatChildCompound : FlatCompound
        {
            internal static readonly Formatter Instance = new FlatChildCompound();

            internal override void SetupLineBreaks(SyntaxTreeProxy target)
                => SetupLineBreaksForChildren(target
                    , hasLineBreakAtTop: true
                    , putLineBreaksBeforePrefix: target.Configuration.LineBreaksBeforeListToken
                );
        }

        sealed class FlatRootCompound : FlatCompound
        {
            internal static readonly Formatter Instance = new FlatRootCompound();

            internal override void SetupLineBreaks(SyntaxTreeProxy target)
            {
                SetupLineBreaksForChildren(target
                    , putLineBreaksBeforePrefix: target.Configuration.LineBreaksBeforeListToken
                );

                if(target.Configuration.LineBreakAtEndOfText == null)
                    target.Anchors.End.EnsureLineBreaks(1);
            }

            internal override void SetupUnconditionalLineBreaks(SyntaxTreeProxy target)
            {
                if(target.Configuration.LineBreakAtEndOfText == true)
                    target.Anchors.End.EnsureLineBreaks(1);
            }

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
                    target.DirectChildren[0] == null? null : new(null, target.DirectChildren[0], false)
                    , new(target.Anchor.Items[0], target.DirectChildren[1], false)
                };
                if(anchorCount == 2)
                    children.Add(new(target.Anchor.Items[1], target.DirectChildren[2], false));
                return children;
            }

            internal override void SetupLineBreaks(SyntaxTreeProxy target)
            {
                var thenClause = target.Children[1];
                if(target.Children[0] != null)
                    thenClause.Anchors.Prefix.EnsureLineBreaks(1);
                if(thenClause.IsLineSplit)
                    thenClause.Anchors.Begin.EnsureLineBreaks(1);
                if(target.Children.Length == 2)
                    return;

                SetupLineBreaksForChildren(
                    target
                    , putLineBreaksBeforePrefix: true
                    , useAdditionalLineBreaksForMultilineItems: false
                );
            }
        }

        sealed class Function : Formatter
        {
            internal static readonly Formatter Instance = new Function();

            protected internal override IEnumerable<Child> GetChildren(IItem target)
            {
                if(target.DirectChildren[0] != null)
                    yield return new(null, target.DirectChildren[0], false);

                yield return new Child(target.Anchor.Items[0], target.DirectChildren[1], false);
            }

            internal override void SetupLineBreaks(SyntaxTreeProxy target)
            {
                if(target.Children.Length == 1)
                    return;

                NotImplementedMethod(target);
            }
        }

        sealed class Special : Formatter
        {
            internal static readonly Formatter Instance = new Special();

            internal override void SetupLineBreaks(SyntaxTreeProxy target) => throw new NotImplementedException();

            protected internal override IEnumerable<Child> GetChildren(IItem target)
                => target.SpecialAnchor.GetNodesFromLeftToRight().Select(GetChild);

            static Child GetChild(BinaryTree target) => new(target, null, false);
        }

        sealed class DeclarerIssue : Formatter
        {
            internal static readonly Formatter Instance = new DeclarerIssue();

            internal override void SetupLineBreaks(SyntaxTreeProxy target) => throw new NotImplementedException();

            protected internal override IEnumerable<Child> GetChildren(IItem target)
                => target
                    .Anchor
                    .Items
                    .Select(GetChild);

            static Child GetChild(BinaryTree target) => new(target, null, false);
        }

        protected internal abstract IEnumerable<Child> GetChildren(IItem target);

        internal abstract void SetupLineBreaks(SyntaxTreeProxy target);

        internal virtual(BinaryTree begin, BinaryTree end) GetFrameAnchors(IItem target) => (default, default);

        internal virtual void SetupUnconditionalLineBreaks(SyntaxTreeProxy target) { }

        [DisableDump]
        internal virtual bool IsIndentRequired => false;

        static Formatter Create(IItem flatItem, bool checkForBracketLevel)
        {
            if(flatItem?.SpecialAnchor != null)
                return Special.Instance;

            if(checkForBracketLevel && HasBrackets(flatItem))
                return BracketLevel.Instance;

            switch(flatItem)
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
                case null:
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
                    NotImplementedFunction(flatItem, checkForBracketLevel);
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

        static void SetupLineBreaksForChildren
        (
            SyntaxTreeProxy target
            , object _ = null
            , bool hasLineBreakAtTop = false
            , bool putLineBreaksBeforePrefix = false
            , bool? useAdditionalLineBreaksForMultilineItems = null
        )
        {
            var useAdditionalLineBreaks
                = useAdditionalLineBreaksForMultilineItems ??
                target.Configuration.AdditionalLineBreaksForMultilineItems;
            SyntaxTreeProxy leftNeighbor = null;
            foreach(var child in target.Children)
            {
                var count = leftNeighbor == null
                    ? hasLineBreakAtTop
                        ? 1
                        : 0
                    : useAdditionalLineBreaks && (leftNeighbor.IsLineSplit || child.IsLineSplit)
                        ? 2
                        : 1;

                if(count > 0)
                    child.EnsureLineBreaks(count, putLineBreaksBeforePrefix);
                leftNeighbor = child;
            }
        }
    }
}