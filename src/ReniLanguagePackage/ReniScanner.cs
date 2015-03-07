using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using Reni;

namespace HoyerWare.ReniLanguagePackage
{
    sealed class ReniScanner : DumpableObject, IScanner
    {
        readonly ValueCache<Compiler> _compilerCache;

        public ReniScanner(IVsTextLines buffer)
        {
            Buffer = buffer;
            _compilerCache = new ValueCache<Compiler>(CreateCompilerForCache);
        }

        Compiler CreateCompilerForCache() => new Compiler(text: All);

        readonly List<int> _lineHash = new List<int>();

        IVsTextLines Buffer { get; }

        bool IScanner.ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int lineIndex)
        {
            EnsureValidCompiler(lineIndex);

            var tokenId = "(" + tokenInfo.StartIndex + "..." + tokenInfo.EndIndex + ")."
                + tokenInfo.Token + "i";
            var line = lineIndex + ": " + Line(lineIndex);

            var trace = lineIndex == -3 && tokenInfo.StartIndex==4;
            StartMethodDump(trace, tokenId, line);
            try
            {
                BreakExecution();
                if(tokenInfo.EndIndex + 1 >= LineLength(lineIndex))
                {
                    lineIndex++;
                    return ReturnMethodDump(false, false);
                }

                var lineStart = LinePosition(lineIndex);
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
                        LineLength(lineIndex),
                        tokenInfo.StartIndex + token.Length - 1
                    );
                tokenInfo.Color = ConvertToTokenColor(token);
                tokenInfo.Type = ConvertToTokenType(token);
                tokenInfo.Trigger = ConvertToTokenTrigger(token);
                tokenInfo.Token = token.Id;
                Dump(nameof(tokenInfo), tokenInfo);

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

            var hashCode = Line(index).GetHashCode();
            if(_lineHash[index] == hashCode)
                return;

            _lineHash[index] = hashCode;
            _compilerCache.IsValid = false;
        }

        void EnsureCorrectNumberOfLines()
        {
            var removedLines = _lineHash.Count - LineCount;
            if(removedLines > 0)
            {
                _lineHash.RemoveRange(LineCount, removedLines);
                _compilerCache.IsValid = false;
                return;
            }

            while(_lineHash.Count < LineCount)
            {
                _lineHash.Add(Line(_lineHash.Count).GetHashCode());
                _compilerCache.IsValid = false;
            }
        }

        int LineCount
        {
            get
            {
                int result;
                Buffer.GetLineCount(out result);
                return result;
            }
        }

        int LinePosition(int lineIndex)
        {
            int result;
            Buffer.GetPositionOfLine(lineIndex, out result);
            return result;
        }

        int LineIndex(int position)
        {
            int result;
            int column;
            Buffer.GetLineIndexOfPosition(position, out result, out column);
            return result;
        }

        int LineLength(int lineIndex)
        {
            int result;
            Buffer.GetLengthOfLine(lineIndex, out result);
            return result;
        }


        [DisableDump]
        string All
        {
            get
            {
                int lineCount;
                Buffer.GetLineCount(out lineCount);
                int lengthOfLastLine;
                Buffer.GetLengthOfLine(lineCount - 1, out lengthOfLastLine);
                string result;
                Buffer.GetLineText(0, 0, lineCount - 1, lengthOfLastLine, out result);
                return result;
            }
        }

        string Line(int index)
        {
            int length;
            Buffer.GetLengthOfLine(index, out length);
            string result;
            Buffer.GetLineText(index, 0, index, length, out result);
            return result;
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
            if(token.IsError)
                return TokenType.Unknown;
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
    }
}