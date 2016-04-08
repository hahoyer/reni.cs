using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;
using Reni.Struct;
using Reni.Validation;

namespace Reni.TokenClasses
{
    public sealed class Syntax : DumpableObject, ISourcePart, ValueCache.IContainer
    {
        internal static Syntax CreateSourceSyntax
            (
            Syntax left,
            ITokenClass tokenClass,
            IToken token,
            Syntax right)
            => new Syntax(left, tokenClass, token, right);

        static int _nextObjectId;

        Syntax _parent;
        internal Syntax Left { get; }
        internal ITokenClass TokenClass { get; }
        [DisableDump]
        internal IToken Token { get; }
        internal Syntax Right { get; }

        FunctionCache<int, Syntax> LocatePositionCache { get; }
        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();

        Syntax
            (
            Syntax left,
            ITokenClass tokenClass,
            IToken token,
            Syntax right)
            : base(_nextObjectId++)
        {
            Left = left;
            TokenClass = tokenClass;
            Token = token;
            Right = right;
            LocatePositionCache = new FunctionCache<int, Syntax>(LocatePositionForCache);

            if(Left != null)
                Left.Parent = this;

            if(Right != null)
                Right.Parent = this;
        }

        [DisableDump]
        internal Syntax Parent
        {
            get { return _parent; }
            private set
            {
                if(value == null)
                    throw new ArgumentNullException(nameof(value));

                Tracer.Assert(_parent == null);
                _parent = value;
            }
        }

        SourcePart ISourcePart.All => SourcePart;

        [DisableDump]
        internal SourcePart SourcePart => Left?.SourcePart + Token.SourcePart + Right?.SourcePart;
        [DisableDump]
        string FilePosition => Token.Characters.FilePosition;

        public Syntax LocatePosition(int current) => LocatePositionCache[current];

        Syntax LocatePositionForCache(int current)
        {
            if(current < SourcePart.Position || current >= SourcePart.EndPosition)
                return Parent?.LocatePosition(current);

            return Left?.CheckedLocatePosition(current) ??
                Right?.CheckedLocatePosition(current) ??
                    this;
        }

        Syntax CheckedLocatePosition(int current)
            =>
                SourcePart.Position <= current && current < SourcePart.EndPosition
                    ? LocatePosition(current)
                    : null;

        internal Syntax Locate(SourcePart part)
            => Left?.CheckedLocate(part) ??
                Right?.CheckedLocate(part) ??
                    this;

        Syntax CheckedLocate(SourcePart part)
            => SourcePart.Contains(part) ? Locate(part) : null;

        internal IEnumerable<Syntax> Belongings(Syntax recent)
        {
            var root = RootOfBelongings(recent);

            var matcher = root?.TokenClass as IBelongingsMatcher;
            return matcher == null
                ? null
                : root
                    .ItemsAsLongAs(item => matcher.IsBelongingTo(item.TokenClass))
                    .ToArray();
        }

        internal IEnumerable<Syntax> ItemsAsLongAs(Func<Syntax, bool> condition)
            => new[] {this}
                .Concat(Left.CheckedItemsAsLongAs(condition))
                .Concat(Right.CheckedItemsAsLongAs(condition));


        internal Syntax RootOfBelongings(Syntax recent)
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

        IEnumerable<Syntax> BackChain(Syntax recent)
        {
            var subChain = SubBackChain(recent);
            if(subChain == null)
                yield break;

            foreach(var items in subChain)
                yield return items;

            yield return this;
        }

        Syntax[] SubBackChain(Syntax recent)
        {
            if(this == recent)
                return new Syntax[0];

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
        internal IEnumerable<Syntax> Items => this.CachedValue(GetItems);

        IEnumerable<Syntax> GetItems()
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
        public IEnumerable<Syntax> ParentChainIncludingThis
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
                NotImplementedMethod();
                return null;

                var syntax = Value
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

        [DisableDump]
        internal Checked<Value> Value
            => TokenClass.GetValue(Left, Token.Characters, Right);

        [DisableDump]
        internal Issue[] Issues 
            => Left?.Issues
            .plus(Value?.Issues)
            .plus(Right?.Issues);

        [DisableDump]
        internal Checked<CompoundSyntax> ToCompound
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }
        internal Syntax GetBracketKernel(SourcePart token)
        {
            Tracer.Assert(token.Id == "");
            Tracer.Assert(this != null);

            Tracer.Assert(Left != null);
            Tracer.Assert(TokenClass is RightParenthesis);
            Tracer.Assert(Right == null);

            Tracer.Assert(Left.Left == null);
            Tracer.Assert(Left.TokenClass is LeftParenthesis);

            var syntax = Left.Right;
            return syntax;
        }
    }

    interface ISyntaxProvider
    {
        OldSyntax Value { get; }
        IEnumerable<Issue> Issues { get; }
    }

    interface IBelongingsMatcher
    {
        bool IsBelongingTo(IBelongingsMatcher otherMatcher);
    }
}