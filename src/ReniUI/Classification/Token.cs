using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Classification
{
    public abstract class Syntax : DumpableObject
    {
        public sealed class Trimmed : DumpableObject
        {
            public readonly SourcePart SourcePart;
            public readonly Syntax Syntax;

            internal Trimmed(Syntax syntax, SourcePart sourcePart)
            {
                Syntax = syntax;
                SourcePart = sourcePart.Intersect(Syntax.SourcePart) ?? Syntax.SourcePart.Start.Span(0);
            }

            public IEnumerable<char> GetCharArray()
                => SourcePart
                    .Id
                    .ToCharArray();
        }

        internal readonly int Index;
        internal readonly Helper.Syntax Master;


        protected Syntax(Helper.Syntax master, int index)
        {
            Master = master;
            Index = index;
        }

        BinaryTree Binary => Master.FlatItem.FrameItems.Items[Index];
        internal TokenClass TokenClass => Binary.TokenClass as TokenClass;
        internal IToken Token => Binary.Token;

        [DisableDump]
        public virtual SourcePart SourcePart => Binary.Token.Characters;

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

        [DisableDump]
        public abstract IEnumerable<SourcePart> ParserLevelGroup { get; }

        public Trimmed TrimLine(SourcePart span) => new Trimmed(this, span);

        bool Equals(Syntax other) => SourcePart == other.SourcePart;

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
                return false;
            if(ReferenceEquals(this, obj))
                return true;
            if(obj.GetType() != GetType())
                return false;
            return Equals((Syntax)obj);
        }

        public override int GetHashCode() => SourcePart.GetHashCode();

        internal static Syntax LocateByPosition(Helper.Syntax target, int offset, bool includingParent = false)
        {
            var result = target.LocateByPosition(offset, includingParent );
            result.AssertIsNotNull();
            var resultToken = result.Item1.FlatItem.FrameItems.Items[result.Item2].Token;
            if(offset < resultToken.Characters.Position)
                return new WhiteSpaceSyntax
                (
                    resultToken.PrecededWith.Last(item => offset >= item.SourcePart.Position),
                    result.Master, result.Index
                );

            return new SyntaxToken(result.Master, result.Index);
        }

        public static Syntax GetRightNeighbor(Helper.Syntax target, int current)
        {
            NotImplementedFunction(target, current);
            return default;
        }
    }
}