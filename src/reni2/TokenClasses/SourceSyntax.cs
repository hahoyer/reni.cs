using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    public sealed class SourceSyntax : DumpableObject, ISourcePart, ValueCache.IContainer
    {
        static int _nextObjectId;
        SourceSyntax _parent;
        [DisableDump]
        readonly ISyntaxProvider SyntaxProvider;
        internal SourceSyntax Left { get; }
        internal ITokenClass TokenClass { get; }
        [DisableDump]
        internal IToken Token { get; }
        internal SourceSyntax Right { get; }
        FunctionCache<int, SourceSyntax> LocatePositionCache { get; }
        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();

        internal SourceSyntax
            (
            SourceSyntax left,
            ITokenClass tokenClass,
            IToken token,
            SourceSyntax right,
            Func<Syntax, IToken, Syntax, ISyntaxProvider> getSyntax)
            : base(_nextObjectId++)
        {
            Left = left;
            TokenClass = tokenClass;
            Token = token;
            Right = right;
            LocatePositionCache = new FunctionCache<int, SourceSyntax>(LocatePositionForCache);
            SyntaxProvider = this.CachedValue(() => getSyntax(Left?.Syntax, Token, Right?.Syntax));

            if(Left != null)
                Left.Parent = this;

            if(Right != null)
                Right.Parent = this;

            AssertValid();
        }

        internal Syntax Syntax => SyntaxProvider.Value;

        void AssertValid() { AssertValidSourceQueue(); }

        void AssertValidSourceQueue()
        {
            if(Left != null)
            {
                Tracer.Assert
                    (
                        Left.SourcePart.End >= Token.SourcePart.Start,
                        () => Left.SourcePart.End.Span(Token.SourcePart.Start).NodeDump
                    );
                if(!Issues.Any())
                    Tracer.Assert
                        (
                            Left.SourcePart.End <= Token.SourcePart.Start,
                            () => Left.SourcePart.NodeDump + " <> " + Token.SourcePart.NodeDump
                        );
            }
            if(Right != null)
                Tracer.Assert
                    (
                        Token.SourcePart.End == Right.SourcePart.Start,
                        () => Token.SourcePart.End.Span(Right.SourcePart.Start).NodeDump
                    );
        }

        internal Issue[] Issues => (Left?.Issues).plus(SyntaxProvider.Issues).plus(Right?.Issues);

        [DisableDump]
        internal SourceSyntax Parent
        {
            get { return _parent; }
            set
            {
                Tracer.Assert(value == null || _parent == null);
                _parent = value;
            }
        }

        SourcePart ISourcePart.All => SourcePart;

        [DisableDump]
        internal SourcePart SourcePart => Left?.SourcePart + Token.SourcePart + Right?.SourcePart;
        [DisableDump]
        string FilePosition => Token.Characters.FilePosition;

        public SourceSyntax LocatePosition(int current) => LocatePositionCache[current];

        SourceSyntax LocatePositionForCache(int current)
        {
            if(current < SourcePart.Position || current >= SourcePart.EndPosition)
                return Parent?.LocatePosition(current);

            return Left?.CheckedLocatePosition(current) ??
                Right?.CheckedLocatePosition(current) ??
                    this;
        }

        SourceSyntax CheckedLocatePosition(int current)
            =>
                SourcePart.Position <= current && current < SourcePart.EndPosition
                    ? LocatePosition(current)
                    : null;

        internal SourceSyntax Locate(SourcePart part)
            => Left?.CheckedLocate(part) ??
                Right?.CheckedLocate(part) ??
                    this;

        SourceSyntax CheckedLocate(SourcePart part)
            => SourcePart.Contains(part) ? Locate(part) : null;

        internal IEnumerable<SourceSyntax> Belongings(SourceSyntax recent)
        {
            var root = RootOfBelongings(recent);

            var matcher = root?.TokenClass as IBelongingsMatcher;
            return matcher == null
                ? null
                : root
                    .ItemsAsLongAs(item => matcher.IsBelongingTo(item.TokenClass))
                    .ToArray();
        }

        internal IEnumerable<SourceSyntax> ItemsAsLongAs(Func<SourceSyntax, bool> condition)
            => new[] {this}
                .Concat(Left.CheckedItemsAsLongAs(condition))
                .Concat(Right.CheckedItemsAsLongAs(condition));


        internal SourceSyntax RootOfBelongings(SourceSyntax recent)
        {
            var matcher = recent.TokenClass as IBelongingsMatcher;
            if(matcher == null)
                return null;

            var sourceSyntaxs = BackChain(recent)
                .ToArray();

            return sourceSyntaxs
                .Skip(1)
                .TakeWhile(item => matcher.IsBelongingTo(item.TokenClass))
                .LastOrDefault()
                ?? recent;
        }

        IEnumerable<SourceSyntax> BackChain(SourceSyntax recent)
        {
            var subChain = SubBackChain(recent);
            if(subChain == null)
                yield break;

            foreach(var items in subChain)
                yield return items;

            yield return this;
        }

        SourceSyntax[] SubBackChain(SourceSyntax recent)
        {
            if(this == recent)
                return new SourceSyntax[0];

            if(Left != null)
            {
                var result = Left.BackChain(recent).ToArray();
                if(result.Any())
                    return result;
            }

            if(Right != null)
            {
                var result = Right.BackChain(recent).ToArray();
                if(result.Any())
                    return result;
            }

            return null;
        }

        [DisableDump]
        internal IEnumerable<SourceSyntax> Items => this.CachedValue(GetItems);

        IEnumerable<SourceSyntax> GetItems()
        {
            yield return this;

            if(Left != null)
                foreach(var sourceSyntax in Left.Items)
                    yield return sourceSyntax;
            if(Right != null)
                foreach(var sourceSyntax in Right.Items)
                    yield return sourceSyntax;
        }

        public string BraceMatchDump => new BraceMatchDumper(this, 3).Dump();

        [DisableDump]
        internal IEnumerable<WhiteSpaceToken> LeadingWhiteSpaceTokens
            => Left != null
                ? Left.LeadingWhiteSpaceTokens
                : Token.PrecededWith.OnlyLeftPart();

        [DisableDump]
        internal ITokenClass LeftMostTokenClass
            => Left == null ? TokenClass : Left.LeftMostTokenClass;

        [DisableDump]
        internal ITokenClass RightMostTokenClass
            => Right == null ? TokenClass : Right.RightMostTokenClass;

        [DisableDump]
        public IEnumerable<SourceSyntax> ParentChainIncludingThis
        {
            get
            {
                yield return this;

                if(Parent == null)
                    yield break;

                foreach(var other in Parent.ParentChainIncludingThis)
                    yield return other;
            }
        }

        [DisableDump]
        public string[] DeclarationOptions
        {
            get
            {
                var typeoid = Syntax.Typeoid;
                NotImplementedMethod(nameof(typeoid), typeoid);
                return null;

                var syntax = Syntax
                    .ToCompiledSyntax
                    .Value;
                var functionCache = syntax.ResultCache;
                if(functionCache.Any())
                    return functionCache
                        .Select(item => item.Value.Type)
                        .SelectMany(item => item.DeclarationOptions)
                        .Distinct()
                        .ToArray();

                NotImplementedMethod();
                return null;
            }
        }
    }

    interface IBelongingsMatcher
    {
        bool IsBelongingTo(IBelongingsMatcher otherMatcher);
    }
}

