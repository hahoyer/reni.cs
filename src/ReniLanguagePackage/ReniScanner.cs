using System;
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
    sealed class ReniScanner : DumpableObject, IScanner
    {
        readonly ValueCache<Compiler> _compilerCache;

        public ReniScanner(IVsTextLines buffer)
        {
            Buffer = new VsTextLinesWrapper(buffer);
            _compilerCache = new ValueCache<Compiler>(CreateCompilerForCache);
        }

        Compiler CreateCompilerForCache() => new Compiler(text: Buffer.All);

        readonly List<int> _lineHash = new List<int>();

        VsTextLinesWrapper Buffer { get; }

        bool IScanner.ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int lineIndex)
        {
            EnsureValidCompiler(lineIndex);

            var tokenId = "(" + tokenInfo.StartIndex + "..." + tokenInfo.EndIndex + ")."
                + tokenInfo.Token + "i";
            var line = lineIndex + ": " + Buffer.Line(lineIndex);

            var trace = lineIndex >= 3;
            StartMethodDump(trace, tokenId, line);
            try
            {
                if(tokenInfo.EndIndex + 1 >= Buffer.LineLength(lineIndex))
                {
                    lineIndex++;
                    return ReturnMethodDump(false, false);
                }

                var lineStart = Buffer.LinePosition(lineIndex);
                var position = lineStart + tokenInfo.EndIndex + 1;
                var token = _compilerCache.Value.Token(position);
                if(token == null)
                {
                    NotImplementedMethod(tokenId, line);
                    return false;
                }

                Dump(nameof(token), token);
                tokenInfo.StartIndex = token.StartPosition - lineStart;
                tokenInfo.EndIndex = Math.Min
                    (
                        Buffer.LineLength(lineIndex),
                        tokenInfo.StartIndex + token.Length - 1
                    );
                tokenInfo.Color = ConvertToTokenColor(token);
                tokenInfo.Type = ConvertToTokenType(token);
                tokenInfo.Trigger = ConvertToTokenTrigger(token);
                tokenInfo.Token = token.Id;
                Dump(nameof(tokenInfo), tokenInfo);

                BreakExecution();
                return ReturnMethodDump(true, false);
            }
            finally
            {
                EndMethodDump();
            }
        }

        void EnsureValidCompiler(int index)
        {
            EnsureCorrectNumberOfLines();

            var hashCode = Buffer.Line(index).GetHashCode();
            if(_lineHash[index] == hashCode)
                return;

            _lineHash[index] = hashCode;
            _compilerCache.IsValid = false;
        }

        void EnsureCorrectNumberOfLines()
        {
            var removedLines = _lineHash.Count - Buffer.LineCount;
            if(removedLines > 0)
            {
                _lineHash.RemoveRange(Buffer.LineCount, removedLines);
                _compilerCache.IsValid = false;
                return;
            }

            while(_lineHash.Count < Buffer.LineCount)
            {
                _lineHash.Add(Buffer.Line(_lineHash.Count).GetHashCode());
                _compilerCache.IsValid = false;
            }
        }

        void IScanner.SetSource(string source, int offset)
        {
            Tracer.Assert
                (
                    offset == 0,
                    () =>
                        "SetSource: " +
                            nameof(source) +
                            " =" +
                            source.Quote() +
                            " " +
                            nameof(offset) +
                            "=" +
                            offset
                );
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