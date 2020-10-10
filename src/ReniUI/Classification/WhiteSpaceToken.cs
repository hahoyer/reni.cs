using System.Collections.Generic;
using hw.Scanner;
using Reni.Parser;

namespace ReniUI.Classification
{
    sealed class WhiteSpaceToken : Token
    {
        internal override Helper.Syntax Syntax { get; }
        readonly IItem Item;

        internal WhiteSpaceToken(IItem item, Helper.Syntax parent)
        {
            Item = item;
            Syntax = parent;
        }

        public override SourcePart SourcePart => Item.SourcePart;
        public override bool IsComment => Lexer.IsMultiLineComment(Item);
        public override bool IsLineComment => Lexer.IsLineComment(Item);
        public override bool IsWhiteSpace => Lexer.IsSpace(Item);
        public override bool IsLineEnd => Lexer.IsLineEnd(Item);
        public override string State => Lexer.Instance.WhiteSpaceId(Item) ?? "";

        public override IEnumerable<SourcePart> FindAllBelongings(CompilerBrowser compiler) { yield break; }
    }
}