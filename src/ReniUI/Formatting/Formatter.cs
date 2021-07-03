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
        internal sealed class Child
        {
            internal readonly Reni.SyntaxTree.Syntax FlatItem;

            public Child(Reni.SyntaxTree.Syntax flatItem) => FlatItem = flatItem;

            static BinaryTree GetHeadingAnchor(Reni.SyntaxTree.Syntax node)
            {
                var result = node.Anchor.Items.First();
                result.AssertIsNotNull();
                result.Left.AssertIsNull();
                return result;
            }
        }

        sealed class TrainWreck : Formatter
        {
            protected internal override BinaryTree[] GetFrameAnchors(Reni.SyntaxTree.Syntax main)
            {
                (main.Anchor.Items.Length <= 1).Assert();
                return new BinaryTree[0];
            }

            protected internal override IEnumerable<Child> GetChildren(Reni.SyntaxTree.Syntax target)
            {
                var wagons = GetWagons(target).ToArray();
                return wagons.Select(GetCargo);
            }

            [DisableDump]
            protected internal override bool IsIndentAtTailRequired => true;

            static Child GetCargo(Reni.SyntaxTree.Syntax node)
                => new(
                    node switch
                    {
                        ExpressionSyntax expression => expression.Right
                        , SuffixSyntax _ => null
                        , InfixSyntax infix => infix.Right
                        , _ => null
                    });

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

        sealed class Compound : Formatter
        {
            protected internal override BinaryTree[] GetFrameAnchors(Reni.SyntaxTree.Syntax main)
                => main
                    .Anchor
                    .Items
                    .Where(node => node.TokenClass is IRightBracket or ILeftBracket)
                    .ToArray();

            protected internal override IEnumerable<Child> GetChildren(Reni.SyntaxTree.Syntax target)
                => target
                    .DirectChildren
                    .Select(node => new Child(node))
                    .ToArray();


            [DisableDump]
            protected internal override bool IsIndentAtTailRequired => false;
        }

        class CompoundWithCleanup
            : Formatter
        {
            protected internal override BinaryTree[] GetFrameAnchors(Reni.SyntaxTree.Syntax main) => throw
                new NotImplementedException();

            protected internal override IEnumerable<Child> GetChildren
                (Reni.SyntaxTree.Syntax target) => throw new NotImplementedException();

            protected internal override bool IsIndentAtTailRequired => throw new NotImplementedException();
        }

        sealed class Terminal : Formatter
        {
            protected internal override BinaryTree[] GetFrameAnchors(Reni.SyntaxTree.Syntax main) => new BinaryTree[0];
            protected internal override IEnumerable<Child> GetChildren(Reni.SyntaxTree.Syntax target) => new Child[0];

            [DisableDump]
            protected internal override bool IsIndentAtTailRequired => false;
        }

        sealed class Declaration : Formatter
        {
            protected internal override BinaryTree[] GetFrameAnchors(Reni.SyntaxTree.Syntax main) => new BinaryTree[0];

            protected internal override IEnumerable<Child> GetChildren(Reni.SyntaxTree.Syntax target)
            {
                foreach(var syntax in GetList(target))
                    if(syntax is DeclarationSyntax declaration)
                        yield return new Child(declaration.Declarer.Name);
                    else
                        yield return new Child(syntax);
            }

            [DisableDump]
            protected internal override bool IsIndentAtTailRequired => false;

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

        protected internal abstract BinaryTree[] GetFrameAnchors(Reni.SyntaxTree.Syntax main);
        protected internal abstract IEnumerable<Child> GetChildren(Reni.SyntaxTree.Syntax target);

        [DisableDump]
        protected internal abstract bool IsIndentAtTailRequired { get; }

        public static Formatter Create(Reni.SyntaxTree.Syntax flatItem)
        {
            switch(flatItem)
            {
                case CompoundSyntax compound:
                    return compound.CleanupSection == null? new Compound() : new CompoundWithCleanup();
                case ExpressionSyntax expression:
                    return new TrainWreck();
                case null:
                    return new Terminal();
                case DeclarerSyntax.NameSyntax name:
                    return new Terminal();
                case DeclarationSyntax declaration:
                    return new Declaration();

                default:
                    NotImplementedFunction(flatItem);
                    return default;
            }
        }
    }
}