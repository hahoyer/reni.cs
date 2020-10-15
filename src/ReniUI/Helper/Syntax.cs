using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni;
using Reni.Context;
using Reni.Helper;
using Reni.Parser;
using Reni.TokenClasses;
using Reni.Validation;
using static hw.Helper.ValueCacheExtension;

namespace ReniUI.Helper
{
    sealed class Syntax : BinaryTreeSyntaxWithParent<Syntax>
    {
        class CacheContainer
        {
            public ValueCache<Reni.Parser.Syntax> FlatSyntax;
            public FunctionCache<int, Syntax> LocateByPosition;
        }

        readonly CacheContainer Cache = new CacheContainer();

        readonly Func<Reni.Parser.Syntax> GetFlatSyntaxRoot;

        internal Syntax(BinaryTree binary, Syntax parent = null, Func<Reni.Parser.Syntax> getFlatSyntax = null)
            : base(binary, parent)
        {
            GetFlatSyntaxRoot = getFlatSyntax;
            Cache.LocateByPosition = new FunctionCache<int, Syntax>(LocateByPositionForCache);
            Cache.FlatSyntax = NewValueCache(() => GetFlatSyntaxRoot != null? GetFlatSyntaxRoot() : GetFlatSyntax());
        }

        internal string FlatSyntaxDump => Cache.FlatSyntax.IsValid? FlatSyntax.Dump() : "<unknown>";

        [DisableDump]
        internal Reni.Parser.Syntax FlatSyntax => Cache.FlatSyntax.Value;

        internal new BinaryTree FlatItem => base.FlatItem;

        [DisableDump]
        internal IEnumerable<Issue> Issues => FlatItem.Issues;

        [DisableDump]
        internal IEnumerable<Syntax> ParserLevelBelongings => this.CachedValue(GetParserLevelBelongings);

        internal string[] GetDeclarationOptions(ContextBase context)
            => (FlatSyntax as ValueSyntax)?.GetDeclarationOptions(context).ToArray();

        IEnumerable<Syntax> GetParserLevelBelongings()
        {
            if(!(TokenClass is IBelongingsMatcher matcher))
                yield break;

            switch(TokenClass)
            {
                case ILeftBracket _ when matcher.IsBelongingTo(Parent.TokenClass):
                    yield return Parent;
                    yield break;
                case ILeftBracket _:
                    NotImplementedMethod();
                    yield break;
                case IRightBracket _ when Left != null && matcher.IsBelongingTo(Left.TokenClass):
                    yield return Left;
                    yield break;
                case IRightBracket _ when Parent.TokenClass is ILeftBracket:
                    yield return Parent;
                    yield break;
                case IRightBracket _:
                    NotImplementedMethod();
                    yield break;
                case List _:
                    break;
                default:
                    NotImplementedMethod();
                    yield break;
            }

            Tracer.Assert(TokenClass is List);

            var parents = Parent
                .Chain(node => node.Parent)
                .Where(node => matcher.IsBelongingTo(node.TokenClass));

            var children = Right
                .Chain(node => node.Right)
                .TakeWhile(node => matcher.IsBelongingTo(node.TokenClass));

            foreach(var node in parents.Concat(children))
                yield return node;
        }

        Reni.Parser.Syntax GetFlatSyntax()
        {
            var result = Parent
                .FlatSyntax
                .GetNodesFromTopToBottom(node => node?.Binary != null)
                .SingleOrDefault(node=>node.Binary == FlatItem);

            if(result != null)
                return result;

            if(TokenClass is ILeftBracket && TokenClass.IsBelongingTo(Parent.TokenClass))
                return Parent.FlatSyntax;

            if(TokenClass is List)
            {
                if(Parent.TokenClass is ILeftBracket)
                    return Parent.FlatSyntax;

                NotImplementedMethod();
                return default;
            }

            if(TokenClass is Colon)
            {
                if(Parent.TokenClass is ILeftBracket)
                    return Parent.FlatSyntax;
                NotImplementedMethod();
                return default;
            }

            NotImplementedMethod();
            return default;
        }

        protected override Syntax Create(BinaryTree flatItem) => new Syntax(flatItem, this);


        internal Syntax LocateByPosition(int offset) => Cache.LocateByPosition[offset];

        internal Syntax Locate(SourcePart span)
        {
            NotImplementedMethod(span);
            return default;
        }

        internal IEnumerable<Syntax> ItemsAsLongAs(Func<Syntax, bool> condition)
            => this
                .GetNodesFromLeftToRight()
                .SelectMany(node => node?.ItemsAsLongAs(condition) ?? new Syntax[0]);

        Syntax LocateByPositionForCache(int current)
        {
            var nodes = this
                .GetNodesFromLeftToRight()
                .ToArray();
            var ranges = nodes
                .Select(node => node.Token.Characters)
                .ToArray();

            return nodes
                .Top(node => node.Token.Characters.EndPosition > current)
                .AssertNotNull();
        }
    }

    abstract class ProxySyntax : Reni.Parser.Syntax.NoChildren
    {
        internal class ColonLevel : ProxySyntax
        {
            public ColonLevel(Syntax client, BinaryTree target)
                : base(client, target) { }
        }

        internal class ListLevel : ProxySyntax
        {
            public ListLevel(Syntax client, BinaryTree target)
                : base(client, target) { }
        }

        internal class LeftBracketOfRightBracket : ProxySyntax
        {
            public LeftBracketOfRightBracket(Syntax client, BinaryTree target)
                : base(client, target) { }
        }

        [DisableDump]
        internal readonly Syntax Client;

        ProxySyntax(Syntax client, BinaryTree target)
            : base(target)
            => Client = client;
    }
}