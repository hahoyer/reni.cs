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

        public Trimmed Trim(SourcePart span) => new Trimmed(this, span);

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

        public sealed class Trimmed : DumpableObject
        {
            public readonly TokenInformation Token;
            public readonly SourcePart SourcePart;

            internal Trimmed(TokenInformation token, SourcePart sourcePart)
            {
                Token = token;
                SourcePart = sourcePart;
            }

            public string Reformat => Token.Reformat(SourcePart);

            public IEnumerable<char> GetCharArray()
                => SourcePart
                    .Id
                    .ToCharArray();
        }

        protected abstract string Reformat(SourcePart targetPart);

        public abstract IEnumerable<SourcePart> FindAllBelongings(Compiler compiler);
    }
}