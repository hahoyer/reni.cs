using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Parser;

namespace Reni.UserInterface
{
    sealed class WhiteSpaceToken : TokenInformation
    {
        readonly hw.Parser.WhiteSpaceToken _item;
        public WhiteSpaceToken(hw.Parser.WhiteSpaceToken item) { _item = item; }

        public override SourcePart SourcePart => _item.Characters;
        public override bool IsComment => Lexer.IsComment(_item);
        public override bool IsLineComment => Lexer.IsLineComment(_item);
        public override bool IsWhiteSpace => Lexer.IsWhiteSpace(_item);
        public override bool IsLineEnd => Lexer.IsLineEnd(_item);
        public override string State => Lexer.Instance.WhiteSpaceId(_item) ?? "";

        public override IEnumerable<SourcePart> FindAllBelongings(Compiler compiler)
        {
            yield break;
        }

        public override string Reformat(SourcePart targetPart)
            => SourcePart.Intersect(targetPart).Id;
    }
}