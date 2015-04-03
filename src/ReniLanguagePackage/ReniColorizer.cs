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
    sealed class ReniColorizer : DumpableObject
    {
        readonly ValueCache<Compiler> _compilerCache;

        public ReniColorizer(IVsTextLines buffer)
        {
            Buffer = new VsTextLinesWrapper(buffer);
            _compilerCache = new ValueCache<Compiler>(CreateCompilerForCache);
        }

        VsTextLinesWrapper Buffer { get; }

        internal static string StartState => "";


        Compiler CreateCompilerForCache() => new Compiler
            (
            text: Buffer.All,
            parameters: new CompilerParameters
            {
                TraceOptions =
                {
                    Parser = false
                }
            }
            );

        Compiler Compiler
        {
            get
            {
                if(_compilerCache.IsValid && Buffer.All != _compilerCache.Value.Source.Data)
                    _compilerCache.IsValid = false;
                return _compilerCache.Value;
            }
        }

        public string  StateAtEndOfLine(int line)
        {
            var trace = line == -3;
            StartMethodDump(trace, line);
            try
            {
                var lineEnd = Buffer.LineEnd(line);
                var token = Compiler.Token(lineEnd);
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
            TokensForLine(line, trace:false)
                .SelectMany(item => SelectColors(item.GetCharArray(), item.Token))
                .ToArray()
                .CopyTo(attrs, 0);
        }

        static IEnumerable<uint> SelectColors(IEnumerable<char> toCharArray, TokenInformation token)
        {
            return toCharArray
                .Select(c => (uint) ConvertToTokenColor(token));
        }

        IEnumerable<TokenInformation.Trimmed> TokensForLine(int lineIndex, bool trace)
        {
            var start = Buffer.LinePosition(lineIndex);
            var end = Buffer.LineEnd(lineIndex);
            var index = start;
            var i = 0;
            while(index < end)
            {
                var token = Compiler.Token(index).AssertNotNull().Trim(start, end);
                if(trace)Tracer.IndentStart();
                if (trace) Tracer.Line("\n" + i + ": " + ConvertToTokenColor(token.Token));
                if (trace) Tracer.Line(token.SourcePart.NodeDump.Quote());
                if (trace) Tracer.Line("-----------------");
                if (trace) Tracer.IndentEnd();
                yield return token;
                index += token.SourcePart.Length;
                i++;
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
            if(token.IsComment)
                return TokenType.Comment;
            if(token.IsLineComment)
                return TokenType.LineComment;
            if(token.IsWhiteSpace)
                return TokenType.WhiteSpace;
            if(token.IsError)
                return TokenType.Unknown;
            if(token.IsText)
                return TokenType.String;
            if(token.IsNumber)
                return TokenType.Literal;
            if(token.IsKeyword)
                return TokenType.Keyword;
            if(token.IsIdentifier)
                return TokenType.Identifier;
            return TokenType.Text;
        }

        static TokenColor ConvertToTokenColor(TokenInformation token)
        {
            if(token.IsComment || token.IsLineComment)
                return TokenColor.Comment;
            if(token.IsError)
                return TokenColor.Text;
            if(token.IsText)
                return TokenColor.String;
            if(token.IsNumber)
                return TokenColor.Number;
            if(token.IsKeyword)
                return TokenColor.Keyword;
            if(token.IsIdentifier)
                return TokenColor.Identifier;

            return TokenColor.Text;
        }
    }
}