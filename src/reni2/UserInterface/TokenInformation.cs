using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.Parser;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.UserInterface
{
    public abstract class TokenInformation : DumpableObject
    {
        [DisableDump]
        public abstract SourcePart SourcePart { get; }
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
        public virtual bool IsLineEnd => false;
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
                if(IsLineEnd)
                    return '$';
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

        public int StartPosition => SourcePart.Position;
        public int EndPosition => SourcePart.EndPosition;

        public Trimmed Trim(int start, int end) => new Trimmed(this, start, end);

        bool Equals(TokenInformation other) => SourcePart == other.SourcePart;

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
                return false;
            if(ReferenceEquals(this, obj))
                return true;
            if(obj.GetType() != GetType())
                return false;
            return Equals((TokenInformation) obj);
        }

        public override int GetHashCode() => SourcePart.GetHashCode();

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

        public abstract IEnumerable<SourcePart> FindAllBelongings(Compiler compiler);
    }

    sealed class SyntaxToken : TokenInformation
    {
        internal SyntaxToken(SourceSyntax sourceSyntax) { SourceSyntax = sourceSyntax; }

        internal SourceSyntax SourceSyntax { get; }

        TokenClass TokenClass => SourceSyntax.TokenClass as TokenClass;

        public override SourcePart SourcePart => SourceSyntax.Token.Characters;
        public override bool IsKeyword => !IsIdentifier && !IsNumber && !IsText;
        public override bool IsIdentifier => TokenClass is Definable;
        public override bool IsText => TokenClass is Text;
        public override bool IsNumber => TokenClass is Number;
        public override bool IsError => SourceSyntax.Issues.Any();
        public override bool IsBraceLike => TokenClass is IBelongingsMatcher;

        public override bool IsComment
            => SourceSyntax.Issues.Any(item => item.IssueId == IssueId.EOFInComment);

        public override bool IsLineComment
            => SourceSyntax.Issues.Any(item => item.IssueId == IssueId.EOFInLineComment);

        public override string State => SourceSyntax.Token.Id ?? "";

        public override IEnumerable<SourcePart> FindAllBelongings(Compiler compiler)
            => compiler.FindAllBelongings(SourceSyntax)?.Select(item => item.Token.Characters);
    }

    sealed class WhiteSpaceToken : TokenInformation
    {
        readonly hw.Parser.WhiteSpaceToken _item;
        public WhiteSpaceToken(hw.Parser.WhiteSpaceToken item) { _item = item; }

        public override SourcePart SourcePart => _item.Characters;
        public override bool IsComment => Lexer.IsComment(_item);
        public override bool IsLineComment => Lexer.IsLineComment(_item);
        public override bool IsWhiteSpace => Lexer.IsWhiteSpace(_item);
        public override bool IsLineEnd => Lexer.IsLineEnd(_item);
        public override string State => Lexer.Instance.WhiteSpaceId(_item) ?? "";

        public override IEnumerable<SourcePart> FindAllBelongings(Compiler compiler) { yield break; }
    }
}