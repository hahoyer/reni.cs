using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    abstract class Formatter : DumpableObject
    {
        internal sealed class Child : DumpableObject
        {
            internal readonly BinaryTree PrefixAnchor;
            internal readonly Reni.SyntaxTree.Syntax FlatItem;
            internal readonly bool HasAdditionalIndent;
            internal readonly Formatter Formatter;

            public Child
            (
                BinaryTree prefixAnchor,
                Reni.SyntaxTree.Syntax flatItem,
                bool hasAdditionalIndent,
                bool checkForBracketLevel = true
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
            protected internal override IEnumerable<Child> GetChildren(Reni.SyntaxTree.Syntax target)
                => T(new Child(null, target, false, false));

            internal override(BinaryTree begin, BinaryTree end) GetFrameAnchors(Reni.SyntaxTree.Syntax target)
                => (target.LeftMostAnchor, target.Anchor.Items.First(item => item.TokenClass is RightParenthesis));

            internal override void SetupLineBreaks(Syntax target)
                => target.Anchors.End.EnsureLineBreaks(1);

            [DisableDump]
            internal override bool IsIndentRequired => true;
        }

        sealed class Terminal : Formatter
        {
            protected internal override IEnumerable<Child> GetChildren(Reni.SyntaxTree.Syntax target) => new Child[0];

            internal override(BinaryTree begin, BinaryTree end) GetFrameAnchors(Reni.SyntaxTree.Syntax target)
            {
                var center = target.MainAnchor.Chain(item => item.BracketKernel?.Center).Last();
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
            protected internal override IEnumerable<Child> GetChildren(Reni.SyntaxTree.Syntax target)
            {
                var wagons = GetWagons(target).ToArray();
                return wagons.Select(GetCargo);
            }

            internal override(BinaryTree begin, BinaryTree end) GetFrameAnchors(Reni.SyntaxTree.Syntax target)
                => (null, null);

            internal override void SetupLineBreaks(Syntax target)
                => SetupLineBreaksForChildren(target, putLineBreaksBeforePrefix: true);

            static Child GetCargo(Reni.SyntaxTree.Syntax node)
            {
                switch(node)
                {
                    case ExpressionSyntax target:
                        return new Child(node.Anchor.Main, target.Right, target.Left != null);
                    case SuffixSyntax target:
                        return new Child(node.Anchor.Main, null, target.Left != null);
                    case InfixSyntax target:
                        return new Child(node.Anchor.Main, target.Right, target.Left != null);
                    default:
                        return new Child(node.Anchor.Main, null, false);
                }
            }

            static IEnumerable<Reni.SyntaxTree.Syntax> GetWagons(Reni.SyntaxTree.Syntax syntax)
                => syntax.Chain(GetWagon).Reverse();

            static Reni.SyntaxTree.Syntax GetWagon(Reni.SyntaxTree.Syntax syntax)
                => syntax switch
                {
                    ExpressionSyntax expression => expression.Left
                    , SuffixSyntax suffix => suffix.Left
                    , InfixSyntax infix => infix.Left
                    , _ => null
                };
        }

        abstract class FlatCompound : Formatter
        {
            protected internal override IEnumerable<Child> GetChildren(Reni.SyntaxTree.Syntax target)
                => ((CompoundSyntax)target)
                    .Statements
                    .Select((node, index) => GetChild(target, node, index))
                    .ToArray();

            internal override(BinaryTree begin, BinaryTree end) GetFrameAnchors(Reni.SyntaxTree.Syntax target)
                => (default, default);

            static Child GetChild
                (Reni.SyntaxTree.Syntax target, IStatementSyntax node, int index)
                => new(index == 0? null : target.Anchor.Items[index], (Reni.SyntaxTree.Syntax)node, false);
        }

        abstract class CompoundWithCleanup : Formatter
        {
            protected internal override IEnumerable<Child> GetChildren
                (Reni.SyntaxTree.Syntax target) => throw new NotImplementedException();
        }

        sealed class Declaration : Formatter
        {
            protected internal override IEnumerable<Child> GetChildren(Reni.SyntaxTree.Syntax target)
            {
                var prefix = default(BinaryTree);
                foreach(var syntax in GetList(target))
                    if(syntax is DeclarationSyntax declaration)
                    {
                        yield return new Child(prefix, declaration.Declarer.Name, false);
                        prefix = declaration.MainAnchor;
                    }
                    else
                        yield return new Child(prefix, syntax, true);
            }

            internal override(BinaryTree begin, BinaryTree end) GetFrameAnchors(Reni.SyntaxTree.Syntax target)
                => (default, default);

            internal override void SetupLineBreaks(Syntax target)
                => SetupLineBreaksForChildren(target 
                    , putLineBreaksBeforePrefix: target.Configuration.LineBreaksBeforeDeclarationToken
                    , useAdditionalLineBreaksForMultilineItems:false
                    );

            static IEnumerable<Reni.SyntaxTree.Syntax> GetList(Reni.SyntaxTree.Syntax syntax)
            {
                var result = syntax.Chain(GetNext);
                foreach(var item in result)
                    AssertValidDeclarer(item);
                return result;
            }

            static void AssertValidDeclarer(Reni.SyntaxTree.Syntax target)
            {
                var declarationSyntax = target as DeclarationSyntax;
                if(declarationSyntax == null)
                    return;
                var declarer = declarationSyntax.Declarer;
                declarer.Name.AssertIsNotNull();
                (!declarer.Tags.Any()).Assert();
                declarer.Issue.AssertIsNull();
            }

            static Reni.SyntaxTree.Syntax GetNext
                (Reni.SyntaxTree.Syntax syntax) => (syntax as DeclarationSyntax)?.Value;
        }

        sealed class FlatChildCompound : FlatCompound
        {
            internal override void SetupLineBreaks(Syntax target)
                => SetupLineBreaksForChildren(target
                    , hasLineBreakAtTop: true
                    , putLineBreaksBeforePrefix: target.Configuration.LineBreaksBeforeListToken
                   );
        }

        sealed class FlatRootCompound : FlatCompound
        {
            internal override void SetupLineBreaks(Syntax target)
            {
                SetupLineBreaksForChildren(target 
                    , putLineBreaksBeforePrefix: target.Configuration.LineBreaksBeforeListToken
                    );

                if(target.Configuration.LineBreakAtEndOfText == null)
                    target.Anchors.End.EnsureLineBreaks(1);
            }

            public override void SetupUnconditionalLineBreaks(Syntax target)
            {
                if(target.Configuration.LineBreakAtEndOfText == true)
                    target.Anchors.End.EnsureLineBreaks(1);
            }

            internal override(BinaryTree begin, BinaryTree end) GetFrameAnchors(Reni.SyntaxTree.Syntax target)
                => (target.LeftMostAnchor, target.RightMostAnchor);
        }

        sealed class RootCompoundWithCleanup : CompoundWithCleanup
        {
            internal override(BinaryTree begin, BinaryTree end) GetFrameAnchors(Reni.SyntaxTree.Syntax target)
                => (target.LeftMostAnchor, target.RightMostAnchor);
        }

        sealed class ChildCompoundWithCleanup : CompoundWithCleanup { }

        protected internal abstract IEnumerable<Child> GetChildren(Reni.SyntaxTree.Syntax target);

        internal virtual(BinaryTree begin, BinaryTree end) GetFrameAnchors(Reni.SyntaxTree.Syntax target)
        {
            NotImplementedMethod(target);
            return default;
        }

        internal virtual void SetupLineBreaks(Syntax target) => NotImplementedMethod(target);

        public virtual void SetupUnconditionalLineBreaks(Syntax target) { }

        [DisableDump]
        internal virtual bool IsIndentRequired => false;

        static Formatter Create(Reni.SyntaxTree.Syntax flatItem, bool checkForBracketLevel)
        {
            if(checkForBracketLevel &&
                flatItem?.LeftMostAnchor.TokenClass is LeftParenthesis &&
                flatItem.RightMostAnchor.TokenClass is RightParenthesis)
                return new BracketLevel();

            switch(flatItem)
            {
                case CompoundSyntax compound:
                    return CreateCompound(compound);
                case ExpressionSyntax {Left: null, Right: null}:
                    return new Terminal();
                case ExpressionSyntax:
                    return new TrainWreck();
                case null:
                    return new Terminal();
                case DeclarerSyntax.NameSyntax:
                    return new Terminal();
                case DeclarationSyntax:
                    return new Declaration();
                case TerminalSyntax:
                    return new Terminal();

                default:
                    NotImplementedFunction(flatItem);
                    return default;
            }
        }

        static Formatter CreateCompound(CompoundSyntax compound)
        {
            if(compound.LeftMostAnchor.TokenClass is BeginOfText)
                return compound.CleanupSection == null
                    ? new FlatRootCompound()
                    : new RootCompoundWithCleanup();

            return compound.CleanupSection == null? new FlatChildCompound() : new ChildCompoundWithCleanup();
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