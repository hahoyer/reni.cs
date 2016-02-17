using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
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
        readonly IDictionary<IFormalCodeItem, int> CodeToFunctionIndexCache =
            new Dictionary<IFormalCodeItem, int>();
        readonly FunctionCache<int, Token> LocateCache;
        readonly Compiler Parent;

        internal CompilerBrowser(Compiler parent)
        {
            Parent = parent;
            LocateCache = new FunctionCache<int, Token>(GetLocateForCache);
        }

        public Source Source => Parent.Source;
        internal IExecutionContext ExecutionContext => Parent;

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

        internal FunctionType Function(int index)
            => Parent.Root.Function(index);


        internal object FindFunction(IFormalCodeItem codeBase)
        {
            var result = FindFunctionIndex(codeBase);
            return result == null
                ? (object) Parent.CodeContainer.Main.Data
                : Function(result.Value);
        }

        internal int? FindFunctionIndex(IFormalCodeItem codeBase)
        {
            int result;
            if(CodeToFunctionIndexCache.TryGetValue(codeBase, out result))
                return result;

            var results = Parent
                .Root
                .FunctionCount
                .Select()
                .Where(item => !CodeToFunctionIndexCache.Values.Contains(item));

            foreach(var index in results)
            {
                var codeItems = Function(index).CodeItems;
                foreach(var item in codeItems)
                    CodeToFunctionIndexCache.Add(item, index);
                if(codeItems.Contains(codeBase))
                    return index;
            }

            return null;
        }

        internal IEnumerable<SourceSyntax> FindAllBelongings(SourceSyntax sourceSyntax)
            => Parent.SourceSyntax.Belongings(sourceSyntax);

        public string Reformat(SourcePart sourcePart, HierachicalFormatter hierachicalFormatter)
            => Parent.SourceSyntax.Reformat(sourcePart, hierachicalFormatter);

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

        internal string Reformat(HierachicalFormatter hierachicalFormatter)
            => Parent.SourceSyntax.Reformat(provider: hierachicalFormatter);

        internal SourceSyntax Locate(SourcePart span)
        {
            var result = Parent.SourceSyntax.Locate(span);
            if(result != null)
                return result;

            Tracer.TraceBreak();
            NotImplementedMethod(span);
            return null;
        }

        internal void Ensure() => Parent.Issues.ToArray();

        internal void Execute(DataStack dataStack) => Parent.ExecuteFromCode(dataStack);
    }
}