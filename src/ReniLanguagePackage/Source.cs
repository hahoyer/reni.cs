using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Microsoft.VisualStudio.Package;
using Reni;
using Reni.UserInterface;

namespace HoyerWare.ReniLanguagePackage
{
    sealed class Source : DumpableObject
    {
        readonly ValueCache<Compiler> _compilerCache;

        public Source(string text)
        {
            _compilerCache = new ValueCache<Compiler>(() => CreateCompilerForCache(text));
        }

        static Compiler CreateCompilerForCache(string text)
            => new Compiler
                (
                text: text,
                parameters: new CompilerParameters
                {
                    TraceOptions =
                    {
                        Parser = false
                    }
                }
                );

        public TokenInfo GetTokenInfo(int line, int column)
        {
            var trace = true;
            StartMethodDump(trace, line, column);
            try
            {
                var offset = Data.FromLineAndColumn(line, column).Position;
                var token = Compiler.Token(offset);
                Dump(nameof(token), token);
                var result = token.ToTokenInfo();
                return ReturnMethodDump(result, false);
            }
            finally
            {
                EndMethodDump();
            }
        }

        Compiler Compiler => _compilerCache.Value;
        hw.Scanner.Source Data => Compiler.Source;

        IEnumerable<TokenInformation.Trimmed> TokensForLine(int lineIndex, bool trace)
        {
            var start = Data.FromLineAndColumn(lineIndex, 0).Position;
            var end = LineEndPosition(lineIndex);

            var index = start;
            var i = 0;
            while(index < end)
            {
                var token = Compiler.Token(index).AssertNotNull().Trim(start, end);
                if(trace)
                    Tracer.IndentStart();
                if(trace)
                    Tracer.Line("\n" + i + ": " + token.Token.ConvertToTokenColor());
                if(trace)
                    Tracer.Line(token.SourcePart.NodeDump.Quote());
                if(trace)
                    Tracer.Line("-----------------");
                if(trace)
                    Tracer.IndentEnd();
                yield return token;
                index += token.SourcePart.Length;
                i++;
            }
        }

        int LineEndPosition(int lineIndex)
        {
            var lineLength
                = Data.FromLineAndColumn(lineIndex + 1, 0)
                    - Data.FromLineAndColumn(lineIndex, 0);
            return Data.FromLineAndColumn(lineIndex, lineLength).Position;
        }

        internal string StateAtEndOfLine(int line, bool trace)
        {
            StartMethodDump(trace, line, trace);
            try
            {
                var lineEnd = LineEndPosition(line);
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

        internal void ColorizeLine(int line, uint[] attrs, bool trace)
        {
            TokensForLine(line, trace)
                .SelectMany(item => item.Token.SelectColors(item.GetCharArray()))
                .ToArray()
                .CopyTo(attrs, 0);
        }

        internal TokenInformation FromLineAndColumn(int lineIndex, int columIndex)
        {
            var start = Data.FromLineAndColumn(lineIndex, columIndex).Position;
            return Compiler.Token(start);
        }

        internal TokenInformation FromLineAndColumnBackwards(int lineIndex, int columIndex)
        {
            var start = Data.FromLineAndColumn(lineIndex, columIndex).Position - 1;
            return Compiler.Token(Math.Max(0, start));
        }
        public IEnumerable<SourcePart> BracesLike(TokenInformation current)
            => current.FindAllBelongings(Compiler);
    }
}