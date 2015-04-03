using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.Parser;
using Reni.ReniParser;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.UserInterface
{
    public abstract class TokenInformation : DumpableObject
    {
        [DisableDump]
        protected abstract SourcePart SourcePart { get; }
        [DisableDump]
        public virtual bool IsKeyword => false;
        [DisableDump]
        public virtual bool IsIdentifier => false;
        [DisableDump]
        public virtual bool IsText => false;
        [DisableDump]
        public virtual bool IsNumber => false;
        [DisableDump]
        public virtual bool IsComment => false;
        [DisableDump]
        public virtual bool IsLineComment => false;
        [DisableDump]
        public virtual bool IsWhiteSpace => false;
        [DisableDump]
        public virtual bool IsBraceLike => false;
        [DisableDump]
        public virtual bool IsError => false;
        [DisableDump]
        public virtual string State => "";
        public char TypeCharacter
        {
            get
            {
                if(IsError)
                    return 'e';
                if(IsComment)
                    return 'c';
                if(IsLineComment)
                    return 'l';
                if(IsWhiteSpace)
                    return 'w';
                if(IsNumber)
                    return 'n';
                if(IsText)
                    return 't';
                if(IsKeyword)
                    return 'k';
                if(IsIdentifier)
                    return 'i';
                NotImplementedMethod();
                return '?';
            }
        }

        public Trimmed Trim(int start, int end) => new Trimmed(this, start, end);

        public sealed class Trimmed
        {
            public readonly TokenInformation Token;
            public readonly SourcePart SourcePart;

            internal Trimmed(TokenInformation token, int start, int end)
            {
                Token = token;
                var sourcePart = token.SourcePart;
                SourcePart = (sourcePart.Source + (Math.Max(sourcePart.Position, start)))
                    .Span(sourcePart.Source + Math.Min(sourcePart.EndPosition, end));
            }

            public IEnumerable<char> GetCharArray()
                => SourcePart
                    .Id
                    .ToCharArray();
        }
    }

    sealed class SyntaxToken : TokenInformation
    {
        internal SyntaxToken(SourceSyntax sourceSyntax) { SourceSyntax = sourceSyntax; }

        SourceSyntax SourceSyntax { get; }
        Syntax Syntax => SourceSyntax.Syntax;

        protected override SourcePart SourcePart => SourceSyntax.Token.Characters;
        public override bool IsKeyword => Syntax.IsKeyword;
        public override bool IsIdentifier => Syntax.IsIdentifier;
        public override bool IsText => Syntax.IsText;
        public override bool IsNumber => Syntax.IsNumber;
        public override bool IsError => SourceSyntax.Issues.Any();
        public override bool IsBraceLike => Syntax.IsBraceLike;

        public override bool IsComment
            => SourceSyntax.Issues.Any(item => item.IssueId == IssueId.EOFInComment);

        public override bool IsLineComment
            => SourceSyntax.Issues.Any(item => item.IssueId == IssueId.EOFInLineComment);

        public override string State => SourceSyntax.Token.Id ?? "";
    }

    sealed class WhiteSpaceToken : TokenInformation
    {
        readonly hw.Parser.WhiteSpaceToken _item;
        public WhiteSpaceToken(hw.Parser.WhiteSpaceToken item) { _item = item; }

        protected override SourcePart SourcePart => _item.Characters;
        public override bool IsComment => ReniLexer.IsComment(_item);
        public override bool IsLineComment => ReniLexer.IsLineComment(_item);
        public override bool IsWhiteSpace => ReniLexer.IsWhiteSpace(_item);
        public override string State => ReniLexer.Instance.WhiteSpaceId(_item) ?? "";
    }
}