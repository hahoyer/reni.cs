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
        readonly hw.Scanner.Source _source;

        public Source(string text)
        {
            _source = new hw.Scanner.Source(text);
            _compilerCache = new ValueCache<Compiler>(CreateCompilerForCache);
        }

        Compiler CreateCompilerForCache()
            => new Compiler
                (
                source: _source,
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
                var offset = _source.FromLineAndColumn(line, column).Position;
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

        IEnumerable<TokenInformation.Trimmed> TokensForLine(int lineIndex, bool trace)
        {
            var start = _source.FromLineAndColumn(lineIndex, 0).Position;
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
                = _source.FromLineAndColumn(lineIndex + 1, 0)
                    - _source.FromLineAndColumn(lineIndex, 0);
            return _source.FromLineAndColumn(lineIndex, lineLength).Position;
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
            var start = _source.FromLineAndColumn(lineIndex, columIndex).Position;
            return Compiler.Token(start);
        }

        internal TokenInformation FromLineAndColumnBackwards(int lineIndex, int columIndex)
        {
            var start = _source.FromLineAndColumn(lineIndex, columIndex).Position - 1;
            return Compiler.Token(Math.Max(0, start));
        }
        public IEnumerable<SourcePart> BracesLike(TokenInformation current)
            => current.FindAllBelongings(Compiler);
    }
}