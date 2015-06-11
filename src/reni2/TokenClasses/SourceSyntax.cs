using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.Scanner;
using Reni.Formatting;
using Reni.Parser;
using Reni.UserInterface;
using Reni.Validation;

namespace Reni.TokenClasses
{
    sealed class SourceSyntax : DumpableObject, ISourcePart
    {
        static int _nextObjectId;
        readonly Issue[] _issues;

        public SourceSyntax
            (
            SourceSyntax left,
            ITokenClass tokenClass,
            IToken token,
            SourceSyntax right,
            Syntax syntax,
            Issue[] issues)
            : base(_nextObjectId++)
        {
            Syntax = syntax;
            _issues = issues ?? new Issue[0];
            Left = left;
            TokenClass = tokenClass;
            Token = token;
            Right = right;
            AssertValid();
        }

        void AssertValid() { AssertValidSourceQueue(); }

        void AssertValidSourceQueue()
        {
            if(Left != null)
                Tracer.Assert
                    (
                        Left.SourcePart.End == Token.SourcePart.Start,
                        () => Left.SourcePart.End.Span(Token.SourcePart.Start).NodeDump
                    );
            if(Right != null)
                Tracer.Assert
                    (
                        Token.SourcePart.End == Right.SourcePart.Start,
                        () => Token.SourcePart.End.Span(Right.SourcePart.Start).NodeDump
                    );
        }

        internal Issue[] Issues => (Left?.Issues).plus(_issues).plus(Right?.Issues);
        [DisableDump]
        internal Syntax Syntax { get; }
        internal SourceSyntax Left { get; }
        internal ITokenClass TokenClass { get; }
        [DisableDump]
        internal IToken Token { get; }
        internal SourceSyntax Right { get; }

        SourcePart ISourcePart.All => SourcePart;

        [DisableDump]
        internal SourcePart SourcePart => Left?.SourcePart + Token.SourcePart + Right?.SourcePart;
        [DisableDump]
        string FilePosition => Token.Characters.FilePosition;


        internal TokenInformation Locate(SourcePosn sourcePosn)
        {
            if(sourcePosn.IsEnd)
                return new SyntaxToken(this);

            if(sourcePosn < Token.SourcePart)
                return Left?.Locate(sourcePosn);

            if(sourcePosn < Token.Characters)
                return new UserInterface.WhiteSpaceToken
                    (Token.PrecededWith.Single(item => item.Characters.Contains(sourcePosn)));

            if(Token.Characters.Contains(sourcePosn))
                return new SyntaxToken(this);

            return Right?.Locate(sourcePosn);
        }

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
        internal IEnumerable<SourceSyntax> Items
        {
            get
            {
                yield return this;
                if(Left != null)
                    foreach(var sourceSyntax in Left.Items)
                        yield return sourceSyntax;
                if(Right != null)
                    foreach(var sourceSyntax in Right.Items)
                        yield return sourceSyntax;
            }
        }

        public string BraceMatchDump => new BraceMatchDumper(this, 3).Dump();

        internal string Reformat(SourcePart targetPart = null, Provider provider = null)
            => (provider ?? new Provider())
                .Reformat(this, targetPart ?? SourcePart);

        [DisableDump]
        internal IEnumerable<hw.Parser.WhiteSpaceToken> LeadingWhiteSpaceTokens
            => Left != null
                ? Left.LeadingWhiteSpaceTokens
                : Token.PrecededWith.OnlyLeftPart();

        [DisableDump]
        internal ITokenClass LeftMostTokenClass
            => Left == null ? TokenClass : Left.LeftMostTokenClass;

        [DisableDump]
        internal ITokenClass RightMostTokenClass
            => Right == null ? TokenClass : Right.RightMostTokenClass;
    }

    interface IBelongingsMatcher
    {
        bool IsBelongingTo(IBelongingsMatcher otherMatcher);
    }
}