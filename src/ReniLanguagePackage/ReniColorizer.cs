using System;
using System.Collections;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using Reni;
using Reni.UserInterface;

namespace HoyerWare.ReniLanguagePackage
{
    sealed class ReniColorizer : DumpableObject
    {
        readonly ValueCache<Compiler> _compilerCache;
        TokenInfo[] cachedLineInfo;

        public ReniColorizer(IVsTextLines buffer)
        {
            Buffer = new VsTextLinesWrapper(buffer);
            _compilerCache = new ValueCache<Compiler>(CreateCompilerForCache);
        }

        VsTextLinesWrapper Buffer { get; }

        internal static InnerTokenStateAtLineEnd StartState => InnerTokenStateAtLineEnd.None;


        Compiler CreateCompilerForCache() => new Compiler(text: Buffer.All);

        Compiler Compiler
        {
            get
            {
                if(_compilerCache.IsValid && Buffer.All != _compilerCache.Value.Source.Data)
                    _compilerCache.IsValid = false;
                return _compilerCache.Value;
            }
        }

        public InnerTokenStateAtLineEnd StateAtEndOfLine(int line)
        {
            var trace = line > -3;
            StartMethodDump(trace, line);
            try
            {
                var token = Compiler.Token(Buffer.LineEnd(line));
                Tracer.Assert(token != null);
                var result = token.State;
                return ReturnMethodDump(result, false);
            }
            finally
            {
                EndMethodDump();
            }
        }

        public void ColorizeLine(int line, uint[] attrs)
        {
            TokensForLine(line)
                .SelectMany(item => SelectColors(item.GetCharArray(), item.Token))
                .ToArray()
                .CopyTo(attrs, 0);
        }

        static IEnumerable<uint> SelectColors(IEnumerable<char> toCharArray, TokenInformation token)
        {
            return toCharArray
                .Select(c => (uint) ConvertToTokenColor(token));
        }

        IEnumerable<TokenInformation.Trimmed> TokensForLine(int lineIndex)
        {
            var start = Buffer.LinePosition(lineIndex);
            var end = Buffer.LineEnd(lineIndex);
            var index = start;
            while(index < end)
            {
                var token = Compiler.Token(index).AssertNotNull().Trim(start, end);
                Tracer.Line(token.SourcePart.NodeDump.Quote() + " " + ConvertToTokenType(token.Token));
                yield return token;
                index += token.SourcePart.Length;
            }
        }

        static TokenTriggers ConvertToTokenTrigger(TokenInformation token)
        {
            var result = TokenTriggers.None;
            if(token.IsBraceLike)
                result |= TokenTriggers.MatchBraces;
            return result;
        }

        static TokenType ConvertToTokenType(TokenInformation token)
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
            if(token.IsError)
                return TokenType.Unknown;
            return TokenType.Text;
        }

        static TokenColor ConvertToTokenColor(TokenInformation token)
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

    }
}