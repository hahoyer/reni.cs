using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using Reni;
using ReniUI;
using ReniUI.Classification;
using ReniUI.Formatting;

namespace ReniVSIX
{
    sealed class Source : DumpableObject
    {
        readonly ValueCache<CompilerBrowser> CompilerCache;

        public Source(string text)
            => CompilerCache = new ValueCache<CompilerBrowser>(() => CreateCompilerForCache(text));

        CompilerBrowser Compiler => CompilerCache.Value;
        hw.Scanner.Source Data => Compiler.Source;

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
                    , ProcessErrors = true
                }
            );

        public TokenInfo GetTokenInfo(int line, int column)
        {
            var trace = false;
            StartMethodDump(trace, line, column);
            try
            {
                var offset = Data.FromLineAndColumn(line, column).Position;
                var token = Compiler.Locate(offset);
                Dump(nameof(token), token);
                var result = token.ToTokenInfo();
                return ReturnMethodDump(result, false);
            }
            finally
            {
                EndMethodDump();
            }
        }

        IEnumerable<Item.Trimmed> TokensForLine(int lineIndex, bool trace)
        {
            var line = Data.Line(lineIndex);

            var index = line.Position;
            var i = 0;
            while(index < line.EndPosition)
            {
                var token = Compiler.Locate(index).AssertNotNull().TrimLine(line);
                if(trace)
                    Tracer.IndentStart();
                if(trace)
                    ("\n" + i + ": " + token.Item.ConvertToTokenColor()).Log();
                if(trace)
                    token.SourcePart.NodeDump.Quote().Log();
                if(trace)
                    "-----------------".Log();
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
                var token = Compiler.Locate(lineEndPosition);
                (token != null).Assert();
                var result = token.State;
                return ReturnMethodDump(result, false);
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal void ReformatSpan(EditArray mgr, TextSpan span, IFormatter provider)
        {
            var sourcePart = Data.ToSourcePart(span);
            var reformat = provider
                .GetEditPieces(Compiler, sourcePart)
                .OrderByDescending(p => p.Location.EndPosition)
                .ToArray();

            var ns = reformat.Select(r => r.Location.Position).ToArray();

            foreach(var edit in reformat)
            {
                var editSpan = new EditSpan
                (
                    edit.Location.ToTextSpan(),
                    edit.NewText
                );
                mgr.Add(editSpan);
            }

            mgr.ApplyEdits();
        }
    }
}