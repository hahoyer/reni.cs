using System.Collections.Generic;
using hw.Scanner;
using Reni;
using Reni.Parser;
using Reni.TokenClasses;

namespace ReniUI.Classification
{
    sealed class WhiteSpaceToken : Token
    {
        readonly IItem _item;

        internal WhiteSpaceToken(IItem item, Syntax parent)
        {
            _item = item;
            Syntax = parent;
        }

        public override SourcePart SourcePart => _item.SourcePart;
        public override bool IsComment => Lexer.IsMultiLineComment(_item);
        public override bool IsLineComment => Lexer.IsLineComment(_item);
        public override bool IsWhiteSpace => Lexer.IsWhiteSpace(_item);
        public override bool IsLineEnd => Lexer.IsLineEnd(_item);
        public override string State => Lexer.Instance.WhiteSpaceId(_item) ?? "";

        public override Syntax Syntax {get;}

        public override IEnumerable<SourcePart> FindAllBelongings(CompilerBrowser compiler) {yield break;}
    }
}