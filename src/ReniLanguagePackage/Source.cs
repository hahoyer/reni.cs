﻿using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
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
            var end = Data.FromLineAndColumn(lineIndex, null).Position;

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

        internal string StateAtEndOfLine(int line, bool trace)
        {
            StartMethodDump(trace, line, trace);
            try
            {
                var lineEndPosition = Data.FromLineAndColumn(line, null).Position;
                var token = Compiler.Token(lineEndPosition);
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
            var array = TokensForLine(line, trace)
                .SelectMany(item => item.Token.SelectColors(item.GetCharArray()))
                .ToArray();

            Tracer.Assert(array.Length <= attrs.Length);
            array
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

        internal IEnumerable<SourcePart> BracesLike(TokenInformation current)
            => current.FindAllBelongings(Compiler);

        internal void ReformatSpan(EditArray mgr, TextSpan span)
        {
            var sourcePart = Data.ToSourcePart(span);
            var start = Compiler.Token(sourcePart.Position);
            var end = Compiler.Token(sourcePart.EndPosition - 1);
            var common = Compiler.Token(start.SourcePart + end.SourcePart);
            mgr.Add(new EditSpan(common.SourcePart.ToTextSpan(), common.Reformat));
            mgr.ApplyEdits();
        }
    }
}