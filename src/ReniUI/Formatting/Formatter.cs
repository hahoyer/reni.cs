using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
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
            protected internal override BinaryTree[] GetAnchors(Syntax target)
            {
                (target.Main.Anchor.Items.Length<=1 ).Assert();
                return new BinaryTree[0];
            }

            protected internal override IEnumerable<Child> GetChildren(Reni.SyntaxTree.Syntax target)
                => GetWagons(target).Select(GetCargo);

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
        }

        sealed class Compound : Formatter
        {
            protected internal override BinaryTree[] GetAnchors(Syntax target)
                => target
                    .Main
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
                          .SingleOrDefault(node => node.TokenClass is Cleanup)
                          ?? compound.RightMostAnchor.RightNeighbor;

                var heads
                    = T(T((BinaryTree)null), listAnchors, T(cleanupAnchor))
                        .ConcatMany()
                        .ToArray();


                return compound
                    .DirectChildren
                    .Select((node, index) => new Child(heads[index], node, target.ChildSourcePart.Start))
                    .ToArray();
            }
        }

        sealed class Terminal : Formatter
        {
            protected internal override BinaryTree[] GetAnchors(Syntax target) => new BinaryTree[0];
            protected internal override IEnumerable<Child> GetChildren(Reni.SyntaxTree.Syntax target) => new Child[0];
        }

        protected internal abstract BinaryTree[] GetAnchors(Syntax target);
        protected internal abstract IEnumerable<Child> GetChildren(Reni.SyntaxTree.Syntax target);

        public static Formatter Create(Reni.SyntaxTree.Syntax flatItem)
        {
            switch(flatItem)
            {
                case CompoundSyntax compound:
                    return new Compound();
                case ExpressionSyntax expression:
                    return new TrainWreck();
                case null:
                    return new Terminal();

                default:
                    NotImplementedFunction(flatItem);
                    return default;
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
}