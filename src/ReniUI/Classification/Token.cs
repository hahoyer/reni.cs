using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni;
using Reni.TokenClasses;

namespace ReniUI.Classification
{
    public abstract class Token : DumpableObject
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
        internal abstract Helper.Syntax Syntax { get; }

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
            return Equals((Token) obj);
        }

        public override int GetHashCode() => SourcePart.GetHashCode();

        public sealed class Trimmed : DumpableObject
        {
            public readonly Token Token;
            public readonly SourcePart SourcePart;

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

        public abstract IEnumerable<SourcePart> FindAllBelongings(CompilerBrowser compiler);

        internal static Token LocatePosition(Helper.Syntax start, int offset)
        {
            var result = start.LocatePosition(offset);
            if (offset < result.Token.Characters.Position)
                return new WhiteSpaceToken
                    (
                    result.Token.PrecededWith.Last(item => offset >= item.SourcePart.Position),
                    result
                    );

            return new SyntaxToken(result);
        }
    }
}