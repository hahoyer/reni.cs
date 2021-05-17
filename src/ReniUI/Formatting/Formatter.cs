using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.SyntaxFactory;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace ReniUI.Formatting
{
    abstract class Formatter : DumpableObject
    {
        internal sealed class Child
        {
            internal readonly SourcePosition Position;
            internal readonly BinaryTree Head;
            internal readonly Reni.SyntaxTree.Syntax FlatItem;

            public Child(BinaryTree head, Reni.SyntaxTree.Syntax flatItem, SourcePosition position = null)
            {
                Position = head?.Token.Characters.Start ?? position;
                Head = head;
                FlatItem = flatItem;
                Position.AssertIsNotNull();
                //Tracer.ConditionalBreak(Head?.ObjectId == 0);
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
                => GetWagons(target).Select(GetCargo);

            protected internal override bool IsIndentAtTailRequired => true;

            static Child GetCargo(Reni.SyntaxTree.Syntax node)
                => new Child
                (node.MainAnchor,
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
                    .Where(node => node.TokenClass is IRightBracket || node.TokenClass is ILeftBracket)
                    .ToArray();

            protected internal override IEnumerable<Child> GetChildren(Reni.SyntaxTree.Syntax target)
            {
                var compound = (CompoundSyntax)target;

                var listAnchors
                    = compound.Anchor.Items
                        .Where(node => node.TokenClass is List)
                        .ToArray();

                (listAnchors.Length + 1 == compound.Statements.Length).Assert();

                var cleanupAnchor
                    = compound.Anchor.Items
                          .SingleOrDefault(node => node.TokenClass is Cleanup) ??
                      compound.RightMostAnchor.RightNeighbor;

                var heads
                    = T(T((BinaryTree)null), listAnchors, T(cleanupAnchor))
                        .ConcatMany()
                        .ToArray();


                return compound
                    .DirectChildren
                    .Select((node, index) => new Child(heads[index], node, target.ChildSourcePart.Start))
                    .ToArray();
            }

            protected internal override bool IsIndentAtTailRequired => false;
        }

        sealed class Terminal : Formatter
        {
            protected internal override BinaryTree[] GetFrameAnchors(Reni.SyntaxTree.Syntax main) => new BinaryTree[0];
            protected internal override IEnumerable<Child> GetChildren(Reni.SyntaxTree.Syntax target) => new Child[0];
            protected internal override bool IsIndentAtTailRequired => false;
        }

        sealed class Declaration : Formatter
        {
            protected internal override BinaryTree[] GetFrameAnchors(Reni.SyntaxTree.Syntax main) => new BinaryTree[0];

            protected internal override IEnumerable<Child> GetChildren(Reni.SyntaxTree.Syntax target)
            {
                BinaryTree head = null;
                foreach(var syntax in GetList(target))
                    if(syntax is DeclarationSyntax declaration)
                    {
                        yield return new Child(head, declaration.Declarer.Name
                            , head == null? target.ChildSourcePart.Start : null);
                        head = syntax.Anchor.Items.Single(item => item.TokenClass is IDeclarationToken);
                    }
                    else
                        yield return new Child(head, syntax);
            }

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
        protected internal abstract bool IsIndentAtTailRequired { get; }

        public static Formatter Create(Reni.SyntaxTree.Syntax flatItem)
        {
            switch(flatItem)
            {
                case CompoundSyntax compound:
                    return new Compound();
                case ExpressionSyntax expression:
                    return new TrainWreck();
                case null:
                case DeclarerSyntax.NameSyntax:
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