using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Parser;
using ReniUI.Helper;

namespace ReniUI.Classification
{
    sealed class WhiteSpaceToken : Token
    {
        internal override Helper.Syntax Master { get; }
        readonly IItem Item;

        internal WhiteSpaceToken(IItem item, Helper.Syntax master)
        {
            Item = item;
            Master = master;
        }

        public override SourcePart SourcePart => Item.SourcePart;
        public override bool IsComment => Lexer.IsMultiLineComment(Item);
        public override bool IsLineComment => Lexer.IsLineComment(Item);
        public override bool IsWhiteSpace => Lexer.IsSpace(Item);
        public override bool IsLineEnd => Lexer.IsLineEnd(Item);
        public override string State => Lexer.Instance.WhiteSpaceId(Item) ?? "";

        [DisableDump]
        public override IEnumerable<SourcePart> ParserLevelBelongings
        {
            get { yield break; }
        }
    }
}