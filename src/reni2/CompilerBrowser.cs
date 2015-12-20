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
        readonly FunctionCache<int, Token> LocateCache;
        readonly Compiler Parent;
        readonly DataStack DataStack;

        internal CompilerBrowser(Compiler parent)
        {
            Parent = parent;
            LocateCache = new FunctionCache<int, Token>(GetLocateForCache);
            DataStack = new DataStack(Parent)
            {
                TraceCollector = new BrowseTraceCollector()
            };
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

        internal void Ensure() => Parent.Issues.ToArray();

        internal void Execute() => Parent.ExecuteFromCode(DataStack);
    }

    sealed class BrowseTraceCollector : DumpableObject, ITraceCollector
    {
        sealed class Step : DumpableObject
        {
            readonly IFormalCodeItem CodeBase;
            readonly DataStackMemento Before;
            readonly Exception RunException;
            readonly DataStackMemento After;

            public Step
                (
                IFormalCodeItem codeBase,
                DataStackMemento before,
                Exception runException,
                DataStackMemento after)
            {
                CodeBase = codeBase;
                Before = before;
                RunException = runException;
                After = after;
            }
        }

        sealed class DataStackMemento : DumpableObject
        {
            readonly string Text;
            public DataStackMemento(DataStack dataStack) { Text = dataStack.Dump(); }
        }

        readonly IList<Step> Steps = new List<Step>();

        void ITraceCollector.AssertionFailed(Func<string> dumper, int depth)
        {
            throw new AssertionFailedException(dumper());
        }

        void ITraceCollector.Run(DataStack dataStack, IFormalCodeItem codeBase)
        {
            var before = new DataStackMemento(dataStack);
            Exception runException = null;
            var beforeSize = dataStack.Size;
            try
            {
                codeBase.Visit(dataStack);
            }
            catch(Exception exception)
            {
                runException = exception;
                dataStack.Size = beforeSize + codeBase.Size;
            }

            Steps.Add(new Step(codeBase, before, runException, new DataStackMemento(dataStack)));
        }

        internal abstract class RunException : Exception
        {
            protected RunException(string message)
                : base(message) {}
        }

        internal sealed class AssertionFailedException : RunException
        {
            public AssertionFailedException(string message)
                : base(message) {}
        }
    }
}