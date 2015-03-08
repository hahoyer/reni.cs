using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.Parser;
using Reni.ReniParser;

namespace Reni.UserInterface
{
    public abstract class TokenInformation : DumpableObject
    {
        public abstract SourcePart SourcePart { get; }
        [DisableDump]
        public int StartPosition => SourcePart.Position;
        [DisableDump]
        public int Length => SourcePart.Length;
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
        public virtual bool IsError => false;
        [DisableDump]
        public int Id => ObjectId;
    }

    sealed class SyntaxToken : TokenInformation
    {
        internal SyntaxToken(Syntax syntax) { Syntax = syntax; }

        Syntax Syntax { get; }
        public override SourcePart SourcePart => Syntax.Token.Characters;
        public override bool IsKeyword => Syntax.IsKeyword;
        public override bool IsIdentifier => Syntax.IsIdentifier;
        public override bool IsText => Syntax.IsText;
        public override bool IsNumber => Syntax.IsNumber;
        public override bool IsError => Syntax.IsError;
        public override bool IsBraceLike => Syntax.IsBraceLike;
    }

    sealed class WhiteSpaceToken : TokenInformation
    {
        readonly hw.Parser.WhiteSpaceToken _item;
        public WhiteSpaceToken(hw.Parser.WhiteSpaceToken item) { _item = item; }

        public override SourcePart SourcePart => _item.Characters;
        public override bool IsComment => ReniLexer.IsComment(_item);
        public override bool IsLineComment => ReniLexer.IsLineComment(_item);
        public override bool IsWhiteSpace => ReniLexer.IsWhiteSpace(_item);
    }
}