using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Parser;
using Reni.Parser.TokenClass;

namespace Reni.Syntax
{
    internal class CompileSyntax : ParsedSyntax, ICompileSyntax
    {
        internal CompileSyntax(Token token)
            : base(token) {}

        string ICompileSyntax.DumpShort()
        {
            return DumpShort();
        }

        string ICompileSyntax.FilePosition()
        {
            return FilePosition();
        }

        Result ICompileSyntax.Result(ContextBase context, Category category)
        {
            var trace = ObjectId >= 0;
            StartMethodDump(trace, context,category);
            if(category.HasInternal || !(category.HasCode || category.HasRefs))
                return ReturnMethodDump(trace, Result(context, category));
            var result = Result(context, category | Category.Internal | Category.Type);
            Dump(trace, "result", result);
            return ReturnMethodDump(trace, result.CreateStatement(category, result.Internal));
        }

        internal protected virtual Result Result(ContextBase context, Category category)
        {
            NotImplementedMethod(context, category);
            return null;
        }

        internal protected override IParsedSyntax SurroundedByParenthesis(Token token)
        {
            return this;
        }

    }

    internal abstract class SpecialSyntax : CompileSyntax
    {
        protected SpecialSyntax(Token token)
            : base(token) {}
    }

    internal sealed class TerminalSyntax : SpecialSyntax
    {
        internal readonly Terminal Terminal;

        public TerminalSyntax(Token token, Terminal terminal)
            : base(token)
        {
            Terminal = terminal;
        }

        internal protected override Result Result(ContextBase context, Category category)
        {
            return Terminal.Result(context, category, Token);
        }
    }

    internal sealed class PrefixSyntax : SpecialSyntax
    {
        internal readonly Prefix Prefix;
        internal readonly ICompileSyntax Right;

        public PrefixSyntax(Token token, Prefix prefix, ICompileSyntax right)
            : base(token)
        {
            Prefix = prefix;
            Right = right;
        }

        internal protected override Result Result(ContextBase context, Category category)
        {
            return Prefix.Result(context, category, Token, Right);
        }
    }

    internal sealed class InfixSyntax : SpecialSyntax
    {
        internal readonly ICompileSyntax Left;
        internal readonly Infix Infix;
        internal readonly ICompileSyntax Right;

        public InfixSyntax(Token token, ICompileSyntax left, Infix infix, ICompileSyntax right) : base(token)
        {
            Left = left;
            Infix = infix;
            Right = right;
        }

        internal protected override Result Result(ContextBase context, Category category)
        {
            return Infix.Result(context, category, Left, Token, Right);
        }
    }
}
