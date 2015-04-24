using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;

namespace Reni.UserInterface
{
    public abstract class TokenInformation : DumpableObject
    {
        [DisableDump]
        public abstract SourcePart TokenSourcePart { get; }
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
                if (IsLineEnd)
                    return '$';
                if (IsNumber)
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

        public virtual string Reformat => SourcePart.Id;

        public Trimmed Trim(int start, int end) => new Trimmed(this, start, end);

        bool Equals(TokenInformation other) => TokenSourcePart == other.TokenSourcePart;

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

        public override int GetHashCode() => TokenSourcePart.GetHashCode();

        public sealed class Trimmed
        {
            public readonly TokenInformation Token;
            public readonly SourcePart SourcePart;

            internal Trimmed(TokenInformation token, int start, int end)
            {
                Token = token;
                var sourcePart = token.TokenSourcePart;
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
}