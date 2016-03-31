using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using Reni;
using ReniUI;
using ReniUI.Classification;
using ReniUI.Formatting;

namespace HoyerWare.ReniLanguagePackage
{
    sealed class Source : DumpableObject
    {
        readonly ValueCache<CompilerBrowser> _compilerCache;

        public Source(string text)
        {
            _compilerCache = new ValueCache<CompilerBrowser>(() => CreateCompilerForCache(text));
        }

        static CompilerBrowser CreateCompilerForCache(string text)
            => CompilerBrowser.FromText
                (
                    text,
                    new CompilerParameters
                    {
                        TraceOptions =
                        {
                            Parser = false
                        }
                    }
                );

        public TokenInfo GetTokenInfo(int line, int column)
        {
            var trace = false;
            StartMethodDump(trace, line, column);
            try
            {
                var offset = Data.FromLineAndColumn(line, column).Position;
                var token = Compiler.LocatePosition(offset);
                Dump(nameof(token), token);
                var result = token.ToTokenInfo();
                return ReturnMethodDump(result, false);
            }
            finally
            {
                EndMethodDump();
            }
        }

        CompilerBrowser Compiler => _compilerCache.Value;
        hw.Scanner.Source Data => Compiler.Source;

        IEnumerable<Token.Trimmed> TokensForLine(int lineIndex, bool trace)
        {
            var line = Data.Line(lineIndex);

            var index = line.Position;
            var i = 0;
            while(index < line.EndPosition)
            {
                var token = Compiler.LocatePosition(index).AssertNotNull().TrimLine(line);
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

        internal string StateAtEndOfLine(int line, bool trace)
        {
            StartMethodDump(trace, line, trace);
            try
            {
                var lineEndPosition = Data.FromLineAndColumn(line, null).Position;
                var token = Compiler.LocatePosition(lineEndPosition);
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

        internal Token FromLineAndColumn(int lineIndex, int columIndex)
        {
            var start = Data.FromLineAndColumn(lineIndex, columIndex).Position;
            return Compiler.LocatePosition(start);
        }

        internal Token FromLineAndColumnBackwards(int lineIndex, int columIndex)
        {
            var start = Data.FromLineAndColumn(lineIndex, columIndex).Position - 1;
            return Compiler.LocatePosition(Math.Max(0, start));
        }

        internal IEnumerable<SourcePart> BracesLike(Token current)
            => Compiler.FindAllBelongings(current);

        internal void ReformatSpan(EditArray mgr, TextSpan span, IFormatter provider)
        {
            var sourcePart = Data.ToSourcePart(span);
            var reformat = Compiler.Locate(sourcePart).Reformat(sourcePart, provider);
            mgr.Add(new EditSpan(span, reformat));
            mgr.ApplyEdits();
        }
    }
}