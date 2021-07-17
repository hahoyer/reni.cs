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
                BinaryTree prefixAnchor,
                IItem flatItem,
                bool hasAdditionalIndent,
                bool checkForBracketLevel = true,
                Formatter forcedFormatter = null
            )
            {
                FlatItem = flatItem;
                HasAdditionalIndent = hasAdditionalIndent;
                PrefixAnchor = prefixAnchor;
                Formatter = Create(flatItem, checkForBracketLevel);
            }
        }

        class BracketLevel : Formatter
        {
            internal static readonly Formatter Instance = new BracketLevel();

            protected internal override IEnumerable<Child> GetChildren(IItem target)
                => T(new Child(null, target, false, false));

            internal override(BinaryTree begin, BinaryTree end) GetFrameAnchors(IItem target)
                => (target.Anchor.Items.First()
                    , target.Anchor.Items.First(item => item.TokenClass is RightParenthesis));

            internal override void SetupLineBreaks(Syntax target)
            {
                SetupLineBreaksForChildren(target,hasLineBreakAtTop:true);
                target.Anchors.End.EnsureLineBreaks(1);
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
                var center = target.Anchor.Main.Chain(item => item.BracketKernel?.Center).Last();
                return (center, null);
            }

            internal override void SetupLineBreaks(Syntax target)
            {
                target.Anchors.Prefix.AssertIsNull();
                target.Anchors.Begin.AssertIsNotNull();
                target.Anchors.Begin.EnsureLineBreaks(1);
            }
        }

        sealed class TrainWreck : Formatter
        {
            internal static readonly Formatter Instance = new TrainWreck();

            protected internal override IEnumerable<Child> GetChildren(IItem target)
            {
                var wagons = GetWagons(target).ToArray();
                return wagons.Select(GetCargo);
            }

            internal override void SetupLineBreaks(Syntax target)
                => SetupLineBreaksForChildren(target, putLineBreaksBeforePrefix: true);

            static Child GetCargo(IItem node)
            {
                var prefixAnchor = node.Anchor.Items.First(item=> item.TokenClass is not LeftParenthesis);
                switch(node)
                {
                    case ExpressionSyntax target:
                        return new Child(prefixAnchor, target.Right, target.Left != null);
                    case InfixSyntax target:
                        return new Child(prefixAnchor, target.Right, target.Left != null);
                    case SuffixSyntax target:
                        return new Child(prefixAnchor, null, target.Left != null);
                    default:
                        return new Child(prefixAnchor, null, false);
                }
            }

            static Child CreateChild(IItem node, IItem left, IItem right)
            {
                var prefixAnchor = node.Anchor.Items.First();
                return new Child(prefixAnchor, right, left != null);
            }

            static IEnumerable<IItem> GetWagons(IItem syntax)
                => syntax.Chain(GetWagon).Reverse();

            static Reni.SyntaxTree.Syntax GetWagon(IItem flatItem)
            {
                if(HasBrackets(flatItem))
                    return null;
                return flatItem switch
                {
                    ExpressionSyntax expression => expression.Left
                    , SuffixSyntax suffix => suffix.Left
                    , InfixSyntax infix => infix.Left
                    , _ => null
                };
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
                => new(index == 0? null : target.Anchor.Items[index], (Reni.SyntaxTree.Syntax)node, false);
        }

        abstract class CompoundWithCleanup : Formatter
        {
            protected internal override IEnumerable<Child> GetChildren(IItem target)
                => throw new NotImplementedException();

            internal override void SetupLineBreaks(Syntax target) => throw new NotImplementedException();
        }

        sealed class Declaration : Formatter
        {
            internal static readonly Formatter Instance = new Declaration();

            protected internal override IEnumerable<Child> GetChildren(IItem target)
            {
                var prefix = default(BinaryTree);
                foreach(var syntax in GetList(target))
                    if(syntax is DeclarationSyntax declaration)
                    {
                        yield return new Child(prefix, declaration.Declarer, false);
                        prefix = declaration.MainAnchor;
                    }
                    else
                        yield return new Child(prefix, syntax, true);
            }

            internal override void SetupLineBreaks(Syntax target)
                => SetupLineBreaksForChildren(target
                    , putLineBreaksBeforePrefix: target.Configuration.LineBreaksBeforeDeclarationToken
                    , useAdditionalLineBreaksForMultilineItems: false
                );

            static IEnumerable<IItem> GetList(IItem target)
            {
                var result = target.Chain(GetNext);
                foreach(var item in result)
                    AssertValidDeclarer(item);
                return result;
            }

            static void AssertValidDeclarer(IItem target)
            {
                var declarationSyntax = target as DeclarationSyntax;
                if(declarationSyntax == null)
                    return;
                var declarer = declarationSyntax.Declarer;
                declarer.Issue.AssertIsNull();
            }

            static Reni.SyntaxTree.Syntax GetNext(IItem target)
                => (target as DeclarationSyntax)?.Value;
        }

        sealed class FlatChildCompound : FlatCompound
        {
            internal static readonly Formatter Instance = new FlatChildCompound();

            internal override void SetupLineBreaks(Syntax target)
                => SetupLineBreaksForChildren(target
                    , hasLineBreakAtTop: true
                    , putLineBreaksBeforePrefix: target.Configuration.LineBreaksBeforeListToken
                );
        }

        sealed class FlatRootCompound : FlatCompound
        {
            internal static readonly Formatter Instance = new FlatRootCompound();

            internal override void SetupLineBreaks(Syntax target)
            {
                SetupLineBreaksForChildren(target
                    , putLineBreaksBeforePrefix: target.Configuration.LineBreaksBeforeListToken
                );

                if(target.Configuration.LineBreakAtEndOfText == null)
                    target.Anchors.End.EnsureLineBreaks(1);
            }

            internal override void SetupUnconditionalLineBreaks(Syntax target)
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

            internal override(BinaryTree begin, BinaryTree end) GetFrameAnchors(IItem target)
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
                    new(null, target.DirectChildren[0], false)
                    , new(target.Anchor.Items[0], target.DirectChildren[1], false)
                };
                if(anchorCount == 2)
                    children.Add(new Child(target.Anchor.Items[1], target.DirectChildren[2], false));
                return children;
            }

            internal override void SetupLineBreaks(Syntax target)
            {
                var thenClause = target.Children[1];
                thenClause.Anchors.Prefix.EnsureLineBreaks(1);
                if(thenClause.IsLineSplit)
                    thenClause.Anchors.Begin.EnsureLineBreaks(1);
                if(target.Children.Length == 2)
                    return;
                NotImplementedMethod(target);
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
                    yield return new Child(null, target.DirectChildren[0], false);

                yield return new Child(target.Anchor.Items[0], target.DirectChildren[1], false);
            }

            internal override void SetupLineBreaks(Syntax target)
            {
                if(target.Children.Length == 1)
                    return;

                NotImplementedMethod(target);
            }
        }

        sealed class Special : Formatter
        {
            internal static readonly Formatter Instance = new Special();
            internal override void SetupLineBreaks(Syntax target) => throw new NotImplementedException();

            protected internal override IEnumerable<Child> GetChildren(IItem target)
                => target.SpecialAnchor.GetNodesFromLeftToRight().Select(GetChild);

            static Child GetChild(BinaryTree target) => new Child(target, null, false);
        }

        protected internal abstract IEnumerable<Child> GetChildren(IItem target);

        internal abstract void SetupLineBreaks(Syntax target);

        internal virtual(BinaryTree begin, BinaryTree end) GetFrameAnchors(IItem target)
            => (default, default);

        internal virtual void SetupUnconditionalLineBreaks(Syntax target) { }

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
                case ExpressionSyntax {Left: null, Right: null}:
                case InfixSyntax {Left: null, Right: null}:
                case PrefixSyntax {Right: null}:
                case SuffixSyntax {Left: null}:
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

                default:
                    NotImplementedFunction(flatItem, checkForBracketLevel);
                    return default;
            }
        }

        static bool HasBrackets(IItem flatItem)
            => flatItem?.Anchor.Items.FirstOrDefault()?.TokenClass is LeftParenthesis &&
                flatItem.Anchor.Items.LastOrDefault()?.TokenClass is RightParenthesis;

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
            Syntax target,
            object _ = null,
            bool hasLineBreakAtTop = false,
            bool putLineBreaksBeforePrefix = false,
            bool? useAdditionalLineBreaksForMultilineItems = null
        )
        {
            var useAdditionalLineBreaks
                = useAdditionalLineBreaksForMultilineItems ??
                target.Configuration.AdditionalLineBreaksForMultilineItems;
            Syntax leftNeighbor = null;
            foreach(var child in target.Children)
            {
                var count = leftNeighbor == null
                    ? hasLineBreakAtTop
                        ? 1
                        : 0
                    : useAdditionalLineBreaks && (leftNeighbor.IsLineSplit || child.IsLineSplit)
                        ? 2
                        : 1;

                child.EnsureLineBreaks(count, putLineBreaksBeforePrefix);
                leftNeighbor = child;
            }
        }
    }
}