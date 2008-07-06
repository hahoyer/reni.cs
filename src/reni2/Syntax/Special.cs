using Reni.Context;
using Reni.Parser;
using Reni.Parser.TokenClass;

namespace Reni.Syntax
{
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
