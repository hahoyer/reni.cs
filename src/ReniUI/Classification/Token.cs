using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using ReniUI.Helper;

namespace ReniUI.Classification
{
    public abstract class Token : DumpableObject
    {
        public sealed class Trimmed : DumpableObject
        {
            public readonly SourcePart SourcePart;
            public readonly Token Token;

            internal Trimmed(Token token, SourcePart sourcePart)
            {
                Token = token;
                SourcePart = sourcePart.Intersect(Token.SourcePart)
                             ?? Token.SourcePart.Start.Span(0);
            }

            public IEnumerable<char> GetCharArray()
                => SourcePart
                    .Id
                    .ToCharArray();
        }

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
        public virtual bool IsBrace => false;

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
        internal abstract Syntax Master { get; }

        public Trimmed TrimLine(SourcePart span) => new Trimmed(this, span);

        bool Equals(Token other) => SourcePart == other.SourcePart;

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
                return false;
            if(ReferenceEquals(this, obj))
                return true;
            if(obj.GetType() != GetType())
                return false;
            return Equals((Token)obj);
        }

        public override int GetHashCode() => SourcePart.GetHashCode();

        public abstract IEnumerable<SourcePart> ParserLevelBelongings { get; }

        internal static Token LocateByPosition(Syntax target, int offset)
        {
            var result = target.LocateByPosition(offset);
            Tracer.Assert(result != null);
            var resultToken = result.Token;
            if(offset < resultToken.Characters.Position)
                return new WhiteSpaceToken
                (
                    resultToken.PrecededWith.Last(item => offset >= item.SourcePart.Position),
                    result
                );

            return new SyntaxToken(result);
        }

        internal static Token GetRightNeighbor(Syntax target, int offset)
        {
            var result = target
                .Chain(node => node.RightNeighbor)
                .First(node => node.Token.Characters.EndPosition > offset);

            var resultToken = result.Token;
            if(offset < resultToken.Characters.Position)
                return new WhiteSpaceToken
                (
                    resultToken.PrecededWith.Last(item => offset >= item.SourcePart.Position),
                    result
                );

            return new SyntaxToken(result);
        }
    }
}