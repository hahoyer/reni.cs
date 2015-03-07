using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using Reni;

namespace HoyerWare.ReniLanguagePackage
{
    sealed class ReniScanner : DumpableObject, IScanner
    {
        Compiler _compiler;
        int _start;
        public ReniScanner(IVsTextLines buffer) { Buffer = buffer; }

        [EnableDump]
        SourcePosn Start => _compiler.Source + _start;   

        IVsTextLines Buffer { get; }

        bool IScanner.ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int state)
        {
            if(tokenInfo == null || _start >= _compiler.Source.Length)
                return false;

            var token = _compiler.Token(_start);
            if(token == null)
            {
                NotImplementedMethod(tokenInfo, state);
                return false;
            }

            tokenInfo.StartIndex = token.Start - (_compiler.Source + 0);
            var endIndex = token.End - (_compiler.Source + 0);
            tokenInfo.EndIndex = endIndex;
            _start = endIndex;

            tokenInfo.Color = ConvertToTokenColor(token);
            tokenInfo.Type = ConvertToTokenType(token);
            tokenInfo.Trigger = ConvertToTokenTrigger(token);

            return true;
        }

        static TokenTriggers ConvertToTokenTrigger(Token token)
        {
            var result = TokenTriggers.None;
            if(token.IsBraceLike)
                result |= TokenTriggers.MatchBraces;
            return result;
        }

        static TokenType ConvertToTokenType(Token token)
        {
            if(token.IsText)
                return TokenType.String;
            if(token.IsNumber)
                return TokenType.Literal;
            if(token.IsKeyword)
                return TokenType.Keyword;
            if(token.IsIdentifier)
                return TokenType.Identifier;
            if(token.IsComment)
                return token.IsLineComment ? TokenType.LineComment : TokenType.Comment;
            if(token.IsWhiteSpace)
                return TokenType.WhiteSpace;
            return TokenType.Text;
        }

        static TokenColor ConvertToTokenColor(Token token)
        {
            if(token.IsText)
                return TokenColor.String;
            if(token.IsNumber)
                return TokenColor.Number;
            if(token.IsKeyword)
                return TokenColor.Keyword;
            if(token.IsIdentifier)
                return TokenColor.Identifier;
            if(token.IsComment)
                return TokenColor.Comment;
            return TokenColor.Text;
        }

        void IScanner.SetSource(string source, int offset)
        {
            _compiler = new Compiler(text: source);
            _start = 0;
        }
    }
}