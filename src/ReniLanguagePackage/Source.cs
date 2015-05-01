﻿using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
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
                var token = Compiler.Containing(offset);
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
            var line = Data.Line(lineIndex);

            var index = line.Position;
            var i = 0;
            while(index < line.EndPosition)
            {
                var token = Compiler.Containing(index).AssertNotNull().Trim(line);
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
                var token = Compiler.Containing(lineEndPosition);
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
            return Compiler.Containing(start);
        }

        internal TokenInformation FromLineAndColumnBackwards(int lineIndex, int columIndex)
        {
            var start = Data.FromLineAndColumn(lineIndex, columIndex).Position - 1;
            return Compiler.Containing(Math.Max(0, start));
        }

        internal IEnumerable<SourcePart> BracesLike(TokenInformation current)
            => current.FindAllBelongings(Compiler);

        internal void ReformatSpan(EditArray mgr, TextSpan span)
        {
            var sourcePart = Data.ToSourcePart(span);
            var common = Compiler.Containing(sourcePart)
                .Trim(sourcePart);
            mgr.Add(new EditSpan(span, common.Reformat));
            mgr.ApplyEdits();
        }
    }
}