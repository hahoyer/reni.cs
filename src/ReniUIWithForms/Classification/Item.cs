using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.TokenClasses;

namespace ReniUI.Classification
{
    public abstract class Item : DumpableObject
    {
        public sealed class Trimmed : DumpableObject
        {
            public readonly SourcePart SourcePart;
            public readonly Item Item;

            internal Trimmed(Item item, SourcePart sourcePart)
            {
                Item = item;
                SourcePart = sourcePart.Intersect(Item.SourcePart) ?? Item.SourcePart.Start.Span(0);
            }

            public IEnumerable<char> GetCharArray()
                => SourcePart
                    .Id
                    .ToCharArray();
        }

        internal readonly int Index;
        internal readonly Helper.Syntax Master;


        protected Item(Helper.Syntax master, int index)
        {
            Master = master;
            Index = index;
        }

        internal BinaryTree Binary => Master.FlatItem.Anchor.Items[Index];
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
        public virtual IEnumerable<SourcePart> ParserLevelGroup => null;

        public Trimmed TrimLine(SourcePart span) => new Trimmed(this, span);

        bool Equals(Item other) => SourcePart == other.SourcePart;

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
                return false;
            if(ReferenceEquals(this, obj))
                return true;
            if(obj.GetType() != GetType())
                return false;
            return Equals((Item)obj);
        }

        public override int GetHashCode() => SourcePart.GetHashCode();

        internal static Item LocateByPosition
            (Helper.Syntax target, SourcePosition offset, bool includingParent = false)
        {
            var result = target.LocateByPosition(offset, includingParent);
            result.Master.AssertIsNotNull();
            var resultToken = result.Master.FlatItem.Anchor.Items[result.Index].Token;
            var item = resultToken.PrecededWith.LastOrDefault(item => item.SourcePart.Contains(offset));
            if(item == null)
                return new Syntax(result.Master, result.Index);
            return new WhiteSpaceItem(item, result.Master, result.Index);
        }

        public static Item GetRightNeighbor(Helper.Syntax target, int current)
        {
            NotImplementedFunction(target, current);
            return default;
        }
    }
}