using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using hw.Scanner;
using Reni.Code;
using Reni.Formatting;
using Reni.Parser;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.UserInterface;

namespace Reni
{
    public sealed class CompilerBrowser : DumpableObject
    {
        readonly FunctionCache<int, Token> LocateCache;
        readonly Compiler Parent;

        internal CompilerBrowser(Compiler parent)
        {
            Parent = parent;
            LocateCache = new FunctionCache<int, Token>(GetLocateForCache);
        }

        public Source Source => Parent.Source;

        internal IEnumerable<CompileSyntax> FindPosition(int p)
        {
            var enumerable = LocateCache[p]
                .SourceSyntax
                .ParentChainIncludingThis
                .Select(item => item.Syntax)
                .ToArray();

            var compileSyntaxs = enumerable
                .OfType<CompileSyntax>()
                .Where(item => item.ResultCache.Any());
            return compileSyntaxs;
        }

        public Token LocatePosition(int current) => LocateCache[current];

        internal FunctionType FindFunction(int index)
            => Parent.RootContext.Function(index);

        internal IEnumerable<SourceSyntax> FindAllBelongings(SourceSyntax sourceSyntax)
            => Parent.SourceSyntax.Belongings(sourceSyntax);

        public string Reformat(SourcePart sourcePart, Provider provider)
            => Parent.SourceSyntax.Reformat(sourcePart, provider);

        Token GetLocateForCache(int offset)
        {
            var posn = Source + offset;
            var sourcePart = posn.Span(posn.IsEnd ? 0 : 1);
            var sourceSyntax = Parent.SourceSyntax.Locate(sourcePart);

            if(posn < sourceSyntax.Token.Characters)
                return new WhiteSpaceToken
                    (
                    sourceSyntax.Token.PrecededWith.Single(item => item.Characters.Contains(posn)),
                    sourceSyntax
                    );

            return new SyntaxToken(sourceSyntax);
        }

        internal string Reformat(Provider provider)
            => Parent.SourceSyntax.Reformat(provider: provider);

        internal SourceSyntax Locate(SourcePart span)
        {
            var result = Parent.SourceSyntax.Locate(span);
            if(result != null)
                return result;

            Tracer.TraceBreak();
            NotImplementedMethod(span);
            return null;
        }

        public void Ensure() => Parent.Issues.ToArray();
    }
}