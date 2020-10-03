using System.Collections.Generic;
using hw.Scanner;
using Reni.Parser;
using ReniUI.Helper;

namespace ReniUI.Classification
{
    sealed class WhiteSpaceToken : Token
    {
        readonly IItem _item;

        internal WhiteSpaceToken(IItem item, Helper.Syntax parent)
        {
            _item = item;
            Syntax = parent;
        }

        public override SourcePart SourcePart => _item.SourcePart;
        public override bool IsComment => Lexer.IsMultiLineComment(_item);
        public override bool IsLineComment => Lexer.IsLineComment(_item);
        public override bool IsWhiteSpace => Lexer.IsSpace(_item);
        public override bool IsLineEnd => Lexer.IsLineEnd(_item);
        public override string State => Lexer.Instance.WhiteSpaceId(_item) ?? "";

        internal override Helper.Syntax Syntax {get;}

        public override IEnumerable<SourcePart> FindAllBelongings(CompilerBrowser compiler) {yield break;}
    }
}