using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;
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
        public override bool IsComment => Lexer.IsComment(_item);
        public override bool IsLineComment => Lexer.IsLineComment(_item);
        public override bool IsWhiteSpace => Lexer.IsWhiteSpace(_item);
        public override bool IsLineEnd => Lexer.IsLineEnd(_item);
        public override string State => Lexer.Instance.WhiteSpaceId(_item) ?? "";

        public override IEnumerable<SourcePart> FindAllBelongings(CompilerBrowser compiler)
        {
            yield break;
        }

        public override Token LocatePosition(int current)
        {
            NotImplementedMethod(current);
            return null;
        }

        public override Syntax Syntax { get; }

        public override string Reformat(SourcePart targetPart)
            => SourcePart.Intersect(targetPart)?.Id ?? "";
    }
}